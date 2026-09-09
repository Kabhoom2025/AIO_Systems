using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FoodOrder.Infrastructure.Services;

public class Msg91SmsService : ISmsService
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Msg91SmsService> _logger;

    public Msg91SmsService(
        ISettingsRepository settingsRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<Msg91SmsService> logger)
    {
        _settingsRepository = settingsRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(
        string phone, string orderNumber, string itemsSummary, decimal grandTotal, string restaurantName)
    {
        var message =
            $"Order Confirmed! {restaurantName}\n" +
            $"Order: {orderNumber}\n" +
            $"{itemsSummary}\n" +
            $"Total: Rs.{grandTotal:F2}\n" +
            $"Thank you for your order!";

        await SendAsync(phone, message);
    }

    public async Task SendOrderCancellationAsync(
        string phone, string orderNumber, decimal grandTotal, string restaurantName)
    {
        var message =
            $"Order Cancelled. {restaurantName}\n" +
            $"Order {orderNumber} has been cancelled.\n" +
            $"Amount: Rs.{grandTotal:F2}\n" +
            $"Sorry for the inconvenience. Visit us again!";

        await SendAsync(phone, message);
    }

    private async Task SendAsync(string phone, string message)
    {
        try
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            if (settings is null ||
                string.IsNullOrWhiteSpace(settings.Msg91ApiKey) ||
                string.IsNullOrWhiteSpace(settings.Msg91SenderId))
            {
                _logger.LogInformation("SMS skipped: MSG91 not configured in settings.");
                return;
            }

            var digits = new string(phone.Where(char.IsDigit).ToArray());
            var mobile = digits.Length == 10 ? $"91{digits}" : digits;

            var url = $"https://api.msg91.com/api/sendhttp.php" +
                      $"?authkey={Uri.EscapeDataString(settings.Msg91ApiKey)}" +
                      $"&mobiles={mobile}" +
                      $"&message={Uri.EscapeDataString(message)}" +
                      $"&sender={Uri.EscapeDataString(settings.Msg91SenderId)}" +
                      $"&route=4" +
                      $"&country=91";

            var client = _httpClientFactory.CreateClient("msg91");
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("MSG91 response for {Mobile}: {Body}", mobile, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS send failed for phone {Phone}", phone);
        }
    }
}
