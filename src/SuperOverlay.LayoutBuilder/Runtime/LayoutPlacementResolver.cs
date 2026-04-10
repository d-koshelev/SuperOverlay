using SuperOverlay.LayoutBuilder.Layout;

namespace SuperOverlay.LayoutBuilder.Runtime;

public static class LayoutPlacementResolver
{
    public static LayoutItemPlacement ResolveForShell(LayoutItemPlacement placement, LayoutCanvas canvas, OverlayShellMode shellMode)
    {
        ArgumentNullException.ThrowIfNull(placement);
        ArgumentNullException.ThrowIfNull(canvas);

        if (shellMode == OverlayShellMode.Editor)
        {
            return placement;
        }

        return placement with
        {
            X = placement.X + canvas.RuntimeOffsetX + placement.RuntimeDeltaX,
            Y = placement.Y + canvas.RuntimeOffsetY + placement.RuntimeDeltaY
        };
    }
}
