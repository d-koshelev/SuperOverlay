using SuperOverlay.Core.Layouts.Layout;

namespace SuperOverlay.Core.Layouts.Panels;

public sealed class PanelPresetEditingService
{
    private readonly PanelPresetComposer _composer = new();

    public PanelPresetDocument? CreateSelectedPanelPreset(LayoutDocument layout, IReadOnlyList<Guid> selectedItemIds, string name, string category = "Custom")
    {
        if (selectedItemIds.Count == 0)
        {
            return null;
        }

        return _composer.CreateFromLayoutSelection(layout, selectedItemIds, name, category);
    }

    public bool InsertPanelPreset(LayoutDocument layout, PanelPresetDocument preset, double x, double y, out LayoutDocument updatedLayout, out IReadOnlyList<Guid> insertedItemIds)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(preset);

        updatedLayout = _composer.InsertIntoLayout(layout, preset, x, y, out insertedItemIds);
        return insertedItemIds.Count > 0;
    }

    public bool OpenPanelPresetForEditing(LayoutDocument sourceLayout, PanelPresetDocument preset, double x, double y, out LayoutDocument updatedLayout, out IReadOnlyList<Guid> insertedItemIds)
    {
        ArgumentNullException.ThrowIfNull(sourceLayout);
        ArgumentNullException.ThrowIfNull(preset);

        var emptyLayout = new LayoutDocument(
            sourceLayout.Version,
            $"Panel - {preset.Metadata.Name}",
            sourceLayout.Canvas,
            Array.Empty<LayoutItemInstance>(),
            Array.Empty<LayoutItemPlacement>(),
            Array.Empty<LayoutItemLink>());

        updatedLayout = _composer.InsertIntoLayout(emptyLayout, preset, x, y, out insertedItemIds);
        return insertedItemIds.Count > 0;
    }
}
