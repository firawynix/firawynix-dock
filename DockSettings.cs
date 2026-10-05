using System.Text.Json;

namespace FirawynixDock;

internal sealed class DockSettings
{
    private bool saveDisabled;
    private static readonly string Path = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FirawynixDock", "settings.json");

    public int TransparencyPercent { get; set; } = 5;
    public bool HideOuterFrame { get; set; }
    public bool HideSearch { get; set; }
    public bool HideMenus { get; set; }
    public bool HideSites { get; set; } = true;
    public bool HideNotInstalled { get; set; } = true;

    public void DisableSaving() => saveDisabled = true;

    public static DockSettings Load()
    {
        try
        {
            if (File.Exists(Path))
            {
                var json = File.ReadAllText(Path);
                var loaded = JsonSerializer.Deserialize<DockSettings>(json);
                if (loaded is not null)
                {
                    using var document = JsonDocument.Parse(json);
                    var root = document.RootElement;
                    if (!root.TryGetProperty(nameof(HideSites), out _) &&
                        !root.TryGetProperty(nameof(HideNotInstalled), out _) &&
                        root.TryGetProperty("OnlyInstalled", out var oldFilter) &&
                        oldFilter.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    {
                        loaded.HideSites = oldFilter.GetBoolean();
                        loaded.HideNotInstalled = oldFilter.GetBoolean();
                    }
                    loaded.TransparencyPercent = Math.Clamp(loaded.TransparencyPercent, 0, 100);
                    return loaded;
                }
            }
        }
        catch (Exception) { /* Mantém o padrão se as preferências não puderem ser lidas. */ }
        return new DockSettings();
    }

    public void Save()
    {
        if (saveDisabled) return;
        try
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
            File.WriteAllText(Path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception) { /* A janela continua funcional mesmo sem salvar. */ }
    }
}
