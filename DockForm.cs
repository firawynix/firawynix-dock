using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace FirawynixDock;

internal sealed class DockForm : Form
{
    // Near-black avoids a bright fringe where rounded controls meet transparent pixels.
    private static readonly Color ClearKey = Color.FromArgb(1, 2, 3);
    private CatalogSnapshot snapshot;
    private readonly RoundedViewport viewport;
    private readonly FlowLayoutPanel grid;
    private readonly CyanScrollBar scrollBar;
    private readonly TextBox search;
    private readonly Label heading;
    private readonly Label searchLabel;
    private readonly Button installedButton;
    private readonly Button settingsButton;
    private readonly Button refreshButton;
    private readonly Button closeButton;
    private readonly DockSettings settings;
    private readonly DockBackdrop backdrop = new();
    private readonly NotifyIcon tray;
    private readonly ContextMenuStrip settingsMenu = new();
    private readonly ToolTip tips = new();
    private readonly System.Windows.Forms.Timer idleTimer = new() { Interval = 250 };
    private readonly SemaphoreSlim refreshGate = new(1);
    private bool exiting;
    private bool menuOpen;
    private readonly bool previewMode;
    private DateTime suppressHideUntil;
    private DateTime lastActivityUtc;
    private Point lastCursor;
    private Point taskbarAnchor;

    public DockForm(CatalogSnapshot initial, bool previewMode = false)
    {
        snapshot = initial;
        this.previewMode = previewMode;
        settings = DockSettings.Load();
        if (previewMode) settings.DisableSaving();
        Text = "Firawynix Dock";
        Size = new Size(490, 560);
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = previewMode;
        StartPosition = FormStartPosition.Manual;
        BackColor = ClearKey;
        TransparencyKey = ClearKey;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);
        TopMost = true;
        DoubleBuffered = true;
        Icon = Icon.ExtractAssociatedIcon(Path.Combine(AppContext.BaseDirectory, "FirawynixDock.exe")) ?? SystemIcons.Application;

        heading = new Label
        {
            Text = "FIRAWYNIX  /  DOCK",
            Location = new Point(22, 22), Size = new Size(315, 34),
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(218, 252, 255),
            Font = new Font("Segoe UI", 15, FontStyle.Bold)
        };
        Controls.Add(heading);

        settingsButton = HeaderButton("⚙", 351);
        settingsButton.Click += (_, _) => ShowSettingsMenu();
        tips.SetToolTip(settingsButton, "Configurar aparência");
        Controls.Add(settingsButton);

        refreshButton = HeaderButton("↻", 395);
        refreshButton.Click += async (_, _) => await RefreshAsync();
        tips.SetToolTip(refreshButton, "Atualizar catálogo");
        Controls.Add(refreshButton);

        closeButton = HeaderButton("×", 439);
        closeButton.Click += (_, _) => Hide();
        tips.SetToolTip(closeButton, "Fechar painel");
        Controls.Add(closeButton);

