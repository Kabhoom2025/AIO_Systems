using System.Net;

namespace NovaERP.Tests.TestDoubles;

/// <summary>Stands in for a real TMS/rate-shop HTTP endpoint in tests — captures the outgoing
/// request body (so a test can assert the outbound field mapping built it correctly) and always
/// returns a fixed canned JSON response (so a test can assert the inbound field mapping parses
/// it correctly). Swapped in via ConfigurePrimaryHttpMessageHandler on the named
/// "ShippingConnectorHttpRunner" HttpClient, the same DI-override technique already used for
/// FakeProposeTicketAiProvider.</summary>
public class FakeRateShopHttpMessageHandler : HttpMessageHandler
{
    public string? LastRequestBody { get; private set; }
    public string ResponseJson { get; set; } = "{}";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ResponseJson, System.Text.Encoding.UTF8, "application/json")
        };
    }
}
