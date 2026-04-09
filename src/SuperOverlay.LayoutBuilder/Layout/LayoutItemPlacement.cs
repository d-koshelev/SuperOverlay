namespace SuperOverlay.LayoutBuilder.Layout;

public sealed record LayoutItemPlacement(
    Guid ItemId,
    double X,
    double Y,
    double Width,
    double Height,
    int ZIndex);
