using System.Security.Cryptography;
using SWFC.Application.M100_System.M103_Authentication;

namespace SWFC.Infrastructure.M100_System.M103_Authentication.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        var normalizedPassword = password.Trim();

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            normalizedPassword,
            salt,
            Iterations,
            Algorithm,
            KeySize);

        return string.Join(
            '.',
            "PBKDF2",
            Algorithm.Name,
            Iterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 5)
        {
            return false;
        }

        if (!string.Equals(parts[0], "PBKDF2", StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(parts[2], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;

        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expectedHash = Convert.FromBase64String(parts[4]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password.Trim(),
            salt,
            iterations,
            Algorithm,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}

