namespace SuperOverlay.LayoutBuilder.PanelLayouts;

using SuperOverlay.LayoutBuilder.Layout;

public sealed record PanelLayoutInstance(
    Guid Id,
    Guid PanelPresetId,
    string PanelName,
    string Category,
    double X,
    double Y,
    int ZIndex,
    bool IsLocked = false,
    double Scale = 1.0,
    bool IsVisible = true);

public sealed record PanelLayoutDocument(
    string Version,
    string Name,
    LayoutCanvas Canvas,
    IReadOnlyList<PanelLayoutInstance> Panels);
