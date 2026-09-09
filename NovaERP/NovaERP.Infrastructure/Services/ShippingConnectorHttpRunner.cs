using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Services;

/// <summary>Builds the outbound request JSON for one ShippingConnector from its Outbound field
/// mappings, calls the connector's configured HTTP endpoint, and parses the response into
/// normalized RateQuoteDtos via the Inbound mappings — entirely data-driven, no per-TMS code.
/// Never throws — mirrors HttpSmsSender's "return a typed result, don't propagate exceptions"
/// discipline for external HTTP integrations.</summary>
public class ShippingConnectorHttpRunner : IShippingConnectorRunner
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ShippingConnectorHttpRunner(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<ConnectorRunResultDto> RunAsync(ShippingConnector connector, Shipment shipment, CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(nameof(ShippingConnectorHttpRunner));
            var method = string.Equals(connector.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase) ? HttpMethod.Get : HttpMethod.Post;
            using var request = new HttpRequestMessage(method, connector.BaseUrl);

            await ApplyAuthAsync(request, connector, client, ct);

            if (method == HttpMethod.Post)
                request.Content = BuildRequestContent(connector, shipment);

            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                return new ConnectorRunResultDto { Error = $"Connector returned {(int)response.StatusCode}: {errorBody}" };
            }

            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (connector.ResponseFormat == "Xml")
            {
                var root = XDocument.Parse(responseBody).Root!;
                var xmlError = TryGetXmlErrorEnvelope(root);
                if (xmlError is not null) return new ConnectorRunResultDto { Error = xmlError };
                return new ConnectorRunResultDto { Quotes = ParseQuotesXml(connector, root) };
            }

            return new ConnectorRunResultDto { Quotes = ParseQuotes(connector, JsonDocument.Parse(responseBody).RootElement) };
        }
        catch (Exception ex)
        {
            return new ConnectorRunResultDto { Error = ex.Message };
        }
    }

    /// <summary>Some XML rate-quote APIs signal a business-level failure (bad date, missing
    /// field, account issue) as HTTP 200 with an error envelope instead of a non-2xx status —
    /// confirmed against CarrierLogistics' real API, whose response root is literally
    /// &lt;error&gt;&lt;errormessage&gt;...&lt;/errormessage&gt;&lt;/error&gt; on a 200. Without
    /// this check, that response's root simply doesn't match any Inbound mapping's path and the
    /// connector would silently return zero quotes with zero errors — indistinguishable from
    /// "no rates available for this lane." Returns null (not an error envelope) for every
    /// unrelated XML shape, including this carrier's own real success shape.</summary>
    private static string? TryGetXmlErrorEnvelope(XElement root)
    {
        var rootName = root.Name.LocalName;
        if (!string.Equals(rootName, "error", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(rootName, "errors", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(rootName, "fault", StringComparison.OrdinalIgnoreCase))
            return null;

        var messageElement = root.Descendants().FirstOrDefault(e =>
            string.Equals(e.Name.LocalName, "errormessage", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.Name.LocalName, "message", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.Name.LocalName, "description", StringComparison.OrdinalIgnoreCase));

        var message = messageElement?.Value.Trim();
        return string.IsNullOrWhiteSpace(message) ? root.Value.Trim() : message;
    }

    /// <summary>Builds the outbound HttpContent per RequestContentType. Json/FormUrlEncoded are
    /// mapping-driven (built from Outbound FieldMappings); Xml/Text send the Sample Request
    /// Payload verbatim — no mapping-driven XML/text templating in this pass, since the concrete
    /// need so far (legacy form-urlencoded rate-quote APIs) only requires Json and
    /// FormUrlEncoded outbound building.</summary>
    private static HttpContent BuildRequestContent(ShippingConnector connector, Shipment shipment) => connector.RequestContentType switch
    {
        "FormUrlEncoded" => new FormUrlEncodedContent(BuildFormFields(connector, shipment)),
        "Xml" => new StringContent(connector.SampleRequestPayload ?? string.Empty, Encoding.UTF8, "application/xml"),
        "Text" => new StringContent(connector.SampleRequestPayload ?? string.Empty, Encoding.UTF8, "text/plain"),
        _ => new StringContent(BuildRequestBody(connector, shipment).ToJsonString(), Encoding.UTF8, "application/json")
    };

    private static JsonObject BuildRequestBody(ShippingConnector connector, Shipment shipment)
    {
        var root = new JsonObject();
        var packages = shipment.Packages.ToList();

        foreach (var mapping in connector.FieldMappings.Where(m => m.Direction == "Outbound"))
        {
            // Any mapping whose External Path contains "[]" repeats once per package — this
            // covers real per-package data (Package.WeightKg etc.) AND a fixed value some
            // carriers require repeated on every package line (e.g. a constant freight class),
            // built the same way in the UI (pick a detected "packages[].xyz" path). Only
            // Package.* mappings actually read a different value per package; Constant/scalar
            // mappings just resolve the same value into each package's slot.
            if (mapping.ExternalPath.Contains("[]", StringComparison.Ordinal))
            {
                var propertyName = mapping.NovaField.StartsWith("Package.", StringComparison.Ordinal)
                    ? mapping.NovaField["Package.".Length..]
                    : null;

                for (var i = 0; i < packages.Count; i++)
                {
                    var raw = propertyName is not null
                        ? GetPackageValue(packages[i], propertyName)
                        : mapping.NovaField == "Constant"
                            ? mapping.ConstantValue
                            : GetScalarValue(shipment, mapping.NovaField);
                    var transformed = ApplyTransform(raw, mapping.Transform);
                    var concretePath = ResolveIndex(mapping.ExternalPath, i);
                    JsonPathWalker.SetValue(root, concretePath, ToJsonNode(transformed));
                }
            }
            else if (mapping.NovaField == "Constant")
            {
                var transformed = ApplyTransform(mapping.ConstantValue, mapping.Transform);
                JsonPathWalker.SetValue(root, mapping.ExternalPath, ToJsonNode(transformed));
            }
            else
            {
                var raw = GetScalarValue(shipment, mapping.NovaField);
                var transformed = ApplyTransform(raw, mapping.Transform);
                JsonPathWalker.SetValue(root, mapping.ExternalPath, ToJsonNode(transformed));
            }
        }

        return root;
    }

    /// <summary>Flat "key=value" analog of BuildRequestBody for RequestContentType =
    /// FormUrlEncoded — same mapping semantics (Package.*, Constant, scalar; "[]" repeats once
    /// per package), but writing directly to a form field list instead of a nested JSON tree.
    /// Uses 1-based package indices ("wweight[1]", "wweight[2]", ...) since that's the real
    /// convention these legacy form-based rate-quote APIs use, unlike JSON's 0-based arrays.</summary>
    private static List<KeyValuePair<string, string>> BuildFormFields(ShippingConnector connector, Shipment shipment)
    {
        var fields = new List<KeyValuePair<string, string>>();
        var packages = shipment.Packages.ToList();

        foreach (var mapping in connector.FieldMappings.Where(m => m.Direction == "Outbound"))
        {
            if (mapping.ExternalPath.Contains("[]", StringComparison.Ordinal))
            {
                var propertyName = mapping.NovaField.StartsWith("Package.", StringComparison.Ordinal)
                    ? mapping.NovaField["Package.".Length..]
                    : null;

                for (var i = 0; i < packages.Count; i++)
                {
                    var raw = propertyName is not null
                        ? GetPackageValue(packages[i], propertyName)
                        : mapping.NovaField == "Constant"
                            ? mapping.ConstantValue
                            : GetScalarValue(shipment, mapping.NovaField);
                    var transformed = ApplyTransform(raw, mapping.Transform);
                    var concreteKey = ResolveIndex(mapping.ExternalPath, i + 1);
                    fields.Add(new KeyValuePair<string, string>(concreteKey, transformed?.ToString() ?? string.Empty));
                }
            }
            else if (mapping.NovaField == "Constant")
            {
                var transformed = ApplyTransform(mapping.ConstantValue, mapping.Transform);
                fields.Add(new KeyValuePair<string, string>(mapping.ExternalPath, transformed?.ToString() ?? string.Empty));
            }
            else
            {
                var raw = GetScalarValue(shipment, mapping.NovaField);
                var transformed = ApplyTransform(raw, mapping.Transform);
                fields.Add(new KeyValuePair<string, string>(mapping.ExternalPath, transformed?.ToString() ?? string.Empty));
            }
        }

        return fields;
    }

    private static List<RateQuoteDto> ParseQuotes(ShippingConnector connector, JsonElement responseRoot)
    {
        var inbound = connector.FieldMappings.Where(m => m.Direction == "Inbound").ToList();
        if (inbound.Count == 0) return new List<RateQuoteDto>();

        // The mapping with the most "[]" wildcards drives how many quote rows exist (e.g.
        // "payload[].rates[].price" — one row per carrier/rate combination). Shallower mappings
        // (e.g. "payload[].carrierName", one wildcard) are resolved against just the leading part
        // of each row's index tuple, so a carrier-level field correctly broadcasts across every
        // one of that carrier's nested rates instead of being flat-index-misaligned against them.
        // "Constant" sentinel rows and Quote.Surcharge* rows never drive row count — Constant's
        // ExternalPath isn't a real response path, and a surcharges sub-array's wildcard is one
        // level deeper than the quote-row wildcard (it would otherwise look "deepest" and
        // wrongly produce one row per surcharge instead of one row per carrier quote).
        var realMappings = inbound.Where(m => m.ExternalPath != "Constant" && !IsSurchargeField(m.NovaField)).ToList();
        var indexTuples = realMappings.Count == 0
            ? new List<int[]> { Array.Empty<int>() }
            : JsonPathWalker.GetIndexTuples(responseRoot, realMappings.OrderByDescending(m => JsonPathWalker.CountWildcards(m.ExternalPath)).First().ExternalPath);

        var surchargeNameMapping = inbound.FirstOrDefault(m => m.NovaField == "Quote.SurchargeName");
        var surchargeAmountMapping = inbound.FirstOrDefault(m => m.NovaField == "Quote.SurchargeAmount");

        var quotes = new List<RateQuoteDto>();
        foreach (var tuple in indexTuples)
        {
            var quote = new RateQuoteDto { ConnectorId = connector.Id, ConnectorName = connector.Name };
            foreach (var mapping in inbound)
            {
                if (IsSurchargeField(mapping.NovaField)) continue;

                // Inbound's "Constant" flips Outbound's convention: since ExternalPath is
                // normally the response-side source for this direction, "Constant" there means
                // "ignore the response, fill this Quote field with ConstantValue on every row" —
                // covers carriers whose response has no carrier-name field to read, for example.
                if (mapping.ExternalPath == "Constant")
                {
                    var constant = ApplyTransform(mapping.ConstantValue, mapping.Transform);
                    AssignQuoteField(quote, mapping.NovaField, constant);
                    continue;
                }

                var resolvedPath = JsonPathWalker.ResolveIndices(mapping.ExternalPath, tuple);
                var element = JsonPathWalker.GetSingleValue(responseRoot, resolvedPath);
                if (element is null) continue;
                var raw = ExtractRawValue(element.Value);
                var transformed = ApplyTransform(raw, mapping.Transform);
                AssignQuoteField(quote, mapping.NovaField, transformed);
            }

            if (surchargeNameMapping is not null)
            {
                var namePath = JsonPathWalker.ResolveIndices(surchargeNameMapping.ExternalPath, tuple);
                var names = JsonPathWalker.GetValues(responseRoot, namePath)
                    .Select(e => ApplyTransform(ExtractRawValue(e), surchargeNameMapping.Transform)?.ToString())
                    .ToList();

                List<object?> amounts = new();
                if (surchargeAmountMapping is not null)
                {
                    var amountPath = JsonPathWalker.ResolveIndices(surchargeAmountMapping.ExternalPath, tuple);
                    amounts = JsonPathWalker.GetValues(responseRoot, amountPath)
                        .Select(e => ApplyTransform(ExtractRawValue(e), surchargeAmountMapping.Transform))
                        .ToList();
                }

                for (var i = 0; i < names.Count; i++)
                {
                    quote.Surcharges.Add(new RateSurchargeDto
                    {
                        Name = names[i],
                        Amount = i < amounts.Count && amounts[i] is not null ? Convert.ToDecimal(amounts[i]) : null
                    });
                }
            }

            quotes.Add(quote);
        }
        return quotes;
    }

    private static bool IsSurchargeField(string novaField) =>
        novaField == "Quote.SurchargeName" || novaField == "Quote.SurchargeAmount";

    /// <summary>XML analog of ParseQuotes — same "deepest-wildcard mapping drives row count"
    /// logic, walked via XmlPathWalker instead of JsonPathWalker. Paths address the response
    /// root element's own children directly (the root element's tag itself isn't part of the
    /// path, mirroring how a JSON root object's properties are addressed without naming the
    /// object itself) — e.g. against "&lt;ratequote&gt;&lt;quotetotal&gt;395.49&lt;/quotetotal&gt;...",
    /// the path is "quotetotal", not "ratequote.quotetotal".</summary>
    private static List<RateQuoteDto> ParseQuotesXml(ShippingConnector connector, XElement responseRoot)
    {
        var inbound = connector.FieldMappings.Where(m => m.Direction == "Inbound").ToList();
        if (inbound.Count == 0) return new List<RateQuoteDto>();

        var realMappings = inbound.Where(m => m.ExternalPath != "Constant" && !IsSurchargeField(m.NovaField)).ToList();
        var indexTuples = realMappings.Count == 0
            ? new List<int[]> { Array.Empty<int>() }
            : XmlPathWalker.GetIndexTuples(responseRoot, realMappings.OrderByDescending(m => XmlPathWalker.CountWildcards(m.ExternalPath)).First().ExternalPath);

        var surchargeNameMapping = inbound.FirstOrDefault(m => m.NovaField == "Quote.SurchargeName");
        var surchargeAmountMapping = inbound.FirstOrDefault(m => m.NovaField == "Quote.SurchargeAmount");

        var quotes = new List<RateQuoteDto>();
        foreach (var tuple in indexTuples)
        {
            var quote = new RateQuoteDto { ConnectorId = connector.Id, ConnectorName = connector.Name };
            foreach (var mapping in inbound)
            {
                if (IsSurchargeField(mapping.NovaField)) continue;

                if (mapping.ExternalPath == "Constant")
                {
                    var constant = ApplyTransform(mapping.ConstantValue, mapping.Transform);
                    AssignQuoteField(quote, mapping.NovaField, constant);
                    continue;
                }

                var resolvedPath = XmlPathWalker.ResolveIndices(mapping.ExternalPath, tuple);
                var raw = XmlPathWalker.GetSingleValue(responseRoot, resolvedPath);
                if (raw is null) continue;
                var transformed = ApplyTransform(raw, mapping.Transform);
                AssignQuoteField(quote, mapping.NovaField, transformed);
            }

            if (surchargeNameMapping is not null)
            {
                var namePath = XmlPathWalker.ResolveIndices(surchargeNameMapping.ExternalPath, tuple);
                var names = XmlPathWalker.GetValues(responseRoot, namePath)
                    .Select(v => ApplyTransform(v, surchargeNameMapping.Transform)?.ToString())
                    .ToList();

                List<object?> amounts = new();
                if (surchargeAmountMapping is not null)
                {
                    var amountPath = XmlPathWalker.ResolveIndices(surchargeAmountMapping.ExternalPath, tuple);
                    amounts = XmlPathWalker.GetValues(responseRoot, amountPath)
                        .Select(v => ApplyTransform(v, surchargeAmountMapping.Transform))
                        .ToList();
                }

                for (var i = 0; i < names.Count; i++)
                {
                    quote.Surcharges.Add(new RateSurchargeDto
                    {
                        Name = names[i],
                        Amount = i < amounts.Count && amounts[i] is not null ? Convert.ToDecimal(amounts[i]) : null
                    });
                }
            }

            quotes.Add(quote);
        }
        return quotes;
    }

    public async Task<ConnectorTestResultDto> SendRawAsync(ShippingConnector connector, string? rawBody, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var client = _httpClientFactory.CreateClient(nameof(ShippingConnectorHttpRunner));
            var method = string.Equals(connector.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase) ? HttpMethod.Get : HttpMethod.Post;
            using var request = new HttpRequestMessage(method, connector.BaseUrl);

            await ApplyAuthAsync(request, connector, client, ct);

            // Postman-style "raw" send — whatever the user typed goes out verbatim as the body,
            // with the Content-Type header matching the connector's RequestContentType (the
            // user manages any encoding themselves, same as Postman's raw body mode).
            if (method == HttpMethod.Post && !string.IsNullOrWhiteSpace(rawBody))
                request.Content = new StringContent(rawBody, Encoding.UTF8, ContentTypeHeaderFor(connector.RequestContentType));

            using var response = await client.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            stopwatch.Stop();

            return new ConnectorTestResultDto
            {
                Success      = response.IsSuccessStatusCode,
                StatusCode   = (int)response.StatusCode,
                ResponseBody = body,
                ElapsedMs    = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ConnectorTestResultDto { Success = false, Error = ex.Message, ElapsedMs = stopwatch.ElapsedMilliseconds };
        }
    }

    private static string ContentTypeHeaderFor(string requestContentType) => requestContentType switch
    {
        "FormUrlEncoded" => "application/x-www-form-urlencoded",
        "Xml" => "application/xml",
        "Text" => "text/plain",
        _ => "application/json"
    };

    private static async Task ApplyAuthAsync(HttpRequestMessage request, ShippingConnector connector, HttpClient client, CancellationToken ct)
    {
        switch (connector.AuthType)
        {
            case "ApiKeyHeader":
                if (!string.IsNullOrWhiteSpace(connector.AuthHeaderName) && !string.IsNullOrWhiteSpace(connector.AuthApiKey))
                    request.Headers.Add(connector.AuthHeaderName, connector.AuthApiKey);
                break;
            case "BearerToken":
                if (!string.IsNullOrWhiteSpace(connector.AuthApiKey))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connector.AuthApiKey);
                break;
            case "BasicAuth":
                if (!string.IsNullOrWhiteSpace(connector.AuthUsername))
                {
                    var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{connector.AuthUsername}:{connector.AuthPassword}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
                }
                break;
            case "TokenLogin":
                var token = await FetchLoginTokenAsync(connector, client, ct);
                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                break;
        }
    }

    /// <summary>Calls the connector's separate login endpoint with {"principal":..,"credential":..}
    /// and extracts the bearer token from the response via AuthTokenResponsePath (default
    /// "token") — covers carriers like eShipper whose auth is a login call, not a static key.
    /// Returns null (never throws) on any failure, matching this class's "typed result, no
    /// exceptions" discipline; the caller simply proceeds unauthenticated in that case.</summary>
    private static async Task<string?> FetchLoginTokenAsync(ShippingConnector connector, HttpClient client, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(connector.AuthTokenUrl)) return null;
        try
        {
            var loginBody = new JsonObject
            {
                ["principal"] = connector.AuthUsername,
                ["credential"] = connector.AuthPassword
            };
            using var loginRequest = new HttpRequestMessage(HttpMethod.Post, connector.AuthTokenUrl)
            {
                Content = new StringContent(loginBody.ToJsonString(), Encoding.UTF8, "application/json")
            };
            using var loginResponse = await client.SendAsync(loginRequest, ct);
            if (!loginResponse.IsSuccessStatusCode) return null;

            var loginResponseBody = await loginResponse.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(loginResponseBody);
            var path = string.IsNullOrWhiteSpace(connector.AuthTokenResponsePath) ? "token" : connector.AuthTokenResponsePath;
            var value = JsonPathWalker.GetSingleValue(doc.RootElement, path);
            return value?.ValueKind == JsonValueKind.String ? value.Value.GetString() : null;
        }
        catch
        {
            return null;
        }
    }

    private static object? GetScalarValue(Shipment shipment, string novaField) => novaField switch
    {
        "Shipment.ShipToName" => shipment.ShipToName,
        "Shipment.ShipToContactName" => shipment.ShipToContactName,
        "Shipment.ShipToEmail" => shipment.ShipToEmail,
        "Shipment.ShipToAddressLine1" => shipment.ShipToAddressLine1,
        "Shipment.ShipToAddressLine2" => shipment.ShipToAddressLine2,
        "Shipment.ShipToCity" => shipment.ShipToCity,
        "Shipment.ShipToState" => shipment.ShipToState,
        "Shipment.ShipToPostalCode" => shipment.ShipToPostalCode,
        "Shipment.ShipToCountry" => shipment.ShipToCountry,
        "Shipment.ShipToPhone" => shipment.ShipToPhone,
        "Shipment.ShipToTaxType" => shipment.ShipToTaxType,
        "Shipment.ShipToTaxCountry" => shipment.ShipToTaxCountry,
        "Shipment.ShipToTaxId" => shipment.ShipToTaxId,
        "Shipment.ShipDate" => shipment.ShipDate.ToString("yyyy-MM-dd"),
        // Ship-from reads prefer the Shipment's own ShipFrom* override (set when a shipment
        // actually ships from somewhere other than its Warehouse's registered address) and
        // fall back to the Warehouse's address otherwise — existing connector mappings that
        // reference these same "Warehouse.*" keys keep working unchanged for the common case.
        "Warehouse.ContactName" => shipment.ShipFromContactName ?? shipment.Warehouse.ContactName,
        "Warehouse.Email" => shipment.ShipFromEmail ?? shipment.Warehouse.Email,
        "Warehouse.Phone" => shipment.ShipFromPhone ?? shipment.Warehouse.Phone,
        "Warehouse.Address" => shipment.ShipFromAddressLine1 ?? shipment.Warehouse.Address,
        "Warehouse.AddressLine2" => shipment.ShipFromAddressLine2 ?? shipment.Warehouse.AddressLine2,
        "Warehouse.City" => shipment.ShipFromCity ?? shipment.Warehouse.City,
        "Warehouse.State" => shipment.ShipFromState ?? shipment.Warehouse.State,
        "Warehouse.PostalCode" => shipment.ShipFromPostalCode ?? shipment.Warehouse.PostalCode,
        "Warehouse.Country" => shipment.ShipFromCountry ?? shipment.Warehouse.Country,
        "Warehouse.TaxType" => shipment.Warehouse.TaxType,
        "Warehouse.TaxCountry" => shipment.Warehouse.TaxCountry,
        "Warehouse.TaxId" => shipment.Warehouse.TaxId,
        "Warehouse.Name" => shipment.ShipFromName ?? shipment.Warehouse.Name,
        "Warehouse.Code" => shipment.Warehouse.Code,
        _ => null
    };

    private static object? GetPackageValue(ShipmentPackage package, string propertyName) => propertyName switch
    {
        "WeightKg" => package.WeightKg,
        "LengthCm" => package.LengthCm,
        "WidthCm" => package.WidthCm,
        "HeightCm" => package.HeightCm,
        _ => null
    };

    private static void AssignQuoteField(RateQuoteDto quote, string novaField, object? value)
    {
        switch (novaField)
        {
            case "Quote.CarrierName": quote.CarrierName = value?.ToString(); break;
            case "Quote.ServiceLevel": quote.ServiceLevel = value?.ToString(); break;
            case "Quote.PublishedCost": quote.PublishedCost = value is null ? null : Convert.ToDecimal(value); break;
            case "Quote.Price": quote.Price = value is null ? null : Convert.ToDecimal(value); break;
            case "Quote.Currency": quote.Currency = value?.ToString(); break;
            case "Quote.EstimatedDays": quote.EstimatedDays = value is null ? null : Convert.ToInt32(Convert.ToDecimal(value)); break;
            case "Quote.Etd": quote.Etd = value is null ? null : DateTime.Parse(value.ToString()!, CultureInfo.InvariantCulture); break;
            case "Quote.RateId": quote.RateId = value?.ToString(); break;
        }
    }

    private static object? ApplyTransform(object? value, string transform)
    {
        if (value is null) return null;
        return transform switch
        {
            "KgToLb" => Convert.ToDecimal(value) * 2.20462m,
            "LbToKg" => Convert.ToDecimal(value) / 2.20462m,
            "CmToIn" => Convert.ToDecimal(value) / 2.54m,
            "InToCm" => Convert.ToDecimal(value) * 2.54m,
            "Uppercase" => value.ToString()?.ToUpperInvariant(),
            "Lowercase" => value.ToString()?.ToLowerInvariant(),
            // NovaERP's Shipment.ShipDate is date-only — some carriers (e.g. eShipper's
            // scheduledShipDate) require a full date+time. Appends a fixed 09:00 rather than
            // adding a time-of-day field to the domain model for one connector's requirement.
            "DateAppend0900" => $"{value} 09:00",
            // Some carriers (e.g. legacy form-based rate-quote APIs) require MM/dd/yyyy rather
            // than NovaERP's internal yyyy-MM-dd — reformats without changing the actual date.
            "DateFormatMDY" => DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var mdy)
                ? mdy.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
                : value,
            _ => value
        };
    }

    private static object? ExtractRawValue(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.String => el.GetString(),
        JsonValueKind.Number => el.TryGetDecimal(out var d) ? d : null,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };

    private static JsonNode? ToJsonNode(object? value) => value switch
    {
        null => null,
        decimal d => JsonValue.Create(d),
        int i => JsonValue.Create(i),
        bool b => JsonValue.Create(b),
        _ => JsonValue.Create(value.ToString())
    };

    private static string ResolveIndex(string path, int index) => path.Replace("[]", $"[{index}]");
}
