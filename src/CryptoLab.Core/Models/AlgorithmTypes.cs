namespace CryptoLab.Core.Models;

public enum AlgorithmCategory
{
    ClassicalCipher,
    SymmetricEncryption,
    AsymmetricCryptography,
    HashFunction
}

public enum CryptoOperation
{
    Encrypt,
    Decrypt,
    Hash
}

public enum SecurityLevel
{
    EducationalOnly,
    Weak,
    Legacy,
    Modern
}
