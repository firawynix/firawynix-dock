namespace FirawynixDock;

internal sealed class RoundedViewport : Panel
{
    public RoundedViewport()
    {
        BackColor = Color.Transparent;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width <= 0 || Height <= 0) return;
        using var shape = AppTile.Rounded(new Rectangle(0, 0, Width, Height), 16);
        Region?.Dispose();
        Region = new Region(shape);
    }
}
