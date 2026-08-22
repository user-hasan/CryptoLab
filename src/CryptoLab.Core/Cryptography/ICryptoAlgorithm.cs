namespace CryptoLab.Core.Cryptography;

using CryptoLab.Core.Models;

public interface ICryptoAlgorithm
{
    AlgorithmMetadata Metadata { get; }
    ValidationResult Validate(CryptoRequest request);
    AlgorithmOutput Process(CryptoRequest request);
}

public sealed record ValidationResult(bool IsValid, string Message)
{
    public static ValidationResult Valid { get; } = new(true, string.Empty);
    public static ValidationResult Invalid(string message) => new(false, message);
}
