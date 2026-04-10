namespace SuperOverlay.LayoutBuilder.Layout;

public sealed record LayoutItemInstance(
    Guid Id,
    string TypeId,
    object Settings,
    bool IsLocked = false);
