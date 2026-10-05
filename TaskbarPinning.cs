using Windows.UI.Shell;

namespace FirawynixDock;

internal static class TaskbarPinning
{
    public static string? LastError { get; private set; }
    // null means this Windows build cannot report the pin state for this app.
    public static async Task<bool?> IsPinnedAsync()
    {
        try
        {
            var manager = TaskbarManager.GetDefault();
            if (manager.IsSupported && await manager.IsCurrentAppPinnedAsync()) return true;
            if (!manager.IsSupported) LastError = "TaskbarManager.IsSupported=false";
        }
        catch (Exception ex) { LastError = $"{ex.GetType().Name}: {ex.Message}"; }
        return HasMatchingPinnedShortcut();
    }

    private static bool HasMatchingPinnedShortcut()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar");
        if (!Directory.Exists(folder)) return false;
        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is null) return false;
            dynamic shell = Activator.CreateInstance(shellType)!;
            var currentExe = Path.GetFullPath(Environment.ProcessPath ??
                Path.Combine(AppContext.BaseDirectory, "FirawynixDock.exe"));
            foreach (var link in Directory.EnumerateFiles(folder, "*.lnk"))
            {
                dynamic shortcut = shell.CreateShortcut(link);
                string target = shortcut.TargetPath;
                if (Path.IsPathFullyQualified(target) &&
                    string.Equals(Path.GetFullPath(target), currentExe, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        catch (Exception ex) { LastError = $"Atalho fixado: {ex.GetType().Name}: {ex.Message}"; }
        return false;
    }

    public static async Task<bool?> RequestPinAsync()
    {
        try
        {
            var manager = TaskbarManager.GetDefault();
            if (!manager.IsSupported || !manager.IsPinningAllowed) return null;
            return await manager.RequestPinCurrentAppAsync();
        }
        catch (Exception) { return null; }
    }
}
