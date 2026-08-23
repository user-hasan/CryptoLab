namespace CryptoLab.Core.Models;

public sealed class OperationHistoryEntry
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
    public string Algorithm { get; set; } = string.Empty;
    public string AlgorithmId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int InputLength { get; set; }
    public int OutputLength { get; set; }
    public double ExecutionTimeMs { get; set; }
    public string Status { get; set; } = string.Empty;
}
