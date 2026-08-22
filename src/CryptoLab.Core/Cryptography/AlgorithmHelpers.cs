using System.Security.Cryptography;
using System.Text;
using CryptoLab.Core.Models;

namespace CryptoLab.Core.Cryptography;

internal static class AlgorithmHelpers
{
    public static string TransformLatinLetters(string text, Func<int, int> transform)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (c is >= 'A' and <= 'Z')
            {
                builder.Append((char)('A' + Mod(transform(c - 'A'), 26)));
            }
            else if (c is >= 'a' and <= 'z')
            {
                builder.Append((char)('a' + Mod(transform(c - 'a'), 26)));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    public static int Mod(int value, int modulus)
    {
        var result = value % modulus;
        return result < 0 ? result + modulus : result;
    }

    public static int Gcd(int a, int b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
        {
            var remainder = a % b;
            a = b;
            b = remainder;
        }

        return a;
    }

    public static int ModularInverse(int value, int modulus)
    {
        value = Mod(value, modulus);
        for (var i = 1; i < modulus; i++)
        {
            if (Mod(value * i, modulus) == 1)
            {
                return i;
            }
        }

        throw new ArgumentException("The selected key has no modular inverse.");
    }

    public static bool TryReadInt(CryptoRequest request, string name, out int value)
    {
        return int.TryParse(request.GetParameter(name), out value);
    }

    public static byte[] ReadUtf8Key(CryptoRequest request, string name, params int[] allowedByteLengths)
    {
        var value = request.GetParameter(name).Trim();
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("A key is required for this algorithm.");
        }

        var key = Encoding.UTF8.GetBytes(value);
        if (!allowedByteLengths.Contains(key.Length))
        {
            throw new ArgumentException($"Invalid key length. Use {string.Join(", ", allowedByteLengths)} UTF-8 bytes.");
        }

        return key;
    }

    public static byte[] ReadOptionalFixedBytes(string value, int requiredLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return RandomBytes(requiredLength);
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(value.Trim());
        }
        catch (FormatException)
        {
            bytes = Encoding.UTF8.GetBytes(value);
        }

        if (bytes.Length != requiredLength)
        {
            throw new ArgumentException($"{fieldName} must be exactly {requiredLength} bytes. You may enter Base64 or text with the exact byte length.");
        }

        return bytes;
    }

    public static string ToHex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();

    public static IReadOnlyList<VisualizationStep> LimitSteps(IEnumerable<VisualizationStep> steps)
    {
        return steps.Take(80).ToList();
    }

    public static byte[] RandomBytes(int length)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }
}
