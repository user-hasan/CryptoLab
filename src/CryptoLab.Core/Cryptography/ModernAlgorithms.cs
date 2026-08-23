using System.Security.Cryptography;
using System.Text;
using CryptoLab.Core.Models;

namespace CryptoLab.Core.Cryptography;

public abstract class BlockCipherAlgorithmBase : ICryptoAlgorithm
{
    public abstract AlgorithmMetadata Metadata { get; }
    protected abstract SymmetricAlgorithm CreateAlgorithm();
    protected abstract int[] AllowedKeyLengths { get; }
    protected abstract string PackageName { get; }

    public ValidationResult Validate(CryptoRequest request)
    {
        if (request.Operation == CryptoOperation.Decrypt && !request.InputText.StartsWith($"CLAB1|{PackageName}|", StringComparison.OrdinalIgnoreCase))
        {
            return ValidationResult.Invalid($"Decrypt expects a CryptoLab {Metadata.Name} package produced by this app.");
        }

        var key = request.GetParameter("key").Trim();
        if (string.IsNullOrEmpty(key))
        {
            return ValidationResult.Invalid($"{Metadata.Name} requires a key.");
        }

        var keyLength = Encoding.UTF8.GetByteCount(key);
        if (!AllowedKeyLengths.Contains(keyLength))
        {
            return ValidationResult.Invalid($"Invalid {Metadata.Name} key length. Use {string.Join(", ", AllowedKeyLengths)} UTF-8 bytes.");
        }

        var mode = request.GetParameter("mode", "CBC");
        if (!string.Equals(mode, "CBC", StringComparison.OrdinalIgnoreCase) && !string.Equals(mode, "ECB", StringComparison.OrdinalIgnoreCase))
        {
            return ValidationResult.Invalid("Mode must be CBC or ECB.");
        }

        return ValidationResult.Valid;
    }

    public AlgorithmOutput Process(CryptoRequest request)
    {
        return request.Operation == CryptoOperation.Encrypt ? Encrypt(request) : Decrypt(request);
    }

    private AlgorithmOutput Encrypt(CryptoRequest request)
    {
        using var algorithm = CreateAlgorithm();
        algorithm.Key = AlgorithmHelpers.ReadUtf8Key(request, "key", AllowedKeyLengths);
        algorithm.Mode = ParseMode(request.GetParameter("mode", "CBC"));
        algorithm.Padding = PaddingMode.PKCS7;
        var iv = algorithm.Mode == CipherMode.ECB ? Array.Empty<byte>() : AlgorithmHelpers.ReadOptionalFixedBytes(request.GetParameter("iv"), algorithm.BlockSize / 8, "IV");
        if (iv.Length > 0) algorithm.IV = iv;
        var inputBytes = Encoding.UTF8.GetBytes(request.InputText);
        using var encryptor = algorithm.CreateEncryptor();
        var cipherBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        var package = $"CLAB1|{PackageName}|{algorithm.Mode}|{Convert.ToBase64String(iv)}|{Convert.ToBase64String(cipherBytes)}";
        var steps = new[]
        {
            new VisualizationStep("Plaintext", algorithm.Key.Length * 8 + "-bit key", "UTF-8 bytes", inputBytes.Length + " bytes", "Text is encoded before encryption."),
            new VisualizationStep("IV", algorithm.Mode.ToString(), "random or provided", iv.Length == 0 ? "not used" : Convert.ToBase64String(iv), "CBC mode uses an IV so repeated messages produce different ciphertext."),
            new VisualizationStep("Ciphertext", PackageName, "PKCS7 padding", Convert.ToBase64String(cipherBytes), "The package stores metadata needed for safe decryption in this learning lab.")
        };
        return new AlgorithmOutput(package, "CryptoLab Base64 Package", steps, $"{Metadata.Name} encrypts UTF-8 bytes with a validated key and PKCS7 padding. The output is Base64 encoded with its mode and IV metadata.");
    }

