using System.Windows;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorFloatingPanelLayoutService
{
    public static Point ResolveToolbarStartPosition(Size viewport, Size panelSize, double padding)
    {
        return ClampPanelPosition((viewport.Width - panelSize.Width) / 2, padding, viewport, panelSize, padding);
    }

    public static Point ResolvePropertiesPanelStartPosition(Size viewport, Size panelSize, double padding, double top)
    {
        return ClampPanelPosition(viewport.Width - panelSize.Width - padding, top, viewport, panelSize, padding);
    }

    public static Point ClampPanelPosition(double left, double top, Size viewport, Size panelSize, double padding = 0)
    {
        var maxLeft = Math.Max(0, viewport.Width - panelSize.Width - padding);
        var maxTop = Math.Max(0, viewport.Height - panelSize.Height - padding);

        return new Point(
            LayoutEditorMath.Clamp(left, padding, maxLeft),
            LayoutEditorMath.Clamp(top, padding, maxTop));
    }

}
