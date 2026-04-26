using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SWFC.Application.M800_Security.M803_DataSecurity;

namespace SWFC.Infrastructure.M800_Security.DataProtection;

public sealed class AesGcmSensitiveDataProtector : ISensitiveDataProtector
{
    private const string Scheme = "AES-256-GCM";
    private readonly byte[] _key;
    private readonly string _keyVersion;

    public AesGcmSensitiveDataProtector(IConfiguration configuration)
    {
        var configuredKey = configuration["Security:DataProtection:Key"]
            ?? Environment.GetEnvironmentVariable("SWFC_DATA_PROTECTION_KEY");

        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            throw new InvalidOperationException("Data protection key is not configured.");
        }

        _key = DecodeKey(configuredKey);
        _keyVersion = configuration["Security:DataProtection:KeyVersion"] ?? "v1";
    }

    public ProtectedDataPayload Protect(
        string plainText,
        SensitiveDataClassification classification)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var envelope = Convert.ToBase64String(Combine(nonce, tag, cipherBytes));

        return new ProtectedDataPayload(
            envelope,
            Scheme,
            _keyVersion,
            classification,
            DateTime.UtcNow);
    }

    public string Reveal(ProtectedDataPayload payload)
    {
        if (!string.Equals(payload.ProtectionScheme, Scheme, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Unsupported data protection scheme.");
        }

        var envelope = Convert.FromBase64String(payload.CipherText);
        var nonceLength = AesGcm.NonceByteSizes.MaxSize;
        var tagLength = AesGcm.TagByteSizes.MaxSize;

        if (envelope.Length <= nonceLength + tagLength)
        {
            throw new InvalidOperationException("Protected payload is invalid.");
        }

        var nonce = envelope[..nonceLength];
        var tag = envelope[nonceLength..(nonceLength + tagLength)];
        var cipherBytes = envelope[(nonceLength + tagLength)..];
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, tagLength);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private static byte[] DecodeKey(string configuredKey)
    {
        try
        {
            var key = Convert.FromBase64String(configuredKey);

            if (key.Length == 32)
            {
                return key;
            }
        }
        catch (FormatException)
        {
        }

        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(configuredKey));
    }

    private static byte[] Combine(
        byte[] nonce,
        byte[] tag,
        byte[] cipherBytes)
    {
        var result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length + tag.Length, cipherBytes.Length);

        return result;
    }
}
