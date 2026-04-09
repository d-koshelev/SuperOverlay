namespace SuperOverlay.LayoutBuilder.Layout;

public sealed record LayoutDocument(
    string Version,
    string Name,
    LayoutCanvas Canvas,
    IReadOnlyList<LayoutItemInstance> Items,
    IReadOnlyList<LayoutItemPlacement> Placements,
    IReadOnlyList<LayoutItemLink> Links);
