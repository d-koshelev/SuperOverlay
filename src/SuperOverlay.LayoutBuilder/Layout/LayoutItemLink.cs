namespace SuperOverlay.LayoutBuilder.Layout;

public sealed record LayoutItemLink(
    Guid SourceItemId,
    Guid TargetItemId,
    LayoutDockSide SourceSide,
    LayoutDockSide TargetSide,
    double Gap);
