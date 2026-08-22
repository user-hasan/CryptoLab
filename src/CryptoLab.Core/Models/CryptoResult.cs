namespace CryptoLab.Core.Models;

public sealed record VisualizationStep(string Input, string Key, string Operation, string Output, string Detail);

public sealed record AlgorithmOutput(
    string Output,
    string Encoding,
    IReadOnlyList<VisualizationStep> Steps,
    string Explanation);

public sealed class CryptoResult
{
    public bool Success { get; init; }
    public string Output { get; init; } = string.Empty;
    public string StatusMessage { get; init; } = string.Empty;
    public AlgorithmMetadata? Algorithm { get; init; }
    public CryptoOperation Operation { get; init; }
    public int InputLength { get; init; }
    public int OutputLength { get; init; }
    public double ExecutionTimeMs { get; init; }
    public string Encoding { get; init; } = "UTF-8";
    public IReadOnlyList<VisualizationStep> Steps { get; init; } = Array.Empty<VisualizationStep>();
    public string Explanation { get; init; } = string.Empty;

    public static CryptoResult Failure(AlgorithmMetadata? algorithm, CryptoOperation operation, string input, string message, double elapsedMs = 0)
    {
        return new CryptoResult
        {
            Success = false,
            Algorithm = algorithm,
            Operation = operation,
            InputLength = input.Length,
            OutputLength = 0,
            StatusMessage = message,
            ExecutionTimeMs = elapsedMs
        };
    }

    public static CryptoResult SuccessResult(AlgorithmMetadata algorithm, CryptoOperation operation, string input, AlgorithmOutput output, double elapsedMs)
    {
        return new CryptoResult
        {
            Success = true,
            Algorithm = algorithm,
            Operation = operation,
            InputLength = input.Length,
            Output = output.Output,
            OutputLength = output.Output.Length,
            StatusMessage = "Success",
            ExecutionTimeMs = elapsedMs,
            Encoding = output.Encoding,
            Steps = output.Steps,
            Explanation = output.Explanation
        };
    }
}
