using System;
using System.Windows;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorFloatingPanelSnapService
{
    public const double SnapDistance = 18;
    public const double PanelGap = 10;

    public static Point ResolveSnappedPosition(
        Point desired,
        Size viewport,
        Size panelSize,
        Rect? otherPanelRect,
        double padding = 0,
        bool enableSnap = true)
    {
        var point = LayoutEditorFloatingPanelLayoutService.ClampPanelPosition(desired.X, desired.Y, viewport, panelSize, padding);
        if (!enableSnap)
        {
            return point;
        }

        var left = point.X;
        var top = point.Y;
        var right = left + panelSize.Width;
        var bottom = top + panelSize.Height;

        var viewportRight = Math.Max(padding, viewport.Width - padding);
        var viewportBottom = Math.Max(padding, viewport.Height - padding);

        if (Math.Abs(left - padding) <= SnapDistance) left = padding;
        if (Math.Abs(top - padding) <= SnapDistance) top = padding;
        if (Math.Abs(viewportRight - right) <= SnapDistance) left = viewportRight - panelSize.Width;
        if (Math.Abs(viewportBottom - bottom) <= SnapDistance) top = viewportBottom - panelSize.Height;

        if (otherPanelRect is Rect other)
        {
            var verticalOverlap = RangesOverlap(top, bottom, other.Top, other.Bottom);
            var horizontalOverlap = RangesOverlap(left, right, other.Left, other.Right);

            if (verticalOverlap)
            {
                if (Math.Abs(left - other.Left) <= SnapDistance) left = other.Left;
                if (Math.Abs(right - other.Right) <= SnapDistance) left = other.Right - panelSize.Width;
                if (Math.Abs(left - (other.Right + PanelGap)) <= SnapDistance) left = other.Right + PanelGap;
                if (Math.Abs(right - (other.Left - PanelGap)) <= SnapDistance) left = other.Left - PanelGap - panelSize.Width;
            }

            if (horizontalOverlap)
            {
                if (Math.Abs(top - other.Top) <= SnapDistance) top = other.Top;
                if (Math.Abs(bottom - other.Bottom) <= SnapDistance) top = other.Bottom - panelSize.Height;
                if (Math.Abs(top - (other.Bottom + PanelGap)) <= SnapDistance) top = other.Bottom + PanelGap;
                if (Math.Abs(bottom - (other.Top - PanelGap)) <= SnapDistance) top = other.Top - PanelGap - panelSize.Height;
            }
        }

        return LayoutEditorFloatingPanelLayoutService.ClampPanelPosition(left, top, viewport, panelSize, padding);
    }

    private static bool RangesOverlap(double a1, double a2, double b1, double b2)
    {
        return Math.Min(a2, b2) - Math.Max(a1, b1) >= 24;
    }
}
