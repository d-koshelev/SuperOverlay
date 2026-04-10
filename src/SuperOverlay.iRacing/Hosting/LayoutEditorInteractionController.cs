using System.Windows;
using System.Windows.Input;
using WpfPoint = System.Windows.Point;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutEditorInteractionController
{
    private readonly OverlayRuntimeSession _session;
    private readonly LayoutInteractionOptions _options;

    private EditorInteractionMode _mode;
    private WpfPoint _startCanvasPoint;
    private WpfPoint _lastCanvasPoint;

    public LayoutEditorInteractionController(OverlayRuntimeSession session, LayoutInteractionOptions? options = null)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _options = options ?? LayoutInteractionOptions.EditorDefault;
    }

    public bool IsInteracting => _mode != EditorInteractionMode.None;
    public bool IsMarqueeSelecting => _mode == EditorInteractionMode.Marquee;

    public LayoutInteractionStartResult BeginInteraction(object? hitSource, WpfPoint canvasPoint, ModifierKeys modifiers)
    {
        var hitItemId = _session.HitTestItemId(hitSource);
        var isCtrl = _options.AllowMultiSelect && modifiers.HasFlag(ModifierKeys.Control);
        _startCanvasPoint = canvasPoint;
        _lastCanvasPoint = canvasPoint;

        if (hitItemId is null)
        {
            if (_options.AllowMarqueeSelection)
            {
                _mode = EditorInteractionMode.Marquee;
                if (!isCtrl && _options.ClearSelectionOnEmptyClick)
                {
                    _session.SelectItem(null);
                    return new LayoutInteractionStartResult(false, true, true, true, GetMarqueeRect(canvasPoint));
                }

                return new LayoutInteractionStartResult(false, true, false, true, GetMarqueeRect(canvasPoint));
            }

            if (_options.ClearSelectionOnEmptyClick)
            {
                _session.SelectItem(null);
                return new LayoutInteractionStartResult(false, false, true, false, null);
            }

            return LayoutInteractionStartResult.None;
        }

        if (isCtrl)
        {
            _session.ToggleItemSelection(hitItemId.Value);
            _mode = EditorInteractionMode.None;
            return LayoutInteractionStartResult.None;
        }

        _session.SetPrimarySelection(hitItemId.Value);

        if (_session.IsLocked(hitItemId.Value))
        {
            _mode = EditorInteractionMode.None;
            return LayoutInteractionStartResult.None;
        }

        _mode = _options.AllowResize && _session.IsResizeHandleHit(hitSource, hitItemId.Value)
            ? EditorInteractionMode.Resize
            : EditorInteractionMode.Drag;

        return new LayoutInteractionStartResult(true, true, false, false, null);
    }

    public LayoutInteractionMoveResult MoveInteraction(WpfPoint canvasPoint, double canvasWidth, double canvasHeight, bool bypassSnap)
    {
        if (_mode == EditorInteractionMode.None)
        {
            return LayoutInteractionMoveResult.None;
        }

        if (_mode == EditorInteractionMode.Marquee)
        {
            _lastCanvasPoint = canvasPoint;
            return new LayoutInteractionMoveResult(false, null, null, GetMarqueeRect(canvasPoint));
        }

        var dx = canvasPoint.X - _lastCanvasPoint.X;
        var dy = canvasPoint.Y - _lastCanvasPoint.Y;
        if (Math.Abs(dx) < double.Epsilon && Math.Abs(dy) < double.Epsilon)
        {
            return LayoutInteractionMoveResult.None;
        }

        if (_mode == EditorInteractionMode.Resize)
        {
            var resizeResult = _session.ResizeSelectedWithSnap(dx, dy, canvasWidth, canvasHeight, bypassSnap);
            if (resizeResult.Moved)
            {
                _lastCanvasPoint = canvasPoint;
            }

            return new LayoutInteractionMoveResult(resizeResult.Moved, resizeResult.SnapX, resizeResult.SnapY, null);
        }

        var moveResult = _session.MoveSelectedWithSnap(dx, dy, canvasWidth, canvasHeight, bypassSnap);
        if (moveResult.Moved)
        {
            _lastCanvasPoint = canvasPoint;
        }

        return new LayoutInteractionMoveResult(moveResult.Moved, moveResult.SnapX, moveResult.SnapY, null);
    }

    public LayoutInteractionEndResult EndInteraction(ModifierKeys modifiers)
    {
        if (_mode == EditorInteractionMode.None)
        {
            return LayoutInteractionEndResult.None;
        }

        var previousMode = _mode;
        _mode = EditorInteractionMode.None;
        _session.EndDrag();

        if (previousMode != EditorInteractionMode.Marquee)
        {
            return new LayoutInteractionEndResult(true, false, null);
        }

        var rect = GetMarqueeRect(_lastCanvasPoint);
        var keepExisting = _options.AllowMultiSelect && modifiers.HasFlag(ModifierKeys.Control);
        var hitIds = _session.GetItemsInSelectionRect(rect.X, rect.Y, rect.Width, rect.Height);

        if (keepExisting)
        {
            _session.SelectItems(_session.GetSelectedItemIds().Concat(hitIds), _session.GetSelectedItemId());
        }
        else
        {
            _session.SelectItems(hitIds, hitIds.LastOrDefault());
        }

        return new LayoutInteractionEndResult(true, true, rect);
    }

    private Rect GetMarqueeRect(WpfPoint canvasPoint)
    {
        var left = Math.Min(_startCanvasPoint.X, canvasPoint.X);
        var top = Math.Min(_startCanvasPoint.Y, canvasPoint.Y);
        var width = Math.Abs(canvasPoint.X - _startCanvasPoint.X);
        var height = Math.Abs(canvasPoint.Y - _startCanvasPoint.Y);
        return new Rect(left, top, width, height);
    }

    private enum EditorInteractionMode
    {
        None,
        Drag,
        Resize,
        Marquee
    }
}

public sealed record LayoutInteractionOptions(bool AllowResize, bool AllowMarqueeSelection, bool AllowMultiSelect, bool ClearSelectionOnEmptyClick)
{
    public static LayoutInteractionOptions EditorDefault { get; } = new(true, true, true, true);
    public static LayoutInteractionOptions RuntimeMoveOnly { get; } = new(false, false, false, true);
}

public readonly record struct LayoutInteractionStartResult(
    bool HasHitItem,
    bool CaptureMouse,
    bool ClearedSelection,
    bool ShowMarquee,
    Rect? MarqueeRect)
{
    public static LayoutInteractionStartResult None => new(false, false, false, false, null);
}

public readonly record struct LayoutInteractionMoveResult(
    bool Changed,
    double? SnapX,
    double? SnapY,
    Rect? MarqueeRect)
{
    public static LayoutInteractionMoveResult None => new(false, null, null, null);
}

public readonly record struct LayoutInteractionEndResult(
    bool Ended,
    bool WasMarquee,
    Rect? MarqueeRect)
{
    public static LayoutInteractionEndResult None => new(false, false, null);
}
