using System.Windows;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorFloatingPanelLayoutService
{
    public static Point ClampPanelPosition(double left, double top, Size viewport, Size panelSize, double padding = 0)
    {
        var maxLeft = Math.Max(0, viewport.Width - panelSize.Width - padding);
        var maxTop = Math.Max(0, viewport.Height - panelSize.Height - padding);

        return new Point(
            LayoutEditorMath.Clamp(left, padding, maxLeft),
            LayoutEditorMath.Clamp(top, padding, maxTop));
    }

    public static Point ResolvePropertiesPanelPosition(
        LayoutEditorWidget? anchor,
        Size viewport,
        Size panelSize,
        double gap,
        double padding,
        double defaultTop)
    {
        if (anchor is null)
        {
            return ClampPanelPosition(
                Math.Max(0, viewport.Width - panelSize.Width - padding),
                defaultTop,
                viewport,
                panelSize,
                0);
        }

        var preferredLeft = anchor.X + anchor.Width + gap;
        var preferredTop = anchor.Y;

        var maxLeft = Math.Max(0, viewport.Width - panelSize.Width - padding);
        var maxTop = Math.Max(0, viewport.Height - panelSize.Height - padding);

        var left = preferredLeft;
        if (left > maxLeft)
        {
            left = anchor.X - panelSize.Width - gap;
        }

        if (left < padding)
        {
            left = maxLeft;
        }

        var top = LayoutEditorMath.Clamp(preferredTop, padding, maxTop);
        return new Point(LayoutEditorMath.Clamp(left, padding, maxLeft), top);
    }
}
