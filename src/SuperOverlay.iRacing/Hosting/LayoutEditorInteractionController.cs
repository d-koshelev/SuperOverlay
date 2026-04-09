using System.Windows;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutEditorInteractionController
{
    private readonly OverlayRuntimeSession _session;

    private EditorInteractionMode _mode;
    private Point _lastCanvasPoint;

    public LayoutEditorInteractionController(OverlayRuntimeSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        _session = session;
    }

    public bool IsInteracting => _mode != EditorInteractionMode.None;

    public LayoutInteractionStartResult BeginInteraction(object? hitSource, Point canvasPoint)
    {
        var hitItemId = _session.HitTestItemId(hitSource);
        if (hitItemId is null)
        {
            _session.SelectItem(null);
            _mode = EditorInteractionMode.None;
            return LayoutInteractionStartResult.Deselected;
        }

        _session.SelectItem(hitItemId.Value);
        _lastCanvasPoint = canvasPoint;
        _mode = _session.IsResizeHandleHit(hitSource, hitItemId.Value)
            ? EditorInteractionMode.Resize
            : EditorInteractionMode.Drag;

        return new LayoutInteractionStartResult(true, true, false);
    }

    public LayoutInteractionMoveResult MoveInteraction(
        Point canvasPoint,
        double canvasWidth,
        double canvasHeight,
        bool bypassSnap)
    {
        if (_mode == EditorInteractionMode.None)
        {
            return LayoutInteractionMoveResult.None;
        }

        var dx = canvasPoint.X - _lastCanvasPoint.X;
        var dy = canvasPoint.Y - _lastCanvasPoint.Y;
        if (Math.Abs(dx) < double.Epsilon && Math.Abs(dy) < double.Epsilon)
        {
            return LayoutInteractionMoveResult.None;
        }

        if (_mode == EditorInteractionMode.Resize)
        {
            var resizeResult = _session.ResizeSelectedWithSnap(
                dx,
                dy,
                canvasWidth,
                canvasHeight,
                bypassSnap);

            if (resizeResult.Moved)
            {
                _lastCanvasPoint = canvasPoint;
            }

            return new LayoutInteractionMoveResult(resizeResult.Moved, resizeResult.SnapX, resizeResult.SnapY);
        }

        var moveResult = _session.MoveSelectedWithSnap(
            dx,
            dy,
            canvasWidth,
            canvasHeight,
            bypassSnap);

        if (moveResult.Moved)
        {
            _lastCanvasPoint = canvasPoint;
        }

        return new LayoutInteractionMoveResult(moveResult.Moved, moveResult.SnapX, moveResult.SnapY);
    }

    public bool EndInteraction()
    {
        if (_mode == EditorInteractionMode.None)
        {
            return false;
        }

        _mode = EditorInteractionMode.None;
        _session.EndDrag();
        return true;
    }

    private enum EditorInteractionMode
    {
        None,
        Drag,
        Resize
    }
}

public readonly record struct LayoutInteractionStartResult(
    bool HasHitItem,
    bool CaptureMouse,
    bool ClearedSelection)
{
    public static LayoutInteractionStartResult Deselected => new(false, false, true);
}

public readonly record struct LayoutInteractionMoveResult(
    bool Changed,
    double? SnapX,
    double? SnapY)
{
    public static LayoutInteractionMoveResult None => new(false, null, null);
}
