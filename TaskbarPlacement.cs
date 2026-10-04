using System.Runtime.InteropServices;
using System.Text;

namespace FirawynixDock;

internal enum TaskbarEdge { Top, Bottom, Left, Right }

internal static class TaskbarPlacement
{
    public static Point Calculate(Point anchor, Rectangle screen, Rectangle work,
        Size panel, Rectangle? taskbar = null)
    {
        var edge = taskbar is { } bar ? EdgeFor(bar, screen) : NearestEdge(anchor, screen);
        var minX = work.Left + 8;
        var maxX = Math.Max(minX, work.Right - panel.Width - 8);
        var minY = work.Top + 8;
        var maxY = Math.Max(minY, work.Bottom - panel.Height - 8);
        var x = Math.Clamp(anchor.X - panel.Width / 2, minX, maxX);
        var y = Math.Clamp(anchor.Y - panel.Height / 2, minY, maxY);
        return edge switch
        {
            TaskbarEdge.Top => new Point(x, Math.Clamp(Math.Max(work.Top, taskbar?.Bottom ?? screen.Top) + 8, minY, maxY)),
            TaskbarEdge.Bottom => new Point(x, Math.Clamp(Math.Min(work.Bottom, taskbar?.Top ?? screen.Bottom) - panel.Height - 8, minY, maxY)),
            TaskbarEdge.Left => new Point(Math.Clamp(Math.Max(work.Left, taskbar?.Right ?? screen.Left) + 8, minX, maxX), y),
            _ => new Point(Math.Clamp(Math.Min(work.Right, taskbar?.Left ?? screen.Right) - panel.Width - 8, minX, maxX), y)
        };
    }

    private static TaskbarEdge EdgeFor(Rectangle taskbar, Rectangle screen)
    {
        if (taskbar.Width >= taskbar.Height)
            return Math.Abs(taskbar.Top - screen.Top) < Math.Abs(screen.Bottom - taskbar.Bottom)
                ? TaskbarEdge.Top : TaskbarEdge.Bottom;
        return Math.Abs(taskbar.Left - screen.Left) < Math.Abs(screen.Right - taskbar.Right)
            ? TaskbarEdge.Left : TaskbarEdge.Right;
    }

    private static TaskbarEdge NearestEdge(Point anchor, Rectangle screen)
    {
        var distances = new[]
        {
            (Edge: TaskbarEdge.Top, Distance: Math.Abs(anchor.Y - screen.Top)),
            (Edge: TaskbarEdge.Bottom, Distance: Math.Abs(screen.Bottom - anchor.Y)),
            (Edge: TaskbarEdge.Left, Distance: Math.Abs(anchor.X - screen.Left)),
            (Edge: TaskbarEdge.Right, Distance: Math.Abs(screen.Right - anchor.X))
        };
        return distances.MinBy(x => x.Distance).Edge;
    }
}

internal static class TaskbarLocator
{
    public static Rectangle? FindFor(Screen screen, Point anchor)
    {
        var found = new List<Rectangle>();
        EnumWindows((handle, _) =>
        {
            if (!IsWindowVisible(handle)) return true;
            var name = new StringBuilder(64);
            GetClassName(handle, name, name.Capacity);
            if (name.ToString() is not ("Shell_TrayWnd" or "Shell_SecondaryTrayWnd")) return true;
            if (GetWindowRect(handle, out var rect))
            {
                var bounds = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
                if (bounds.Width >= 24 && bounds.Height >= 24 && bounds.IntersectsWith(screen.Bounds))
                    found.Add(bounds);
            }
            return true;
        }, IntPtr.Zero);
        if (found.Count == 0) return null;
        var clicked = found.FirstOrDefault(x => x.Contains(anchor));
        return !clicked.IsEmpty ? clicked : found.MinBy(x => DistanceSquared(anchor, x));
    }

    private static long DistanceSquared(Point p, Rectangle r)
    {
        var x = Math.Clamp(p.X, r.Left, r.Right);
        var y = Math.Clamp(p.Y, r.Top, r.Bottom);
        return (long)(p.X - x) * (p.X - x) + (long)(p.Y - y) * (p.Y - y);
    }

    private delegate bool EnumWindowsCallback(IntPtr handle, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr parameter);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr handle, StringBuilder className, int maxCount);
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr handle, out NativeRect rect);
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr handle);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left, Top, Right, Bottom;
    }
}
