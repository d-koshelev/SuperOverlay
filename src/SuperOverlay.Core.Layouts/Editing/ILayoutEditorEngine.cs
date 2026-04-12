using System.Windows;

namespace SuperOverlay.Core.Layouts.Editing;

public interface ILayoutEditorEngine
{
    void SyncWidgets(IReadOnlyCollection<LayoutEditorEngineWidgetInput> widgets, Size canvasSize);
    void SyncSelection(IReadOnlyCollection<Guid> selectedIds, Guid? primaryId);
    LayoutEditorEngineMoveResult MoveSelection(double deltaX, double deltaY, Size canvasSize, bool bypassSnap = false);
    LayoutEditorEngineMoveResult ResizeSelection(double deltaX, double deltaY, Size canvasSize, bool bypassSnap = false);
    IReadOnlyList<Guid> GetItemsInSelectionRect(Rect rect, bool requireFullContainment = false);
    IReadOnlyCollection<Guid> NormalizeSelection(IReadOnlyCollection<Guid> selectedIds, Guid? primaryId);
    IReadOnlyList<Guid> DuplicateSelection();
    bool DeleteSelection();
    bool GroupSelection();
    bool UngroupSelection();
    bool BringToFrontSelection();
    bool SendToBackSelection();
    bool BringForwardSelection();
    bool SendBackwardSelection();
    bool SetLockSelection(bool isLocked);
    LayoutEditorEngineSnapshot GetSnapshot();
}

public sealed record LayoutEditorEngineWidgetInput(
    Guid Id,
    double X,
    double Y,
    double Width,
    double Height,
    int ZIndex = 0,
    bool IsLocked = false,
    Guid? LinkedGroupKey = null);

public sealed record LayoutEditorEngineMoveResult(bool Changed, double? SnapX = null, double? SnapY = null);

public sealed record LayoutEditorEngineSnapshot(
    IReadOnlyDictionary<Guid, LayoutEditorEngineWidgetState> Widgets,
    IReadOnlyCollection<Guid> SelectedIds,
    Guid? PrimarySelectedId);

public sealed record LayoutEditorEngineWidgetState(
    Guid Id,
    double X,
    double Y,
    double Width,
    double Height,
    int ZIndex = 0,
    bool IsLocked = false,
    Guid? LinkedGroupKey = null);
