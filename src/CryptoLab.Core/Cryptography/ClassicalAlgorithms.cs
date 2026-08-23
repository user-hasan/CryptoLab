using System.Text;
using CryptoLab.Core.Models;

namespace CryptoLab.Core.Cryptography;

public sealed class CaesarCipherAlgorithm : ICryptoAlgorithm
{
    public AlgorithmMetadata Metadata { get; } = new(
        "caesar", "Caesar Cipher", AlgorithmCategory.ClassicalCipher, SecurityLevel.EducationalOnly,
        "A substitution cipher that shifts each alphabetic character by a fixed number.",
        "Used in ancient Rome for simple military messages.",
        "Useful for teaching substitution, modular arithmetic, and why simple ciphers fail.",
        new[] { "Shift key", "Preserves non-Latin characters", "Reversible", "Easy to brute-force" },
        true, true, false, true, "Shift number from 0 to 25",
        "Educational use only. Caesar Cipher is not secure for sensitive data.");

    public ValidationResult Validate(CryptoRequest request)
    {
        return AlgorithmHelpers.TryReadInt(request, "shift", out var shift) && shift is >= 0 and <= 25
            ? ValidationResult.Valid
            : ValidationResult.Invalid("Caesar Cipher requires a shift from 0 to 25.");
    }

    public AlgorithmOutput Process(CryptoRequest request)
    {
        var shift = int.Parse(request.GetParameter("shift"));
        var effectiveShift = request.Operation == CryptoOperation.Decrypt ? -shift : shift;
        var output = AlgorithmHelpers.TransformLatinLetters(request.InputText, x => x + effectiveShift);
        var steps = request.InputText.Zip(output, (input, result) => new VisualizationStep(
            input.ToString(), shift.ToString(), $"shift {effectiveShift}", result.ToString(),
            "Each Latin letter moves around a 26-letter alphabet. Non-Latin characters remain unchanged."));
        return new AlgorithmOutput(output, "Text", AlgorithmHelpers.LimitSteps(steps),
            "The Caesar cipher applies the same numeric shift to every alphabetic character. Decryption uses the opposite shift.");
    }
}

public sealed class AtbashCipherAlgorithm : ICryptoAlgorithm
{
    public AlgorithmMetadata Metadata { get; } = new(
        "atbash", "Atbash", AlgorithmCategory.ClassicalCipher, SecurityLevel.EducationalOnly,
        "A monoalphabetic substitution cipher that mirrors the alphabet.",
        "Known from classical Hebrew substitution examples.",
        "Useful for explaining substitution tables and symmetric transformations.",
        new[] { "No key", "Same operation encrypts and decrypts", "Alphabet mirror", "Educational only" },
        true, true, false, false, "No key required", "Educational use only. Atbash is trivially breakable.");

    public ValidationResult Validate(CryptoRequest request) => ValidationResult.Valid;

    public AlgorithmOutput Process(CryptoRequest request)
    {
        var output = AlgorithmHelpers.TransformLatinLetters(request.InputText, x => 25 - x);
        var steps = request.InputText.Zip(output, (input, result) => new VisualizationStep(
            input.ToString(), "alphabet mirror", "A<->Z", result.ToString(),
            "The alphabet is reflected so the first letter maps to the last letter."));
        return new AlgorithmOutput(output, "Text", AlgorithmHelpers.LimitSteps(steps),
            "Atbash reverses the alphabet. Applying it twice returns the original text.");
    }
}

public sealed class Rot13CipherAlgorithm : ICryptoAlgorithm
{
    public AlgorithmMetadata Metadata { get; } = new(
        "rot13", "ROT13", AlgorithmCategory.ClassicalCipher, SecurityLevel.EducationalOnly,
        "A Caesar variant that always shifts letters by 13 positions.",
        "Historically used to hide spoilers or puzzle answers, not to secure data.",
        "Useful as a compact demonstration of reversible substitution.",
        new[] { "Fixed shift", "No key", "Same operation encrypts and decrypts", "Educational only" },
        true, true, false, false, "No key required", "Educational use only. ROT13 is not encryption for sensitive data.");

