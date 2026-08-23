namespace CryptoLab.Core.Models;

public sealed class CryptoRequest
{
    public string AlgorithmId { get; init; } = string.Empty;
    public CryptoOperation Operation { get; init; }
    public string InputText { get; init; } = string.Empty;
    public Dictionary<string, string> Parameters { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public string GetParameter(string name, string defaultValue = "")
    {
        return Parameters.TryGetValue(name, out var value) ? value : defaultValue;
    }
}