    private AlgorithmOutput Decrypt(CryptoRequest request)
    {
        var parts = request.InputText.Split('|');
        if (parts.Length != 5 || !string.Equals(parts[1], PackageName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid {Metadata.Name} package.");
        }

        using var algorithm = CreateAlgorithm();
        algorithm.Key = AlgorithmHelpers.ReadUtf8Key(request, "key", AllowedKeyLengths);
        algorithm.Mode = ParseMode(parts[2]);
        algorithm.Padding = PaddingMode.PKCS7;
        var iv = string.IsNullOrEmpty(parts[3]) ? Array.Empty<byte>() : Convert.FromBase64String(parts[3]);
        if (iv.Length > 0) algorithm.IV = iv;
        var cipherBytes = Convert.FromBase64String(parts[4]);
        using var decryptor = algorithm.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        var output = Encoding.UTF8.GetString(plainBytes);
        var steps = new[]
        {
            new VisualizationStep("Ciphertext package", algorithm.Key.Length * 8 + "-bit key", "parse metadata", parts[4], "The package provides mode, IV, and ciphertext."),
            new VisualizationStep("Decrypt", algorithm.Mode.ToString(), "remove PKCS7 padding", plainBytes.Length + " bytes", "The validated key reverses the block-cipher operation."),
            new VisualizationStep("Plaintext", "UTF-8", "decode bytes", output, "The decrypted bytes are decoded back to text.")
        };
        return new AlgorithmOutput(output, "UTF-8 Text", steps, $"{Metadata.Name} decryption parses the CryptoLab package, restores the IV and mode, and decodes the plaintext as UTF-8.");
    }

    private static CipherMode ParseMode(string mode)
    {
        return string.Equals(mode, "ECB", StringComparison.OrdinalIgnoreCase) ? CipherMode.ECB : CipherMode.CBC;
    }
}

public sealed class AesAlgorithm : BlockCipherAlgorithmBase
{
    public override AlgorithmMetadata Metadata { get; } = new(
        "aes", "AES", AlgorithmCategory.SymmetricEncryption, SecurityLevel.Modern,
        "A modern symmetric block cipher widely used for protecting data.",
        "Standardized by NIST in 2001 after the Rijndael competition.",
        "Used in file encryption, TLS internals, disk encryption designs, and secure protocols.",
        new[] { "Symmetric shared key", "128-bit block size", "Fast", "Use authenticated modes in production systems" },
        true, true, false, true, "16, 24, or 32 UTF-8 byte key",
        "AES is modern, but this lab mode is educational and does not replace audited production security tooling.");
    protected override int[] AllowedKeyLengths { get; } = { 16, 24, 32 };
    protected override string PackageName => "AES";
    protected override SymmetricAlgorithm CreateAlgorithm() => Aes.Create();
}

public sealed class DesAlgorithm : BlockCipherAlgorithmBase
{
    public override AlgorithmMetadata Metadata { get; } = new(
        "des", "DES", AlgorithmCategory.SymmetricEncryption, SecurityLevel.Legacy,
        "A legacy symmetric block cipher with a 56-bit effective key size.",
        "Historically important as a former U.S. federal standard.",
        "Kept only for compatibility and education; it should not protect sensitive data.",
        new[] { "Legacy", "Small key size", "64-bit block", "Educational or compatibility only" },
        true, true, false, true, "8 UTF-8 byte key",
        "Educational use only. DES is obsolete and vulnerable to brute-force attacks.");
    protected override int[] AllowedKeyLengths { get; } = { 8 };
    protected override string PackageName => "DES";
    protected override SymmetricAlgorithm CreateAlgorithm() => DES.Create();
}

public sealed class TripleDesAlgorithm : BlockCipherAlgorithmBase
{
    public override AlgorithmMetadata Metadata { get; } = new(
        "3des", "3DES", AlgorithmCategory.SymmetricEncryption, SecurityLevel.Legacy,
        "A legacy construction that applies DES three times with longer keys.",
        "Used to extend the useful life of DES in older payment and enterprise systems.",
        "Kept for compatibility discussions; new systems should use AES or authenticated modern designs.",
        new[] { "Legacy", "16 or 24 byte key", "Slower than AES", "Compatibility only" },
        true, true, false, true, "16 or 24 UTF-8 byte key",
        "Educational use only. 3DES is deprecated for new designs.");
    protected override int[] AllowedKeyLengths { get; } = { 16, 24 };
    protected override string PackageName => "3DES";
    protected override SymmetricAlgorithm CreateAlgorithm() => TripleDES.Create();
}

public sealed class RsaAlgorithm : ICryptoAlgorithm
{
    public AlgorithmMetadata Metadata { get; } = new(
        "rsa", "RSA", AlgorithmCategory.AsymmetricCryptography, SecurityLevel.Modern,
        "An asymmetric cryptosystem using a public key for encryption and a private key for decryption.",
        "Introduced in 1977 and foundational for public-key cryptography education.",
        "Used mainly for key exchange, signatures, and small payload encryption patterns, not bulk data.",
        new[] { "Public/private key pair", "OAEP SHA-256 padding", "Payload size limit", "Slower than symmetric encryption" },
        true, true, false, true, "PEM public key for encrypt, PEM private key for decrypt",
        "RSA is modern only when configured correctly. This app uses OAEP SHA-256 for educational experiments.");

