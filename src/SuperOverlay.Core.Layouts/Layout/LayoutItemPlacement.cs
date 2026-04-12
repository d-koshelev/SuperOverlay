namespace SuperOverlay.Core.Layouts.Layout;

public sealed record LayoutItemPlacement(
    Guid ItemId,
    double X,
    double Y,
    double Width,
    double Height,
    int ZIndex,
    double RuntimeDeltaX = 0,
    double RuntimeDeltaY = 0,
    double? RuntimeX = null,
    double? RuntimeY = null,
    bool HasRuntimeOverride = false);
