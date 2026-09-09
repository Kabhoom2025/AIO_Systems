using System.Net;
using System.Net.Mail;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;

namespace FoodOrder.Infrastructure.Services;

public class EmailService(ISettingsRepository settingsRepository) : IEmailService
{
    public async Task SendPasswordResetAsync(string toEmail, string toName, string resetLink)
    {
        var s = await settingsRepository.GetSettingsAsync();

        var host     = s?.SmtpHost;
        var username = s?.SmtpUsername;
        var password = s?.SmtpPassword;
        var from     = s?.SmtpFromEmail;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException(
                "SMTP is not configured. Go to Settings → Email Configuration and fill in the SMTP details.");

        var port = s!.SmtpPort > 0 ? s.SmtpPort : 587;
        var ssl  = s.SmtpSsl;
        var fromName = string.IsNullOrWhiteSpace(s.SmtpFromName) ? "FoodOrder POS" : s.SmtpFromName;

        var body = $@"
<!DOCTYPE html>
<html>
<body style=""font-family:Arial,sans-serif;background:#f5f5f5;margin:0;padding:0"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
    <tr><td align=""center"" style=""padding:40px 16px"">
      <table width=""560"" cellpadding=""0"" cellspacing=""0""
             style=""background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.08)"">
        <tr>
          <td style=""background:#bf360c;padding:28px 32px"">
            <h1 style=""margin:0;color:#fff;font-size:20px;font-weight:700"">
              🍽️ FoodOrder POS — Password Reset
            </h1>
          </td>
        </tr>
        <tr>
          <td style=""padding:32px"">
            <p style=""margin:0 0 16px;color:#212121;font-size:15px"">Hi {toName},</p>
            <p style=""margin:0 0 24px;color:#424242;font-size:15px;line-height:1.6"">
              We received a request to reset the password for your account.<br/>
              Click the button below to set a new password. This link expires in <strong>30 minutes</strong>.
            </p>
            <div style=""text-align:center;margin:32px 0"">
              <a href=""{resetLink}""
                 style=""display:inline-block;background:#bf360c;color:#fff;padding:14px 36px;
                         border-radius:6px;font-size:15px;font-weight:600;text-decoration:none"">
                Reset Password
              </a>
            </div>
            <p style=""margin:24px 0 0;color:#757575;font-size:13px"">
              If you didn't request this, you can safely ignore this email — your password will not change.<br/>
              The link will expire automatically after 30 minutes.
            </p>
          </td>
        </tr>
        <tr>
          <td style=""background:#fafafa;padding:16px 32px;border-top:1px solid #f0f0f0"">
            <p style=""margin:0;color:#9e9e9e;font-size:12px"">
              © {DateTime.UtcNow.Year} FoodOrder POS. This is an automated message — please do not reply.
            </p>
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

        using var smtp = new SmtpClient(host, port)
        {
            Credentials = string.IsNullOrWhiteSpace(username)
                ? null
                : new NetworkCredential(username, password),
            EnableSsl = ssl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };

        var msg = new MailMessage
        {
            From       = new MailAddress(from, fromName),
            Subject    = "Reset your FoodOrder POS password",
            Body       = body,
            IsBodyHtml = true,
        };
        msg.To.Add(new MailAddress(toEmail, toName));

        await smtp.SendMailAsync(msg);
    }
}
