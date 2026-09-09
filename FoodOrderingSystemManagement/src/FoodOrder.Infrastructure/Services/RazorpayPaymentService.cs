using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FoodOrder.Application.DTOs.Payment;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Infrastructure.Services;

public class RazorpayPaymentService : IPaymentService
{
    private readonly ISettingsRepository _settingsRepo;
    private readonly IHttpClientFactory  _httpFactory;

    public RazorpayPaymentService(ISettingsRepository settingsRepo, IHttpClientFactory httpFactory)
    {
        _settingsRepo = settingsRepo;
        _httpFactory  = httpFactory;
    }

    public async Task<RazorpayOrderResult> CreateRazorpayOrderAsync(decimal amountInRupees, string receiptId)
    {
        var settings = await _settingsRepo.GetSettingsAsync();
        if (string.IsNullOrWhiteSpace(settings.RazorpayKeyId) ||
            string.IsNullOrWhiteSpace(settings.RazorpayKeySecret))
            throw new AppException("Razorpay is not configured. Add Key ID and Key Secret in Integration Settings.");

        var client = _httpFactory.CreateClient("razorpay");
        var creds  = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{settings.RazorpayKeyId}:{settings.RazorpayKeySecret}"));
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", creds);

        var body    = new { amount = (long)(amountInRupees * 100), currency = "INR", receipt = receiptId };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response     = await client.PostAsync("https://api.razorpay.com/v1/orders", content);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new AppException($"Razorpay order creation failed: {responseText}");

        using var doc  = JsonDocument.Parse(responseText);
        var root = doc.RootElement;

        return new RazorpayOrderResult
        {
            RazorpayOrderId = root.GetProperty("id").GetString()       ?? string.Empty,
            Amount          = root.GetProperty("amount").GetInt64(),
            Currency        = root.GetProperty("currency").GetString()  ?? "INR",
            Receipt         = root.GetProperty("receipt").GetString()   ?? receiptId,
            KeyId           = settings.RazorpayKeyId,
        };
    }

    public async Task<bool> VerifyRazorpaySignatureAsync(
        string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
    {
        var settings = await _settingsRepo.GetSettingsAsync();
        if (string.IsNullOrWhiteSpace(settings.RazorpayKeySecret))
            return false;

        var payload  = $"{razorpayOrderId}|{razorpayPaymentId}";
        var keyBytes = Encoding.UTF8.GetBytes(settings.RazorpayKeySecret);

        using var hmac     = new HMACSHA256(keyBytes);
        var hash           = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expected       = Convert.ToHexString(hash).ToLower();

        return string.Equals(expected, razorpaySignature, StringComparison.OrdinalIgnoreCase);
    }
}
