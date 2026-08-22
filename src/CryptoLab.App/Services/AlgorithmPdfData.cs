using System.Collections.Generic;

namespace CryptoLab.App.Services;

internal sealed record AlgorithmPdfData
{
    public required string AppTitle { get; init; }
    public required string AppSubtitle { get; init; }
    public required string AlgorithmName { get; init; }
    public required string MetaLine { get; init; }
    public required string OverviewTitle { get; init; }
    public required string Overview { get; init; }
    public required string HowItWorksTitle { get; init; }
    public required string HowItWorks { get; init; }
    public required string MathConceptTitle { get; init; }
    public required string MathConcept { get; init; }
    public required string AdvantagesTitle { get; init; }
    public required IReadOnlyList<string> Advantages { get; init; }
    public required string SecurityAnalysisTitle { get; init; }
    public required string SecurityNotice { get; init; }
    public required string PythonTitle { get; init; }
    public required string PythonIntro { get; init; }
    public required string PythonVerified { get; init; }
    public required string PythonLibrariesTitle { get; init; }
    public required string PythonLibraries { get; init; }
    public required string PythonFunctionsTitle { get; init; }
    public required IReadOnlyList<string> PythonFunctions { get; init; }
    public required string PythonCodeTitle { get; init; }
    public required string PythonCode { get; init; }
    public bool Arabic { get; init; }
    public byte[]? LogoBytes { get; init; }
}