    public ValidationResult Validate(CryptoRequest request) => ValidationResult.Valid;

    public AlgorithmOutput Process(CryptoRequest request)
    {
        var output = AlgorithmHelpers.TransformLatinLetters(request.InputText, x => x + 13);
        var steps = request.InputText.Zip(output, (input, result) => new VisualizationStep(
            input.ToString(), "13", "rotate 13", result.ToString(),
            "ROT13 moves each Latin letter halfway around the alphabet."));
        return new AlgorithmOutput(output, "Text", AlgorithmHelpers.LimitSteps(steps),
            "ROT13 is its own inverse because shifting by 13 twice completes a 26-letter cycle.");
    }
}

public sealed class VigenereCipherAlgorithm : ICryptoAlgorithm
{
    public AlgorithmMetadata Metadata { get; } = new(
        "vigenere", "Vigenere", AlgorithmCategory.ClassicalCipher, SecurityLevel.EducationalOnly,
        "A polyalphabetic cipher that uses a repeated keyword to choose different Caesar shifts.",
        "Known as a stronger classical cipher before modern cryptanalysis.",
        "Useful for teaching repeated keys and frequency analysis limitations.",
        new[] { "Keyword key", "Variable shift per letter", "Reversible", "Vulnerable to key-length analysis" },
        true, true, false, true, "Alphabetic keyword",
        "Educational use only. Repeated-key Vigenere is not modern secure encryption.");

    public ValidationResult Validate(CryptoRequest request)
    {
        var keyword = request.GetParameter("keyword").Trim();
        return !string.IsNullOrEmpty(keyword) && keyword.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z')
            ? ValidationResult.Valid
            : ValidationResult.Invalid("Vigenere requires an English alphabetic keyword.");
    }

    public AlgorithmOutput Process(CryptoRequest request)
    {
        var keyword = request.GetParameter("keyword").Trim();
        var keyShifts = keyword.Select(c => char.ToUpperInvariant(c) - 'A').ToArray();
        var keyIndex = 0;
        var output = new StringBuilder(request.InputText.Length);
        var steps = new List<VisualizationStep>();
        foreach (var c in request.InputText)
        {
            if (c is >= 'A' and <= 'Z' or >= 'a' and <= 'z')
            {
                var shift = keyShifts[keyIndex % keyShifts.Length];
                var effectiveShift = request.Operation == CryptoOperation.Decrypt ? -shift : shift;
                var transformed = AlgorithmHelpers.TransformLatinLetters(c.ToString(), x => x + effectiveShift)[0];
                output.Append(transformed);
                steps.Add(new VisualizationStep(c.ToString(), keyword[keyIndex % keyword.Length].ToString(), $"shift {effectiveShift}", transformed.ToString(), "The keyword chooses the Caesar shift for this character."));
                keyIndex++;
            }
            else
            {
                output.Append(c);
                steps.Add(new VisualizationStep(c.ToString(), "-", "unchanged", c.ToString(), "Non-Latin characters are preserved."));
            }
        }
        return new AlgorithmOutput(output.ToString(), "Text", AlgorithmHelpers.LimitSteps(steps),
            "Vigenere repeats the keyword over the plaintext. Each key letter becomes a different shift.");
    }
}

public sealed class AffineCipherAlgorithm : ICryptoAlgorithm
{
    public AlgorithmMetadata Metadata { get; } = new(
        "affine", "Affine Cipher", AlgorithmCategory.ClassicalCipher, SecurityLevel.EducationalOnly,
        "A substitution cipher using the formula E(x) = (a*x + b) mod 26.",
        "A classical teaching cipher for modular arithmetic.",
        "Useful for explaining modular inverses and valid key constraints.",
        new[] { "Two numeric keys", "Requires gcd(a, 26) = 1", "Reversible", "Educational only" },
        true, true, false, true, "a and b numeric values",
        "Educational use only. Affine Cipher is vulnerable to basic cryptanalysis.");

