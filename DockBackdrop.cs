using System.Drawing.Drawing2D;

namespace FirawynixDock;

// A separate window lets the panel background fade without fading its controls.
internal sealed class DockBackdrop : Form
{
    public DockBackdrop()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        DoubleBuffered = true;
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var parameters = base.CreateParams;
            parameters.ExStyle |= 0x00000020 | 0x08000000; // WS_EX_TRANSPARENT | WS_EX_NOACTIVATE
            return parameters;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width <= 0 || Height <= 0) return;
        using var shape = AppTile.Rounded(new Rectangle(0, 0, Width, Height), 22);
        Region?.Dispose();
        Region = new Region(shape);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var gradient = new LinearGradientBrush(ClientRectangle,
            Color.FromArgb(7, 42, 55), Color.FromArgb(12, 99, 111), 35f);
        e.Graphics.FillRectangle(gradient, ClientRectangle);
    }
}
