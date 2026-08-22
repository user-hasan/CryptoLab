namespace CryptoLab.Core.Models;

public sealed record Lesson(
    string Id,
    string Title,
    string Summary,
    IReadOnlyList<string> Sections,
    IReadOnlyList<string> Exercises);