    public ValidationResult Validate(CryptoRequest request)
    {
        if (!AlgorithmHelpers.TryReadInt(request, "a", out var a) || !AlgorithmHelpers.TryReadInt(request, "b", out _))
        {
            return ValidationResult.Invalid("Affine Cipher requires numeric keys a and b.");
        }
        return AlgorithmHelpers.Gcd(a, 26) == 1
            ? ValidationResult.Valid
            : ValidationResult.Invalid("Affine key a must be coprime with 26. Try 1, 3, 5, 7, 11, 15, 17, 19, 21, 23, or 25.");
    }

    public AlgorithmOutput Process(CryptoRequest request)
    {
        var a = int.Parse(request.GetParameter("a"));
        var b = int.Parse(request.GetParameter("b"));
        Func<int, int> transform = request.Operation == CryptoOperation.Encrypt
            ? x => a * x + b
            : x => AlgorithmHelpers.Mod(AlgorithmHelpers.ModularInverse(a, 26) * (x - b), 26);
        var output = AlgorithmHelpers.TransformLatinLetters(request.InputText, transform);
        var steps = request.InputText.Zip(output, (input, result) => new VisualizationStep(
            input.ToString(), $"a={a}, b={b}", request.Operation == CryptoOperation.Encrypt ? "(a*x+b) mod 26" : "a^-1*(y-b) mod 26", result.ToString(),
            "The character index is transformed with modular arithmetic."));
        return new AlgorithmOutput(output, "Text", AlgorithmHelpers.LimitSteps(steps),
            "Affine Cipher combines multiplication and addition in modular arithmetic. Decryption requires a valid modular inverse.");
    }
}

public sealed class RailFenceCipherAlgorithm : ICryptoAlgorithm
{
    public AlgorithmMetadata Metadata { get; } = new(
        "rail-fence", "Rail Fence", AlgorithmCategory.ClassicalCipher, SecurityLevel.EducationalOnly,
        "A transposition cipher that writes text in a zigzag pattern across rails.",
        "Used as a simple historical transposition example.",
        "Useful for teaching that transposition changes order rather than replacing symbols.",
        new[] { "Rail count", "Reorders characters", "Reversible", "Educational only" },
        true, true, false, true, "Number of rails", "Educational use only. Rail Fence is not secure for sensitive data.");

    public ValidationResult Validate(CryptoRequest request)
    {
        return AlgorithmHelpers.TryReadInt(request, "rails", out var rails) && rails >= 2
            ? ValidationResult.Valid
            : ValidationResult.Invalid("Rail Fence requires at least 2 rails.");
    }

    public AlgorithmOutput Process(CryptoRequest request)
    {
        var rails = int.Parse(request.GetParameter("rails"));
        var output = request.Operation == CryptoOperation.Encrypt ? Encrypt(request.InputText, rails) : Decrypt(request.InputText, rails);
        return new AlgorithmOutput(output, "Text", BuildRailSteps(request.InputText, output, rails),
            "Rail Fence writes the message diagonally across rows, then reads each row in order. Decryption rebuilds the zigzag positions.");
    }

    private static string Encrypt(string input, int rails)
    {
        if (rails >= input.Length) return input;
        var rows = Enumerable.Range(0, rails).Select(_ => new StringBuilder()).ToArray();
        var rail = 0;
        var direction = 1;
        foreach (var c in input)
        {
            rows[rail].Append(c);
            if (rail == 0) direction = 1;
            else if (rail == rails - 1) direction = -1;
            rail += direction;
        }
        return string.Concat(rows.Select(r => r.ToString()));
    }

    private static string Decrypt(string cipher, int rails)
    {
        if (rails >= cipher.Length) return cipher;
        var pattern = BuildPattern(cipher.Length, rails);
        var counts = new int[rails];
        foreach (var rail in pattern) counts[rail]++;
        var rows = new Queue<char>[rails];
        var offset = 0;
        for (var i = 0; i < rails; i++)
        {
            rows[i] = new Queue<char>(cipher.Substring(offset, counts[i]));
            offset += counts[i];
        }
        var builder = new StringBuilder(cipher.Length);
        foreach (var rail in pattern) builder.Append(rows[rail].Dequeue());
        return builder.ToString();
    }

