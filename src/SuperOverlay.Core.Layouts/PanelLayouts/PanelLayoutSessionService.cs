using System.Windows;
using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.Panels;

namespace SuperOverlay.Core.Layouts.PanelLayouts;

public sealed class PanelLayoutSessionService
{
    private readonly string _layoutPath;
    private readonly PanelLayoutCompiler _panelLayoutCompiler = new();
    private readonly PanelPresetLibrary _panelPresetLibrary = new();

    public PanelLayoutSessionService(string layoutPath)
    {
        _layoutPath = string.IsNullOrWhiteSpace(layoutPath)
            ? throw new ArgumentException("Layout path is required.", nameof(layoutPath))
            : layoutPath;
    }

    public bool HasPanelLayout(PanelLayoutDocument? panelLayout) => panelLayout is not null;

    public PanelLayoutDocument CreateNew(string name, LayoutDocument layout)
    {
        var canvasWidth = Math.Max(1280, Math.Round(SystemParameters.PrimaryScreenWidth));
        var canvasHeight = Math.Max(720, Math.Round(SystemParameters.PrimaryScreenHeight));

        return new PanelLayoutDocument(
            Version: layout.Version,
            Name: string.IsNullOrWhiteSpace(name) ? "Panel Layout" : name,
            Canvas: new LayoutCanvas(canvasWidth, canvasHeight),
            Panels: Array.Empty<PanelLayoutInstance>());
    }

    public PanelLayoutDocument Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var library = new PanelLayoutLibrary();
        return library.Load(path);
    }

    public void Save(string path, PanelLayoutDocument panelLayout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(panelLayout);

        var library = new PanelLayoutLibrary();
        library.Save(path, panelLayout);
    }

    public LayoutDocument Compile(PanelLayoutDocument panelLayout, out IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> panelItemMap)
    {
        ArgumentNullException.ThrowIfNull(panelLayout);

        var presetEntries = _panelPresetLibrary.List(_panelPresetLibrary.GetDefaultDirectory(_layoutPath))
            .GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.Last());

        return _panelLayoutCompiler.Compile(
            panelLayout,
            presetId => presetEntries.TryGetValue(presetId, out var entry) ? _panelPresetLibrary.Load(entry.Path) : null,
            out panelItemMap);
    }
}
