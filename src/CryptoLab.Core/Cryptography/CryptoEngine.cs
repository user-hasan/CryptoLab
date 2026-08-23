using System.Diagnostics;
using System.Security.Cryptography;
using CryptoLab.Core.Models;

namespace CryptoLab.Core.Cryptography;

public sealed class CryptoEngine
{
    private readonly Dictionary<string, ICryptoAlgorithm> _algorithms;

    public CryptoEngine()
        : this(AlgorithmRegistry.CreateAlgorithms())
    {
    }

    public CryptoEngine(IEnumerable<ICryptoAlgorithm> algorithms)
    {
        _algorithms = algorithms.ToDictionary(a => a.Metadata.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<AlgorithmMetadata> Algorithms =>
        _algorithms.Values.Select(a => a.Metadata).OrderBy(a => a.Category).ThenBy(a => a.Name).ToList();

    public AlgorithmMetadata? FindMetadata(string algorithmId)
    {
        return _algorithms.TryGetValue(algorithmId, out var algorithm) ? algorithm.Metadata : null;
    }

    public CryptoResult Execute(CryptoRequest request)
    {
        if (!_algorithms.TryGetValue(request.AlgorithmId, out var algorithm))
        {
            return CryptoResult.Failure(null, request.Operation, request.InputText, "Select a supported algorithm.");
        }

        if (string.IsNullOrWhiteSpace(request.InputText))
        {
            return CryptoResult.Failure(algorithm.Metadata, request.Operation, request.InputText, "Input text is required.");
        }

        if (request.Operation == CryptoOperation.Hash && !algorithm.Metadata.SupportsHashing)
        {
            return CryptoResult.Failure(algorithm.Metadata, request.Operation, request.InputText, "This algorithm is not a hash function.");
        }

        if (request.Operation == CryptoOperation.Encrypt && !algorithm.Metadata.SupportsEncryption)
        {
            return CryptoResult.Failure(algorithm.Metadata, request.Operation, request.InputText, "This algorithm does not support encryption.");
        }

        if (request.Operation == CryptoOperation.Decrypt && !algorithm.Metadata.SupportsDecryption)
        {
            return CryptoResult.Failure(algorithm.Metadata, request.Operation, request.InputText, "Hash functions are one-way and cannot be decrypted.");
        }

        var validation = algorithm.Validate(request);
        if (!validation.IsValid)
        {
            return CryptoResult.Failure(algorithm.Metadata, request.Operation, request.InputText, validation.Message);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var output = algorithm.Process(request);
            stopwatch.Stop();
            return CryptoResult.SuccessResult(algorithm.Metadata, request.Operation, request.InputText, output, stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (CryptographicException)
        {
            stopwatch.Stop();
            return CryptoResult.Failure(algorithm.Metadata, request.Operation, request.InputText, "The key, IV, nonce, or ciphertext is not valid for this operation.", stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (FormatException)
        {
            stopwatch.Stop();
            return CryptoResult.Failure(algorithm.Metadata, request.Operation, request.InputText, "The ciphertext package or key encoding is not valid Base64.", stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (ArgumentException ex)
        {
            stopwatch.Stop();
            return CryptoResult.Failure(algorithm.Metadata, request.Operation, request.InputText, ex.Message, stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (PlatformNotSupportedException)
        {
            stopwatch.Stop();
            return CryptoResult.Failure(algorithm.Metadata, request.Operation, request.InputText, "This algorithm is not supported by the current Windows cryptography provider.", stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
