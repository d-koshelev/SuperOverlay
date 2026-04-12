namespace SuperOverlay.Core.Layouts.Panels;

using SuperOverlay.Core.Layouts.Layout;

public sealed record PanelPresetMetadata(
    Guid Id,
    string Name,
    string Category,
    double Width,
    double Height);

public sealed record PanelPresetDocument(
    string Version,
    PanelPresetMetadata Metadata,
    IReadOnlyList<LayoutItemInstance> Items,
    IReadOnlyList<LayoutItemPlacement> Placements,
    IReadOnlyList<LayoutItemLink> Links);

public sealed record PanelPresetLibraryEntry(
    Guid Id,
    string Name,
    string Category,
    string Path,
    double Width,
    double Height,
    int ItemCount);
