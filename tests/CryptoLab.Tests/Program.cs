using CryptoLab.Core.Cryptography;
using CryptoLab.Core.Models;
using CryptoLab.Core.Storage;

namespace CryptoLab.Tests;

internal static class Program
{
    private static int _passed;
    private static int _failed;

    private static int Main()
    {
        Run("Caesar roundtrip with English and Arabic text", CaesarRoundTrip);
        Run("Vigenere roundtrip", VigenereRoundTrip);
        Run("Atbash and ROT13 roundtrip", SimpleClassicalRoundTrip);
        Run("Rail Fence roundtrip", RailFenceRoundTrip);
        Run("Columnar transposition roundtrip", ColumnarRoundTrip);
        Run("AES roundtrip with Unicode text", AesRoundTrip);
        Run("RSA key generation and roundtrip", RsaRoundTrip);
        Run("SHA-256 known digest and hash is one-way", HashingBehavior);
        Run("Validation rejects empty input and invalid keys", ValidationBehavior);
        Run("Settings and history storage do not require plaintext", StorageBehavior);

        Console.WriteLine($"Passed: {_passed}, Failed: {_failed}");
        return _failed == 0 ? 0 : 1;
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            _passed++;
            Console.WriteLine($"[PASS] {name}");
        }
        catch (Exception ex)
        {
            _failed++;
            Console.WriteLine($"[FAIL] {name}: {ex.Message}");
        }
    }

    private static void CaesarRoundTrip()
    {
        var engine = new CryptoEngine();
        var text = "HELLO جامعة CryptoLab";
        var encrypted = Execute(engine, "caesar", CryptoOperation.Encrypt, text, new() { ["shift"] = "3" });
        Assert(encrypted.Output.StartsWith("KHOOR"), "Caesar encryption did not shift English letters.");
        var decrypted = Execute(engine, "caesar", CryptoOperation.Decrypt, encrypted.Output, new() { ["shift"] = "3" });
        AssertEqual(text, decrypted.Output, "Caesar decrypt(encrypt(text)) failed.");
    }

    private static void VigenereRoundTrip()
    {
        var engine = new CryptoEngine();
        var text = "ATTACK AT DAWN";
        var parameters = new Dictionary<string, string> { ["keyword"] = "LEMON" };
        var encrypted = Execute(engine, "vigenere", CryptoOperation.Encrypt, text, parameters);
        var decrypted = Execute(engine, "vigenere", CryptoOperation.Decrypt, encrypted.Output, parameters);
        AssertEqual(text, decrypted.Output, "Vigenere roundtrip failed.");
    }

    private static void SimpleClassicalRoundTrip()
    {
        var engine = new CryptoEngine();
        foreach (var id in new[] { "atbash", "rot13" })
        {
            var text = "Simple Text 123";
            var encrypted = Execute(engine, id, CryptoOperation.Encrypt, text, new());
            var decrypted = Execute(engine, id, CryptoOperation.Decrypt, encrypted.Output, new());
            AssertEqual(text, decrypted.Output, $"{id} roundtrip failed.");
        }
    }

    private static void RailFenceRoundTrip()
    {
        var engine = new CryptoEngine();
        var parameters = new Dictionary<string, string> { ["rails"] = "3" };
        var encrypted = Execute(engine, "rail-fence", CryptoOperation.Encrypt, "WEAREDISCOVERED", parameters);
        var decrypted = Execute(engine, "rail-fence", CryptoOperation.Decrypt, encrypted.Output, parameters);
        AssertEqual("WEAREDISCOVERED", decrypted.Output, "Rail Fence roundtrip failed.");
    }

    private static void ColumnarRoundTrip()
    {
        var engine = new CryptoEngine();
        var parameters = new Dictionary<string, string> { ["key"] = "ZEBRA" };
        var encrypted = Execute(engine, "columnar", CryptoOperation.Encrypt, "MEETATNOON", parameters);
        var decrypted = Execute(engine, "columnar", CryptoOperation.Decrypt, encrypted.Output, parameters);
        AssertEqual("MEETATNOON", decrypted.Output, "Columnar roundtrip failed.");
    }

    private static void AesRoundTrip()
    {
        var engine = new CryptoEngine();
        var text = new string('A', 4096) + " نص عربي";
        var parameters = new Dictionary<string, string> { ["key"] = "1234567890abcdef", ["mode"] = "CBC", ["iv"] = "" };
        var encrypted = Execute(engine, "aes", CryptoOperation.Encrypt, text, parameters);
        Assert(encrypted.Output.StartsWith("CLAB1|AES|CBC|"), "AES output package missing metadata.");
        var decrypted = Execute(engine, "aes", CryptoOperation.Decrypt, encrypted.Output, parameters);
        AssertEqual(text, decrypted.Output, "AES roundtrip failed.");
    }

    private static void RsaRoundTrip()
    {
        var engine = new CryptoEngine();
        var keys = RsaAlgorithm.GenerateKeyPair(2048);
        var encrypted = Execute(engine, "rsa", CryptoOperation.Encrypt, "RSA small payload", new() { ["publicKey"] = keys.PublicKeyPem });
        var decrypted = Execute(engine, "rsa", CryptoOperation.Decrypt, encrypted.Output, new() { ["privateKey"] = keys.PrivateKeyPem });
        AssertEqual("RSA small payload", decrypted.Output, "RSA roundtrip failed.");
    }

    private static void HashingBehavior()
    {
        var engine = new CryptoEngine();
        var result = Execute(engine, "sha256", CryptoOperation.Hash, "hello", new());
        AssertEqual("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824", result.Output, "SHA-256 digest mismatch.");
        var decrypt = engine.Execute(new CryptoRequest { AlgorithmId = "sha256", Operation = CryptoOperation.Decrypt, InputText = result.Output });
        Assert(!decrypt.Success, "Hash decrypt should fail.");
    }

    private static void ValidationBehavior()
    {
        var engine = new CryptoEngine();
        var empty = engine.Execute(new CryptoRequest { AlgorithmId = "caesar", Operation = CryptoOperation.Encrypt, InputText = "", Parameters = new() { ["shift"] = "3" } });
        Assert(!empty.Success && empty.StatusMessage.Contains("Input text"), "Empty input validation failed.");
        var invalid = engine.Execute(new CryptoRequest { AlgorithmId = "aes", Operation = CryptoOperation.Encrypt, InputText = "x", Parameters = new() { ["key"] = "short", ["mode"] = "CBC" } });
        Assert(!invalid.Success && invalid.StatusMessage.Contains("key length", StringComparison.OrdinalIgnoreCase), "Invalid key validation failed.");
    }

    private static void StorageBehavior()
    {
        var dir = Path.Combine(Path.GetTempPath(), "CryptoLabTests", Guid.NewGuid().ToString("N"));
        var store = new JsonFileStore(dir);
        store.SaveSettings(new AppSettings { Language = "ar", Theme = "Light", SaveHistory = true });
        store.SaveHistory(new[] { new OperationHistoryEntry { Algorithm = "AES", Operation = "Encrypt", Status = "Success", InputLength = 10, OutputLength = 30 } });
        var settings = store.LoadSettings();
        var history = store.LoadHistory();
        AssertEqual("ar", settings.Language, "Settings did not persist language.");
        Assert(history.Count == 1 && history[0].InputLength == 10, "History metadata did not persist.");
        Assert(!File.ReadAllText(Path.Combine(dir, "history.json")).Contains("secret", StringComparison.OrdinalIgnoreCase), "History should not contain plaintext by default.");
    }

    private static CryptoResult Execute(CryptoEngine engine, string algorithm, CryptoOperation operation, string text, Dictionary<string, string> parameters)
    {
        var result = engine.Execute(new CryptoRequest { AlgorithmId = algorithm, Operation = operation, InputText = text, Parameters = parameters });
        Assert(result.Success, result.StatusMessage);
        return result;
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void AssertEqual(string expected, string actual, string message)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{message} Expected '{expected}', got '{actual}'.");
        }
    }
}
