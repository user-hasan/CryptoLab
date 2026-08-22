namespace CryptoLab.Core.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Dark";
    public string Language { get; set; } = "en";
    public bool AutoClearInput { get; set; }
    public bool SaveHistory { get; set; } = true;
    public bool AnimationEffects { get; set; } = true;
}
