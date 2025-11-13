using System;
using System.Globalization;
using System.Security.Cryptography;

namespace SchedulePayrollBlazor.Utilities;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    private const char Delimiter = ':';

    public static string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(key), Iterations, Algorithm.Name);
    }

    public static bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var segments = passwordHash.Split(Delimiter);
        if (segments.Length != 4)
        {
            return false;
        }

        var salt = Convert.FromBase64String(segments[0]);
        var storedKey = Convert.FromBase64String(segments[1]);
        var iterations = int.Parse(segments[2], CultureInfo.InvariantCulture);
        var algorithm = new HashAlgorithmName(segments[3]);

        var generatedKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, algorithm, storedKey.Length);
        return CryptographicOperations.FixedTimeEquals(storedKey, generatedKey);
    }
}
