using System.Security.Cryptography;
using System.Text;
using CryptoLab.Core.Models;

namespace CryptoLab.Core.Cryptography;

public abstract class HashAlgorithmBase : ICryptoAlgorithm
{
    public abstract AlgorithmMetadata Metadata { get; }
    protected abstract byte[] ComputeHash(byte[] bytes);
    protected abstract string AlgorithmName { get; }

    public ValidationResult Validate(CryptoRequest request)
    {
        return request.Operation == CryptoOperation.Hash
            ? ValidationResult.Valid
            : ValidationResult.Invalid("Hash functions are one-way. Use the Hash operation, not Encrypt or Decrypt.");
    }

    public AlgorithmOutput Process(CryptoRequest request)
    {
        var inputBytes = Encoding.UTF8.GetBytes(request.InputText);
        var hash = ComputeHash(inputBytes);
        var hex = AlgorithmHelpers.ToHex(hash);
        var steps = new[]
        {
            new VisualizationStep("Input", "UTF-8", "encode", inputBytes.Length + " bytes", "Hashing starts by converting text into bytes."),
            new VisualizationStep("Digest", AlgorithmName, "one-way compression", hex, "A fixed-length digest is produced. It cannot normally be decrypted.")
        };
        return new AlgorithmOutput(hex, "Hex", steps,
            $"{AlgorithmName} maps input bytes to a fixed-length digest. Hashing is one-way and is not encryption.");
    }
}

public sealed class Md5HashAlgorithm : HashAlgorithmBase
{
    public override AlgorithmMetadata Metadata { get; } = new(
        "md5", "MD5", AlgorithmCategory.HashFunction, SecurityLevel.Legacy,
        "A fast legacy hash function that produces a 128-bit digest.",
        "Used widely in old checksums and file identification workflows.",
        "No longer collision-resistant. Use only for education or non-security legacy compatibility.",
        new[] { "One-way", "128-bit digest", "Broken collision resistance", "No decrypt operation" },
        false, false, true, false, "No key required",
        "Educational use only. MD5 is broken for security-sensitive use.");
    protected override string AlgorithmName => "MD5";
    protected override byte[] ComputeHash(byte[] bytes) => MD5.HashData(bytes);
}

public sealed class Sha1HashAlgorithm : HashAlgorithmBase
{
    public override AlgorithmMetadata Metadata { get; } = new(
        "sha1", "SHA-1", AlgorithmCategory.HashFunction, SecurityLevel.Legacy,
        "A legacy hash function that produces a 160-bit digest.",
        "Historically used in certificates, version control internals, and signatures.",
        "Deprecated for collision-resistant security; useful in class to discuss attacks.",
        new[] { "One-way", "160-bit digest", "Collision attacks exist", "No decrypt operation" },
        false, false, true, false, "No key required",
        "Educational use only. SHA-1 should not be used for new security designs.");
    protected override string AlgorithmName => "SHA-1";
    protected override byte[] ComputeHash(byte[] bytes) => SHA1.HashData(bytes);
}

public sealed class Sha256HashAlgorithm : HashAlgorithmBase
{
    public override AlgorithmMetadata Metadata { get; } = new(
        "sha256", "SHA-256", AlgorithmCategory.HashFunction, SecurityLevel.Modern,
        "A SHA-2 hash function that produces a 256-bit digest.",
        "Standardized as part of SHA-2 and widely used in modern systems.",
        "Used for integrity checks, password hashing building blocks, signatures, and blockchain systems.",
        new[] { "One-way", "256-bit digest", "Modern hash", "No decrypt operation" },
        false, false, true, false, "No key required",
        "SHA-256 is modern, but this lab output is educational and not a full password-storage design.");
    protected override string AlgorithmName => "SHA-256";
    protected override byte[] ComputeHash(byte[] bytes) => SHA256.HashData(bytes);
}

public sealed class Sha512HashAlgorithm : HashAlgorithmBase
{
    public override AlgorithmMetadata Metadata { get; } = new(
        "sha512", "SHA-512", AlgorithmCategory.HashFunction, SecurityLevel.Modern,
        "A SHA-2 hash function that produces a 512-bit digest.",
        "Used in systems that want larger digest sizes or perform well on 64-bit processors.",
        "Useful for integrity checks and as a primitive inside larger audited designs.",
        new[] { "One-way", "512-bit digest", "Modern hash", "No decrypt operation" },
        false, false, true, false, "No key required",
        "SHA-512 is modern, but hashing alone is not encryption and cannot be reversed.");
    protected override string AlgorithmName => "SHA-512";
    protected override byte[] ComputeHash(byte[] bytes) => SHA512.HashData(bytes);
}
