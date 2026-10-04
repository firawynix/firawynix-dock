using System.Text.Json;
using System.Drawing;

namespace FirawynixDock;

internal static class Program
{
    private const string SignalName = @"Local\FirawynixDockToggle";
    private const string MutexName = @"Local\FirawynixDockSingleInstance";

    [STAThread]
    private static void Main(string[] args)
    {
        var snapshot = Catalog.LoadLocal();
        if (args.Contains("--check", StringComparer.OrdinalIgnoreCase))
        {
            var report = snapshot.Items.Select(item => new
            {
                item.Name,
                item.Slug,
                item.Category,
                item.Type,
                item.Action,
                item.Target,
                item.ImageUrl
            });
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "verification.json"),
                JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            return;
        }

        if (args.Contains("--check-placement", StringComparer.OrdinalIgnoreCase))
        {
            var panel = new Size(490, 560);
            var cases = new[]
            {
                (Name: "bottom with top bar", Anchor: new Point(370, 1060),
                    Screen: new Rectangle(0, 0, 1920, 1080), Work: new Rectangle(0, 20, 1920, 1020),
                    Bar: new Rectangle(0, 1040, 1920, 40), Expected: new Point(125, 472)),
                (Name: "top", Anchor: new Point(1200, 20),
                    Screen: new Rectangle(0, 0, 1920, 1080), Work: new Rectangle(0, 40, 1920, 1040),
                    Bar: new Rectangle(0, 0, 1920, 40), Expected: new Point(955, 48)),
                (Name: "left", Anchor: new Point(25, 500),
                    Screen: new Rectangle(0, 0, 1920, 1080), Work: new Rectangle(50, 0, 1870, 1080),
                    Bar: new Rectangle(0, 0, 50, 1080), Expected: new Point(58, 220)),
                (Name: "right", Anchor: new Point(1900, 700),
                    Screen: new Rectangle(0, 0, 1920, 1080), Work: new Rectangle(0, 0, 1870, 1080),
                    Bar: new Rectangle(1870, 0, 50, 1080), Expected: new Point(1372, 420)),
                (Name: "second monitor", Anchor: new Point(-1200, 880),
                    Screen: new Rectangle(-1600, 0, 1600, 900), Work: new Rectangle(-1600, 0, 1600, 860),
                    Bar: new Rectangle(-1600, 860, 1600, 40), Expected: new Point(-1445, 292))
            };
            var report = cases.Select(x =>
            {
                var actual = TaskbarPlacement.Calculate(x.Anchor, x.Screen, x.Work, panel, x.Bar);
                return new { x.Name, x.Expected, Actual = actual, Passed = actual == x.Expected };
            }).ToList();
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "placement-verification.json"),
                JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            if (report.Any(x => !x.Passed)) Environment.ExitCode = 1;
            return;
        }

        if (args.Contains("--check-images", StringComparer.OrdinalIgnoreCase))
        {
            var images = Task.WhenAll(snapshot.Items.Select(x => ImageStore.GetAsync(x.ImageUrl)))
                .GetAwaiter().GetResult();
            var report = snapshot.Items.Select((item, index) => new
            {
                item.Name,
                Loaded = images[index] is not null
            });
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "images-verification.json"),
                JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            return;
        }

        var previewBottom = args.Contains("--preview-bottom", StringComparer.OrdinalIgnoreCase);
        var interactivePreview = args.Contains("--preview-interactive", StringComparer.OrdinalIgnoreCase);
        if (previewBottom || interactivePreview || args.Contains("--preview", StringComparer.OrdinalIgnoreCase))
        {
            ApplicationConfiguration.Initialize();
            using var preview = new DockForm(snapshot, previewMode: true);
            using var timer = new System.Windows.Forms.Timer { Interval = 6000 };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                if (previewBottom) preview.ScrollToBottomForPreview();
                using var bitmap = new Bitmap(preview.Width, preview.Height);
                preview.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                bitmap.Save(Path.Combine(AppContext.BaseDirectory,
                    previewBottom ? "preview-bottom.png" : "preview.png"));
                preview.ExitForPreview();
            };
            if (!interactivePreview) preview.Shown += (_, _) => timer.Start();
            Application.Run(preview);
            return;
        }

        using var mutex = new Mutex(true, MutexName, out var firstInstance);
        using var signal = new EventWaitHandle(false, EventResetMode.AutoReset, SignalName);
        if (!firstInstance)
        {
            signal.Set();
            return;
        }

        ApplicationConfiguration.Initialize();
        using var form = new DockForm(snapshot);
        _ = Task.Run(() =>
        {
            while (!form.IsDisposed)
            {
                signal.WaitOne();
                if (form.IsHandleCreated && !form.IsDisposed)
                    form.BeginInvoke(form.Toggle);
            }
        });
        Application.Run(form);
    }
}
