using System.Collections.Generic;
using System.Linq;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorWorkspaceService
{
    private readonly LayoutEditorPresetStore _presetStore;
    private readonly LayoutEditorLayoutStore _layoutStore;

    public LayoutEditorWorkspaceService(LayoutEditorPresetStore presetStore, LayoutEditorLayoutStore layoutStore)
    {
        _presetStore = presetStore;
        _layoutStore = layoutStore;
    }

    public IReadOnlyList<string> ListPresetNames() => _presetStore.ListPresetNames();

    public LayoutEditorPresetDocument? LoadPreset(string presetName) => _presetStore.Load(presetName);

    public void SavePreset(string presetName, IReadOnlyList<LayoutEditorWidget> widgets)
    {
        var ordered = widgets.OrderBy(x => x.Y).ThenBy(x => x.X).ToList();
        var minX = ordered.Min(x => x.X);
        var minY = ordered.Min(x => x.Y);

        var document = new LayoutEditorPresetDocument
        {
            Name = presetName,
            Widgets = ordered.Select(x => LayoutEditorDocumentMapper.ToPresetWidget(x, minX, minY)).ToList(),
        };

        _presetStore.Save(document);
    }

    public string SuggestPresetName(IReadOnlyList<LayoutEditorWidget> widgets) => LayoutEditorPresetStore.SuggestPresetName(widgets);

    public IReadOnlyList<string> ListLayoutNames() => _layoutStore.ListLayoutNames();

    public LayoutEditorLayoutDocument? LoadLayout(string layoutName) => _layoutStore.Load(layoutName);

    public LayoutEditorLayoutDocument SaveLayout(string layoutName, IReadOnlyList<LayoutEditorWidget> widgets)
    {
        var document = new LayoutEditorLayoutDocument
        {
            Name = layoutName,
            Widgets = widgets
                .OrderBy(x => x.Y)
                .ThenBy(x => x.X)
                .Select(LayoutEditorDocumentMapper.ToLayoutWidget)
                .ToList(),
        };

        _layoutStore.Save(document);
        return document;
    }

    public string SuggestLayoutName(string? currentLayoutName, int widgetCount) => LayoutEditorLayoutStore.SuggestLayoutName(currentLayoutName, widgetCount);
}
