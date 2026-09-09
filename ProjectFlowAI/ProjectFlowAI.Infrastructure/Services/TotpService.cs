using System.Security.Cryptography;
using System.Text;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Infrastructure.Services;

/// <summary>RFC 6238 TOTP (30s step, 6 digits, HMAC-SHA1 per the RFC's default) implemented directly
/// against System.Security.Cryptography — no external NuGet package needed. QR image rendering is
/// left to the frontend; this only produces the otpauth:// URI a QR-code component can encode.</summary>
public class TotpService : ITotpService
{
    private const int StepSeconds = 30;
    private const int Digits = 6;

    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20); // 160-bit secret, standard for TOTP
        return Base32Encode(bytes);
    }

    public string GenerateQrCodeUri(string secret, string email, string issuer) =>
        $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}" +
        $"?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&digits={Digits}&period={StepSeconds}&algorithm=SHA1";

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code)) return false;

        var counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / StepSeconds;
        // Allow +/- 1 step of clock drift, as is conventional for TOTP validators.
        for (var offset = -1; offset <= 1; offset++)
        {
            if (ComputeCode(secret, counter + offset) == code)
                return true;
        }
        return false;
    }

    private static string ComputeCode(string base32Secret, long counter)
    {
        var key = Base32Decode(base32Secret);
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);

        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                     | ((hash[offset + 1] & 0xFF) << 16)
                     | ((hash[offset + 2] & 0xFF) << 8)
                     | (hash[offset + 3] & 0xFF);

        var otp = binary % (int)Math.Pow(10, Digits);
        return otp.ToString(new string('0', Digits));
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var sb = new StringBuilder();
        int bits = 0, value = 0;
        foreach (var b in data)
        {
            value = (value << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                sb.Append(alphabet[(value >> (bits - 5)) & 31]);
                bits -= 5;
            }
        }
        if (bits > 0)
            sb.Append(alphabet[(value << (5 - bits)) & 31]);
        return sb.ToString();
    }

    private static byte[] Base32Decode(string base32)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        base32 = base32.TrimEnd('=').ToUpperInvariant();
        var bytes = new List<byte>();
        int bits = 0, value = 0;
        foreach (var c in base32)
        {
            var index = alphabet.IndexOf(c);
            if (index < 0) continue;
            value = (value << 5) | index;
            bits += 5;
            if (bits >= 8)
            {
                bytes.Add((byte)((value >> (bits - 8)) & 0xFF));
                bits -= 8;
            }
        }
        return bytes.ToArray();
    }
}
