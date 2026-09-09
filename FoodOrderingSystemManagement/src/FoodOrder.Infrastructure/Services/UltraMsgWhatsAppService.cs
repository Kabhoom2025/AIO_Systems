using System.Text;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FoodOrder.Infrastructure.Services;

public class UltraMsgWhatsAppService : IWhatsAppService
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UltraMsgWhatsAppService> _logger;

    public UltraMsgWhatsAppService(
        ISettingsRepository settingsRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<UltraMsgWhatsAppService> logger)
    {
        _settingsRepository = settingsRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(
        string phone, string orderNumber, string itemsSummary, decimal grandTotal, string restaurantName)
    {
        var message =
            $"🍽️ *Order Confirmed!*\n" +
            $"📍 {restaurantName}\n\n" +
            $"📋 *Order:* {orderNumber}\n" +
            $"🛒 {itemsSummary}\n\n" +
            $"*Total: ₹{grandTotal:F2}*\n\n" +
            $"Thank you for your order! We'll get it ready soon. 😊";

        await SendAsync(phone, message);
    }

    public async Task SendOrderCancellationAsync(
        string phone, string orderNumber, decimal grandTotal, string restaurantName)
    {
        var message =
            $"❌ *Order Cancelled*\n" +
            $"📍 {restaurantName}\n\n" +
            $"Order *{orderNumber}* has been cancelled.\n" +
            $"Amount: ₹{grandTotal:F2}\n\n" +
            $"Sorry for the inconvenience. Visit us again! 🙏";

        await SendAsync(phone, message);
    }

    private async Task SendAsync(string phone, string message)
    {
        try
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            if (settings is null ||
                string.IsNullOrWhiteSpace(settings.UltraMsgInstanceId) ||
                string.IsNullOrWhiteSpace(settings.UltraMsgToken))
            {
                _logger.LogInformation("WhatsApp skipped: UltraMsg not configured in settings.");
                return;
            }

            var digits = new string(phone.Where(char.IsDigit).ToArray());
            var to = digits.Length == 10 ? $"91{digits}" : digits;

            var url = $"https://api.ultramsg.com/{settings.UltraMsgInstanceId}/messages/chat";

            var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"]  = settings.UltraMsgToken,
                ["to"]     = to,
                ["body"]   = message,
                ["priority"] = "1"
            });

            var client = _httpClientFactory.CreateClient("ultramsg");
            var response = await client.PostAsync(url, body);
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("UltraMsg response for {To}: {Body}", to, responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WhatsApp send failed for phone {Phone}", phone);
        }
    }
}