    private static IReadOnlyList<int> BuildPattern(int length, int rails)
    {
        var pattern = new List<int>(length);
        var rail = 0;
        var direction = 1;
        for (var i = 0; i < length; i++)
        {
            pattern.Add(rail);
            if (rail == 0) direction = 1;
            else if (rail == rails - 1) direction = -1;
            rail += direction;
        }
        return pattern;
    }

    private static IReadOnlyList<VisualizationStep> BuildRailSteps(string input, string output, int rails)
    {
        return AlgorithmHelpers.LimitSteps(input.Take(80).Select((c, i) =>
            new VisualizationStep(c.ToString(), rails.ToString(), $"zigzag index {i}", i < output.Length ? output[i].ToString() : string.Empty, "Characters are placed on rails and read by row.")));
    }
}

public sealed class ColumnarTranspositionAlgorithm : ICryptoAlgorithm
{
    public AlgorithmMetadata Metadata { get; } = new(
        "columnar", "Columnar Transposition", AlgorithmCategory.ClassicalCipher, SecurityLevel.EducationalOnly,
        "A transposition cipher that fills a grid and reads columns in key order.",
        "Common in manual cryptography before modern machines.",
        "Useful for teaching permutation keys and grid-based transposition.",
        new[] { "Keyword order", "Grid transposition", "Reversible", "Educational only" },
        true, true, false, true, "Keyword", "Educational use only. Columnar transposition is not modern secure encryption.");

    public ValidationResult Validate(CryptoRequest request)
    {
        return string.IsNullOrWhiteSpace(request.GetParameter("key"))
            ? ValidationResult.Invalid("Columnar Transposition requires a keyword.")
            : ValidationResult.Valid;
    }

    public AlgorithmOutput Process(CryptoRequest request)
    {
        var key = request.GetParameter("key").Trim();
        var output = request.Operation == CryptoOperation.Encrypt ? Encrypt(request.InputText, key) : Decrypt(request.InputText, key);
        return new AlgorithmOutput(output, "Text", BuildSteps(request.InputText, output, key),
            "Columnar Transposition writes the text into a grid, then reads columns according to the sorted keyword order.");
    }

    private static int[] ColumnOrder(string key)
    {
        return key.Select((c, i) => new { c = char.ToUpperInvariant(c), i }).OrderBy(x => x.c).ThenBy(x => x.i).Select(x => x.i).ToArray();
    }

    private static string Encrypt(string input, string key)
    {
        var columns = key.Length;
        var rows = (int)Math.Ceiling(input.Length / (double)columns);
        var padded = input.PadRight(rows * columns, ' ');
        var builder = new StringBuilder(padded.Length);
        foreach (var column in ColumnOrder(key))
        {
            for (var row = 0; row < rows; row++) builder.Append(padded[row * columns + column]);
        }
        return builder.ToString().TrimEnd();
    }

    private static string Decrypt(string cipher, string key)
    {
        var columns = key.Length;
        var rows = (int)Math.Ceiling(cipher.Length / (double)columns);
        var total = rows * columns;
        var padded = cipher.PadRight(total, ' ');
        var grid = new char[rows, columns];
        var index = 0;
        foreach (var column in ColumnOrder(key))
        {
            for (var row = 0; row < rows; row++) grid[row, column] = padded[index++];
        }
        var builder = new StringBuilder(total);
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++) builder.Append(grid[row, column]);
        }
        return builder.ToString().TrimEnd();
    }

    private static IReadOnlyList<VisualizationStep> BuildSteps(string input, string output, string key)
    {
        return AlgorithmHelpers.LimitSteps(input.Take(80).Select((c, i) =>
            new VisualizationStep(c.ToString(), key, $"grid cell {i}", i < output.Length ? output[i].ToString() : string.Empty, "The key determines the column read order.")));
    }
}