        searchLabel = new Label
        {
            Text = "BUSCAR",
            Location = new Point(22, 61), Size = new Size(80, 15),
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(144, 220, 231),
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold)
        };
        Controls.Add(searchLabel);

        installedButton = new Button
        {
            Location = new Point(320, 57), Size = new Size(148, 20),
            FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(13, 77, 91),
            ForeColor = Color.FromArgb(144, 244, 221),
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        installedButton.FlatAppearance.BorderSize = 0;
        installedButton.Click += (_, _) => ToggleInstalledOnly();
        tips.SetToolTip(installedButton, "Alternar entre todos os itens e apenas programas instalados");
        Controls.Add(installedButton);

        search = new TextBox
        {
            PlaceholderText = "Buscar programas, jogos e sites",
            Location = new Point(22, 78), Size = new Size(446, 34),
            BackColor = Color.FromArgb(14, 74, 88), ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10)
        };
        search.MouseWheel += (_, e) => ScrollBy(-Math.Sign(e.Delta) * 70);
        search.TextChanged += (_, _) => { MarkActivity(); RenderGrid(); };
        search.KeyDown += (_, _) => MarkActivity();
        Controls.Add(search);

        viewport = new RoundedViewport
        {
            Location = new Point(22, 131), Size = new Size(426, 402),
            BackColor = Color.Transparent
        };
        Controls.Add(viewport);

        grid = new FlowLayoutPanel
        {
            Location = new Point(0, 0), Size = new Size(426, 402),
            AutoScroll = false, WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent,
            Padding = new Padding(2, 2, 0, 0)
        };
        grid.MouseWheel += (_, e) => ScrollBy(-Math.Sign(e.Delta) * 70);
        viewport.MouseWheel += (_, e) => ScrollBy(-Math.Sign(e.Delta) * 70);
        MouseWheel += (_, e) => ScrollBy(-Math.Sign(e.Delta) * 70);
        viewport.Controls.Add(grid);

        scrollBar = new CyanScrollBar
        {
            Location = new Point(457, 131), Size = new Size(10, 402)
        };
        scrollBar.ValueChanged += (_, _) => grid.Top = -scrollBar.Value;
        scrollBar.MouseMove += (_, _) => MarkActivity();
        Controls.Add(scrollBar);

        AddAppearanceItems(settingsMenu.Items);
        ConfigureMenuLifetime(settingsMenu);
        ContextMenuStrip = settingsMenu;
        foreach (var control in new Control[]
                 { heading, settingsButton, refreshButton, closeButton, searchLabel, installedButton, search,
                   viewport, grid, scrollBar })
            control.ContextMenuStrip = settingsMenu;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Abrir dock", null, (_, _) => ShowDock());
        menu.Items.Add("Atualizar", null, async (_, _) => await RefreshAsync());
        var appearance = new ToolStripMenuItem("Aparência");
        AddAppearanceItems(appearance.DropDownItems);
        menu.Items.Add(appearance);
        menu.Items.Add("Sair", null, (_, _) => { exiting = true; Close(); });
        ConfigureMenuLifetime(menu);
        tray = new NotifyIcon
        {
            Icon = Icon, Text = "Firawynix Dock", Visible = true, ContextMenuStrip = menu
        };
        tray.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) Toggle(); };

        Deactivate += (_, _) =>
        {
            if (!previewMode && !menuOpen && DateTime.UtcNow >= suppressHideUntil) Hide();
        };
        FormClosing += (_, e) =>
        {
            if (!exiting) { e.Cancel = true; Hide(); }
        };
        FormClosed += (_, _) =>
        {
            backdrop.Dispose();
            idleTimer.Stop();
            idleTimer.Dispose();
            tray.Dispose();
            menu.Dispose();
            settingsMenu.Dispose();
            tips.Dispose();
        };
        Shown += async (_, _) =>
        {
            suppressHideUntil = DateTime.UtcNow.AddSeconds(2);
            taskbarAnchor = Cursor.Position;
            PlaceAboveTaskbar();
            Activate();
            if (search.Visible) search.Focus();
            MarkActivity();
            idleTimer.Start();
            await RefreshAsync();
        };
        idleTimer.Tick += (_, _) =>
        {
            if (!Visible || previewMode || menuOpen) return;
            var cursor = Cursor.Position;
            if (cursor != lastCursor)
            {
                if (Bounds.Contains(cursor)) MarkActivity();
                lastCursor = cursor;
            }
            if (DateTime.UtcNow - lastActivityUtc >= TimeSpan.FromSeconds(8)) Hide();
        };
        LocationChanged += (_, _) => SyncBackdrop();
        SizeChanged += (_, _) => SyncBackdrop();
        VisibleChanged += (_, _) => SyncBackdrop();
        ApplyAppearance(reposition: false);
        RenderGrid();
    }

    private static readonly int[] TransparencyChoices =
        [0, 5, 10, 15, 20, 25, 30, 40, 50, 60, 70, 80, 90, 100];

    private void AddAppearanceItems(ToolStripItemCollection items)
    {
        var installed = new ToolStripMenuItem("Mostrar só instalados") { Tag = "installed" };
        installed.Click += (_, _) => ToggleInstalledOnly();
        items.Add(installed);
        items.Add(new ToolStripSeparator());
        var transparency = new ToolStripMenuItem("Transparência do fundo");
        foreach (var percent in TransparencyChoices)
        {
            var choice = percent;
            var option = new ToolStripMenuItem($"{choice}%") { Tag = choice };
            option.Click += (_, _) => SetTransparency(choice);
            transparency.DropDownItems.Add(option);
        }
        items.Add(transparency);
        items.Add(new ToolStripSeparator());
        foreach (var (label, key) in new[]
        {
            ("Ocultar moldura externa", "frame"),
            ("Ocultar busca", "search"),
            ("Ocultar cabeçalho e botões", "menus")
        })
        {
            var option = new ToolStripMenuItem(label) { Tag = key };
            option.Click += (_, _) => ToggleAppearance(key);
            items.Add(option);
        }
    }

    private void ConfigureMenuLifetime(ContextMenuStrip menu)
    {
        menu.Opening += (_, _) =>
        {
            menuOpen = true;
            SyncMenuChecks(menu.Items);
            MarkActivity();
        };
        menu.Closed += (_, _) =>
        {
            menuOpen = false;
            suppressHideUntil = DateTime.UtcNow.AddMilliseconds(500);
            MarkActivity();
        };
    }

    private void SyncMenuChecks(ToolStripItemCollection items)
    {
        foreach (ToolStripItem entry in items)
        {
            if (entry is not ToolStripMenuItem item) continue;
            item.Checked = item.Tag switch
            {
                int percent => percent == settings.TransparencyPercent,
                "frame" => settings.HideOuterFrame,
                "search" => settings.HideSearch,
                "menus" => settings.HideMenus,
                "installed" => settings.OnlyInstalled,
                _ => false
            };
            SyncMenuChecks(item.DropDownItems);
        }
    }

    private void ToggleAppearance(string key)
    {
        switch (key)
        {
            case "frame": settings.HideOuterFrame = !settings.HideOuterFrame; break;
            case "search": settings.HideSearch = !settings.HideSearch; break;
            case "menus": settings.HideMenus = !settings.HideMenus; break;
        }
        settings.Save();
        ApplyAppearance(reposition: true);
        MarkActivity();
    }

    private void ToggleInstalledOnly()
    {
        settings.OnlyInstalled = !settings.OnlyInstalled;
        settings.Save();
        RenderGrid();
        MarkActivity();
    }

    private int ContentTop => 18 + (settings.HideMenus ? 0 : 48) +
        (settings.HideSearch ? 0 : 65);

    private int PreferredHeight => 560 - (settings.HideMenus ? 48 : 0) -
        (settings.HideSearch ? 65 : 0);

    private void ApplyAppearance(bool reposition)
    {
        heading.Visible = settingsButton.Visible = refreshButton.Visible = closeButton.Visible =
            !settings.HideMenus;
        searchLabel.Visible = installedButton.Visible = search.Visible = !settings.HideSearch;
        searchLabel.Top = settings.HideMenus ? 16 : 61;
        installedButton.Top = settings.HideMenus ? 12 : 57;
        search.Top = settings.HideMenus ? 33 : 78;
        if (settings.HideSearch && search.TextLength > 0) search.Clear();
        viewport.Top = scrollBar.Top = ContentTop;
        if (reposition) PlaceAboveTaskbar();
        else
        {
            Height = PreferredHeight;
            viewport.Height = scrollBar.Height = Height - ContentTop - 27;
            RenderGrid();
        }
        Invalidate();
    }

    private void SyncBackdrop()
    {
        if (!IsHandleCreated || IsDisposed || backdrop.IsDisposed) return;
        if (!Visible || settings.TransparencyPercent == 100)
        {
            backdrop.Hide();
            return;
        }
        backdrop.Bounds = Bounds;
        backdrop.Opacity = 1 - settings.TransparencyPercent / 100d;
        if (!backdrop.Visible) backdrop.Show();
        BringToFront();
    }

    public void Toggle()
    {
        if (Visible) Hide(); else ShowDock();
    }

    public void ExitForPreview()
    {
        exiting = true;
        Close();
    }

    public void ScrollToBottomForPreview() => scrollBar.Value = scrollBar.Maximum;

    private void ScrollBy(int amount)
    {
        scrollBar.Value += amount;
        MarkActivity();
    }

    private void MarkActivity() => lastActivityUtc = DateTime.UtcNow;

    private void SetTransparency(int percent)
    {
        settings.TransparencyPercent = percent;
        SyncBackdrop();
        settings.Save();
        MarkActivity();
    }

    private void ShowSettingsMenu()
    {
        MarkActivity();
        settingsMenu.Show(settingsButton, new Point(0, settingsButton.Height));
    }

    private Button HeaderButton(string text, int x)
    {
        var button = new Button
        {
            Text = text, Location = new Point(x, 19), Size = new Size(34, 34),
            FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(13, 77, 91),
            ForeColor = Color.FromArgb(220, 252, 255),
            Font = new Font("Segoe UI", 15)
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(64, 176, 192);
        return button;
    }

    private void ShowDock()
    {
        snapshot = Catalog.LoadLocal();
        RenderGrid();
        MarkActivity();
        suppressHideUntil = DateTime.UtcNow.AddMilliseconds(800);
        taskbarAnchor = Cursor.Position;
        PlaceAboveTaskbar();
        Show();
        Activate();
        if (search.Visible) search.Focus();
        _ = RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (!await refreshGate.WaitAsync(0)) return;
        try
        {
            var local = Catalog.LoadLocal();
            if (local.Items.Count > 0)
            {
                snapshot = local;
                RenderGrid();
            }
            var remote = await Catalog.RefreshRemoteAsync();
            if (remote is not null && !IsDisposed)
            {
                snapshot = remote;
                RenderGrid();
            }
        }
        finally { refreshGate.Release(); }
    }

    private void PlaceAboveTaskbar()
    {
        var point = taskbarAnchor;
        var screen = Screen.FromPoint(point);
        var work = screen.WorkingArea;
        var desiredHeight = Math.Min(PreferredHeight, Math.Max(280, work.Height - 16));
        if (Height != desiredHeight)
            Height = desiredHeight;
        viewport.Height = scrollBar.Height = Math.Max(100, desiredHeight - ContentTop - 27);
        RenderGrid();
        var taskbar = TaskbarLocator.FindFor(screen, point);
        Location = TaskbarPlacement.Calculate(point, screen.Bounds, work, Size, taskbar);
    }

    private void RenderGrid()
    {
        if (grid is null || scrollBar is null || search is null) return;
        grid.SuspendLayout();
        foreach (var child in grid.Controls.Cast<Control>().ToArray()) child.Dispose();
        grid.Controls.Clear();
        var filtered = snapshot.Items
            .Where(x => x.Name.Contains(search.Text, StringComparison.CurrentCultureIgnoreCase))
            .Where(x => !settings.OnlyInstalled || (x.Type != "web" && x.Target is not null))
            .OrderBy(x => x.Category == "Jogos" ? 0 : 1)
            .ThenBy(x => x.Name)
            .ToList();
        foreach (var item in filtered)
        {
            var tile = new AppTile(item);
            tile.ContextMenuStrip = settingsMenu;
            tile.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) Open(item); };
            tile.MouseDown += (_, e) => { if (e.Button == MouseButtons.Right) MarkActivity(); };
            tile.MouseWheel += (_, e) => ScrollBy(-Math.Sign(e.Delta) * 70);
            tile.MouseMove += (_, _) => MarkActivity();
            tips.SetToolTip(tile, $"{item.Name} · {item.Action}");
            grid.Controls.Add(tile);
        }
        grid.ResumeLayout();
        var rows = (filtered.Count + 2) / 3;
        var contentHeight = Math.Max(viewport.Height, rows * 167 + 4);
        grid.Height = contentHeight;
        scrollBar.SetRange(contentHeight, viewport.Height);
        installedButton.Text = settings.OnlyInstalled ? "✓ SÓ INSTALADOS" : "MOSTRAR TUDO";
        installedButton.ForeColor = settings.OnlyInstalled
            ? Color.FromArgb(144, 244, 221) : Color.FromArgb(156, 231, 241);
    }

    private void Open(CatalogItem item)
    {
        var target = item.Target ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Firawynix Center", "FirawynixCenter.exe");
        if (!Uri.TryCreate(target, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            if (!File.Exists(target))
            {
                MessageBox.Show("Não encontrei o programa nem o Firawynix Center neste computador.",
                    "Firawynix Dock", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }
        try
        {
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            Hide();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível abrir {item.Name}: {ex.Message}",
                "Firawynix Dock", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        using var shape = AppTile.Rounded(new Rectangle(0, 0, Width, Height), 22);
        Region?.Dispose();
        Region = new Region(shape);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(TransparencyKey);
        if (settings.HideOuterFrame) return;
        using var outline = new Pen(Color.FromArgb(104, 220, 234), 2);
        using var shape = AppTile.Rounded(new Rectangle(1, 1, Width - 3, Height - 3), 22);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.DrawPath(outline, shape);
    }
}