    public ValidationResult Validate(CryptoRequest request)
    {
        if (request.Operation == CryptoOperation.Encrypt && string.IsNullOrWhiteSpace(request.GetParameter("publicKey")))
        {
            return ValidationResult.Invalid("RSA encryption requires a PEM public key.");
        }

        if (request.Operation == CryptoOperation.Decrypt && string.IsNullOrWhiteSpace(request.GetParameter("privateKey")))
        {
            return ValidationResult.Invalid("RSA decryption requires a PEM private key.");
        }

        return ValidationResult.Valid;
    }

    public AlgorithmOutput Process(CryptoRequest request)
    {
        return request.Operation == CryptoOperation.Encrypt ? Encrypt(request) : Decrypt(request);
    }

    public static (string PublicKeyPem, string PrivateKeyPem) GenerateKeyPair(int keySize)
    {
        if (keySize is not (2048 or 3072 or 4096))
        {
            throw new ArgumentException("RSA key size must be 2048, 3072, or 4096 bits.");
        }

        using var rsa = RSA.Create(keySize);
        return (rsa.ExportSubjectPublicKeyInfoPem(), rsa.ExportPkcs8PrivateKeyPem());
    }

    private static AlgorithmOutput Encrypt(CryptoRequest request)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(request.GetParameter("publicKey"));
        var data = Encoding.UTF8.GetBytes(request.InputText);
        var maxPayload = rsa.KeySize / 8 - 2 * 32 - 2;
        if (data.Length > maxPayload)
        {
            throw new ArgumentException($"RSA payload is too large for this key and OAEP SHA-256. Use AES for bulk text or keep RSA input under {maxPayload} bytes.");
        }
        var cipher = rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
        var output = Convert.ToBase64String(cipher);
        var steps = new[]
        {
            new VisualizationStep("Plaintext", rsa.KeySize + "-bit public key", "OAEP SHA-256", data.Length + " bytes", "RSA encrypts only small payloads."),
            new VisualizationStep("Ciphertext", "public key", "modular exponentiation", output, "The private key is required to reverse this operation.")
        };
        return new AlgorithmOutput(output, "Base64", steps, "RSA encryption uses the public key and OAEP SHA-256 padding. It is suitable for small payloads and teaching public-key concepts.");
    }

    private static AlgorithmOutput Decrypt(CryptoRequest request)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(request.GetParameter("privateKey"));
        var cipher = Convert.FromBase64String(request.InputText.Trim());
        var data = rsa.Decrypt(cipher, RSAEncryptionPadding.OaepSHA256);
        var output = Encoding.UTF8.GetString(data);
        var steps = new[]
        {
            new VisualizationStep("Ciphertext", rsa.KeySize + "-bit private key", "OAEP SHA-256 decrypt", cipher.Length + " bytes", "Only the matching private key can decrypt the message."),
            new VisualizationStep("Plaintext", "UTF-8", "decode", output, "The decrypted bytes return to readable text.")
        };
        return new AlgorithmOutput(output, "UTF-8 Text", steps, "RSA decryption uses the private key corresponding to the public key used for encryption.");
    }
}
