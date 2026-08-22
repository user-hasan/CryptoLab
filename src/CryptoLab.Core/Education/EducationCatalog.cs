using CryptoLab.Core.Cryptography;
using CryptoLab.Core.Models;

namespace CryptoLab.Core.Education;

public static class EducationCatalog
{
    public static IReadOnlyList<Lesson> Lessons { get; } = new[]
    {
        Lesson("what-is-cryptography", "What is Cryptography?", "Cryptography protects information by transforming it into forms that only intended parties can understand.",
            "Cryptography is the science of designing methods for confidentiality, integrity, authentication, and non-repudiation.",
            "A plaintext message is readable. A ciphertext message is transformed so it is not useful without the required secret or private key.",
            "Modern cryptography depends on public algorithms and secret keys, not hidden algorithms."),
        Lesson("encryption-decryption", "Encryption vs Decryption", "Encryption transforms plaintext into ciphertext. Decryption reverses it when the correct key and parameters are available.",
            "Symmetric encryption uses the same shared key for encryption and decryption.",
            "Asymmetric encryption uses a public key and a private key pair.",
            "Hashing is different because it is one-way and has no decrypt operation."),
        Lesson("plaintext-ciphertext", "Plaintext vs Ciphertext", "Plaintext is the original readable input; ciphertext is the transformed output.",
            "Text must often be encoded into bytes before modern algorithms process it.",
            "Ciphertext is commonly displayed as Base64 or hexadecimal so it can be copied safely.",
            "Changing one byte of ciphertext can make decryption fail or produce invalid data."),
        Lesson("keys", "Keys", "A key controls the transformation performed by a cryptographic algorithm.",
            "Classical ciphers often use small human-readable keys such as shifts or keywords.",
            "Modern algorithms require strict key sizes and random values for security.",
            "Never reuse weak, predictable, or exposed keys in real systems."),
        Lesson("symmetric", "Symmetric Encryption", "Symmetric encryption uses one shared key for both encryption and decryption.",
            "AES is the common modern example and is efficient for large data.",
            "Modes and IVs matter. Reusing an IV incorrectly can weaken security.",
            "Production systems should use authenticated encryption or audited protocols."),
        Lesson("asymmetric", "Asymmetric Encryption", "Asymmetric cryptography uses public/private key pairs.",
            "Public keys can be shared. Private keys must stay secret.",
            "RSA is useful for teaching public-key concepts but is not used for bulk encryption.",
            "Hybrid encryption commonly uses RSA or ECDH to protect a symmetric key."),
        Lesson("hashing", "Hashing", "Hashing maps input to a fixed-size digest and is designed to be one-way.",
            "Hashes are used for integrity checks and as building blocks in digital signatures.",
            "MD5 and SHA-1 are legacy and unsafe for collision-resistant security.",
            "A hash cannot normally be decrypted back to the original input."),
        Lesson("digital-signatures", "Digital Signatures", "Digital signatures prove that a private key holder approved a message or digest.",
            "A signer uses a private key; verifiers use the matching public key.",
            "Signatures provide authenticity and integrity, not confidentiality.",
            "They are different from encryption even when similar key pairs are involved."),
        Lesson("public-key", "Public Key Cryptography", "Public-key systems allow secure interactions without pre-sharing the same secret key.",
            "They enable key exchange, signatures, and identity checks.",
            "They are slower than symmetric primitives and must be combined carefully.",
            "Trust still depends on key validation and certificate or identity models."),
        Lesson("attacks", "Cryptographic Attacks", "Cryptanalysis studies how systems fail under realistic assumptions.",
            "Brute force tries many possible keys. Small keyspaces fail quickly.",
            "Frequency analysis attacks simple substitution ciphers.",
            "Implementation errors, bad randomness, and key leaks often break real systems."),
        Lesson("classical", "Classical Ciphers", "Classical ciphers are excellent learning tools but are not secure today.",
            "Caesar, Atbash, Vigenere, and transposition ciphers show substitution and permutation ideas.",
            "They are usually breakable by hand or by small scripts.",
            "Use them to understand concepts, not to protect sensitive information."),
        Lesson("modern", "Modern Cryptography", "Modern cryptography combines carefully analyzed primitives, protocols, and implementations.",
            "Algorithms such as AES, RSA, SHA-2, and modern signature schemes are public and standardized.",
            "Security depends on key management, protocol design, randomness, and implementation quality.",
            "Educational labs should clearly separate demonstrations from production security claims.")
    };

    public static string ExplainAlgorithm(string algorithmId)
    {
        var metadata = AlgorithmRegistry.CreateAlgorithms().Select(a => a.Metadata).FirstOrDefault(a => a.Id == algorithmId);
        if (metadata is null) return "Select an algorithm to view its educational explanation.";
        return $"{metadata.Name}: {metadata.Description}\n\nHistorical usage: {metadata.HistoricalUsage}\nModern relevance: {metadata.ModernUsage}\nSecurity notice: {metadata.Notice}";
    }

    private static Lesson Lesson(string id, string title, string summary, params string[] sections)
    {
        return new Lesson(id, title, summary, sections, new[] { "Try the related algorithm in Encrypt / Decrypt.", "Explain why this topic matters for real security." });
    }
}
