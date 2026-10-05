using System.Drawing.Drawing2D;

namespace FirawynixDock;

internal sealed class AppTile : Control
{
    private static readonly Font InitialFont = new("Segoe UI", 24, FontStyle.Bold);
    private static readonly Font NameFont = new("Segoe UI", 9.2f, FontStyle.Bold);
    private static readonly Font StatusFont = new("Segoe UI", 7.4f, FontStyle.Bold);
    private Image? image;
    private bool hovering;
    public CatalogItem Item { get; }

    public AppTile(CatalogItem item)
    {
        Item = item;
        Size = new Size(132, 158);
        Margin = new Padding(0, 0, 7, 9);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        _ = LoadImageAsync();
    }

    private async Task LoadImageAsync()
    {
        var loaded = await ImageStore.GetAsync(Item.ImageUrl);
        if (!IsDisposed) { image = loaded; Invalidate(); }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        hovering = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hovering = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var outer = new Rectangle(1, 1, Width - 3, Height - 3);
        using var shape = Rounded(outer, 17);
        using var fill = new SolidBrush(hovering ? Color.FromArgb(27, 100, 112) : Color.FromArgb(17, 67, 78));
        using var border = new Pen(hovering ? Color.FromArgb(82, 226, 239) : Color.FromArgb(54, 140, 156), hovering ? 2 : 1);
        g.FillPath(fill, shape);
        g.DrawPath(border, shape);

        var iconBounds = new Rectangle((Width - 65) / 2, 15, 65, 65);
        if (image is not null)
        {
            using var clip = Rounded(iconBounds, 13);
            var state = g.Save();
            g.SetClip(clip);
            g.DrawImage(image, Fit(image.Size, iconBounds));
            g.Restore(state);
        }
        else
        {
            using var iconFill = new SolidBrush(Color.FromArgb(32, 132, 149));
            using var iconOutline = new Pen(Color.FromArgb(105, 224, 238));
            g.FillEllipse(iconFill, iconBounds);
            g.DrawEllipse(iconOutline, iconBounds);
            TextRenderer.DrawText(g, Item.Name[..1].ToUpperInvariant(), InitialFont,
                iconBounds, Color.FromArgb(217, 251, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        var nameBounds = new Rectangle(9, 88, Width - 18, 44);
        TextRenderer.DrawText(g, Item.Name, NameFont, nameBounds,
            Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

        var status = Item.Type == "web" ? "SITE" : Item.Target is null ? "NÃO INSTALADO" : "INSTALADO";
        var statusColor = Item.Type == "web" ? Color.FromArgb(156, 231, 241) :
            Item.Target is null ? Color.FromArgb(244, 202, 130) : Color.FromArgb(128, 244, 213);
        TextRenderer.DrawText(g, status, StatusFont,
            new Rectangle(5, 136, Width - 10, 17), statusColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static Rectangle Fit(Size source, Rectangle bounds)
    {
        var scale = Math.Min((float)bounds.Width / source.Width, (float)bounds.Height / source.Height);
        var width = Math.Max(1, (int)(source.Width * scale));
        var height = Math.Max(1, (int)(source.Height * scale));
        return new Rectangle(bounds.X + (bounds.Width - width) / 2,
            bounds.Y + (bounds.Height - height) / 2, width, height);
    }

    internal static GraphicsPath Rounded(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
