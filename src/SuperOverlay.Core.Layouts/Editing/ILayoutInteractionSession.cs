namespace SuperOverlay.Core.Layouts.Editing;

public interface ILayoutInteractionSession
{
    Guid? HitTestItemId(object? hitSource);
    void SelectItem(Guid? itemId);
    void ToggleItemSelection(Guid itemId);
    void SetPrimarySelection(Guid itemId);
    bool IsLocked(Guid itemId);
    bool IsResizeHandleHit(object? hitSource, Guid itemId);
    LayoutMoveResult ResizeSelectedWithSnap(double deltaWidth, double deltaHeight, double canvasWidth, double canvasHeight, bool bypassSnap);
    LayoutMoveResult MoveSelectedWithSnap(double deltaX, double deltaY, double canvasWidth, double canvasHeight, bool bypassSnap);
    IReadOnlyList<Guid> GetItemsInSelectionRect(double x, double y, double width, double height, bool requireFullContainment = false);
    IEnumerable<Guid> GetSelectedItemIds();
    Guid? GetSelectedItemId();
    void SelectItems(IEnumerable<Guid> itemIds, Guid? primaryItemId = null);
    void EndDrag();
}
