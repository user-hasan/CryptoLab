namespace CryptoLab.Core.Cryptography;

public static class AlgorithmRegistry
{
    public static IReadOnlyList<ICryptoAlgorithm> CreateAlgorithms()
    {
        return new ICryptoAlgorithm[]
        {
            new CaesarCipherAlgorithm(),
            new AtbashCipherAlgorithm(),
            new Rot13CipherAlgorithm(),
            new VigenereCipherAlgorithm(),
            new AffineCipherAlgorithm(),
            new RailFenceCipherAlgorithm(),
            new ColumnarTranspositionAlgorithm(),
            new AesAlgorithm(),
            new DesAlgorithm(),
            new TripleDesAlgorithm(),
            new RsaAlgorithm(),
            new Md5HashAlgorithm(),
            new Sha1HashAlgorithm(),
            new Sha256HashAlgorithm(),
            new Sha512HashAlgorithm()
        };
    }
}
