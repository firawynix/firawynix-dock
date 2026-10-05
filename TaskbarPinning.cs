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
            if (!manager.IsSupported) { LastError = "TaskbarManager.IsSupported=false"; return null; }
            return await manager.IsCurrentAppPinnedAsync();
        }
        catch (Exception ex) { LastError = $"{ex.GetType().Name}: {ex.Message}"; return null; }
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
