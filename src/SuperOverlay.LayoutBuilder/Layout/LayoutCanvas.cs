namespace SuperOverlay.LayoutBuilder.Layout;

public sealed record LayoutCanvas(
    double Width,
    double Height,
    double RuntimeOffsetX = 0,
    double RuntimeOffsetY = 0);
