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

    public void DisableSaving() => saveDisabled = true;

    public static DockSettings Load()
    {
        try
        {
            if (File.Exists(Path))
            {
                var loaded = JsonSerializer.Deserialize<DockSettings>(File.ReadAllText(Path));
                if (loaded is not null)
                {
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
