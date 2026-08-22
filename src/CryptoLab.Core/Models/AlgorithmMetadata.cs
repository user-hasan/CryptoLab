namespace CryptoLab.Core.Models;

public sealed record AlgorithmMetadata(
    string Id,
    string Name,
    AlgorithmCategory Category,
    SecurityLevel Security,
    string Description,
    string HistoricalUsage,
    string ModernUsage,
    IReadOnlyList<string> KeyCharacteristics,
    bool SupportsEncryption,
    bool SupportsDecryption,
    bool SupportsHashing,
    bool RequiresKey,
    string KeyHint,
    string Notice)
{
    public bool IsReversible => SupportsEncryption && SupportsDecryption;
}
