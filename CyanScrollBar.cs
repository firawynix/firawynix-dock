using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace FirawynixDock;

internal sealed class CyanScrollBar : Control
{
    private int contentHeight;
    private int viewportHeight;
    private int value;
    private bool dragging;
    private int dragOffset;

    public event EventHandler? ValueChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get => value;
        set
        {
            var next = Math.Clamp(value, 0, Maximum);
            if (this.value == next) return;
            this.value = next;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public int Maximum => Math.Max(0, contentHeight - viewportHeight);

    public CyanScrollBar()
    {
        Width = 10;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    public void SetRange(int content, int viewport)
    {
        contentHeight = Math.Max(content, 0);
        viewportHeight = Math.Max(viewport, 0);
        Visible = Maximum > 0;
        Value = Math.Min(Value, Maximum);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Maximum <= 0) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var trackBrush = new SolidBrush(Color.FromArgb(21, 91, 103));
        using var thumbBrush = new SolidBrush(Color.FromArgb(82, 218, 235));
        using var track = AppTile.Rounded(new Rectangle(3, 0, 4, Height), 2);
        using var thumb = AppTile.Rounded(ThumbBounds(), 5);
        e.Graphics.FillPath(trackBrush, track);
        e.Graphics.FillPath(thumbBrush, thumb);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        var thumb = ThumbBounds();
        if (thumb.Contains(e.Location))
        {
            dragging = true;
            dragOffset = e.Y - thumb.Top;
        }
        else
        {
            Value += e.Y < thumb.Top ? -viewportHeight : viewportHeight;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!dragging || Maximum <= 0) return;
        var thumb = ThumbBounds();
        var travel = Math.Max(1, Height - thumb.Height);
        Value = (int)Math.Round(Math.Clamp(e.Y - dragOffset, 0, travel) * (double)Maximum / travel);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        dragging = false;
        base.OnMouseUp(e);
    }

    private Rectangle ThumbBounds()
    {
        var height = Math.Clamp((int)(Height * (double)viewportHeight / Math.Max(contentHeight, 1)), 36, Height);
        var top = Maximum == 0 ? 0 : (int)Math.Round((Height - height) * (double)value / Maximum);
        return new Rectangle(0, top, Width, height);
    }
}
