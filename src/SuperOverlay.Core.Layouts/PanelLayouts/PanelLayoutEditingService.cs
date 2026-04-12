using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.Panels;

namespace SuperOverlay.Core.Layouts.PanelLayouts;

public sealed class PanelLayoutEditingService
{
    public bool InsertPanelPresetAsPanelInstance(PanelLayoutDocument? panelLayout, PanelPresetDocument preset, double x, double y, out PanelLayoutDocument? updatedPanelLayout)
    {
        ArgumentNullException.ThrowIfNull(preset);
        updatedPanelLayout = panelLayout;
        if (panelLayout is null)
        {
            return false;
        }

        var nextZ = panelLayout.Panels.Count == 0 ? 0 : panelLayout.Panels.Max(x => x.ZIndex) + 1;
        var panel = new PanelLayoutInstance(
            Id: Guid.NewGuid(),
            PanelPresetId: preset.Metadata.Id,
            PanelName: preset.Metadata.Name,
            Category: preset.Metadata.Category,
            X: x,
            Y: y,
            ZIndex: nextZ,
            IsLocked: false,
            Scale: 1.0,
            IsVisible: true);

        updatedPanelLayout = panelLayout with { Panels = panelLayout.Panels.Concat(new[] { panel }).ToList() };
        return true;
    }

    public PanelLayoutDocument? SyncPanelLayoutFromCompiledLayout(
        PanelLayoutDocument? panelLayout,
        LayoutDocument layout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        if (panelLayout is null || compiledPanelItemMap.Count == 0)
        {
            return panelLayout;
        }

        var updatedPanels = new List<PanelLayoutInstance>(panelLayout.Panels.Count);
        foreach (var panel in panelLayout.Panels)
        {
            if (!compiledPanelItemMap.TryGetValue(panel.Id, out var itemIds) || itemIds.Count == 0)
            {
                updatedPanels.Add(panel);
                continue;
            }

            var placements = layout.Placements.Where(x => itemIds.Contains(x.ItemId)).ToList();
            if (placements.Count == 0)
            {
                updatedPanels.Add(panel);
                continue;
            }

            var items = layout.Items.Where(x => itemIds.Contains(x.Id)).ToList();
            updatedPanels.Add(panel with
            {
                X = placements.Min(x => x.X),
                Y = placements.Min(x => x.Y),
                ZIndex = placements.Min(x => x.ZIndex),
                IsLocked = items.Count > 0 && items.All(x => x.IsLocked)
            });
        }

        return panelLayout with { Panels = updatedPanels };
    }
}
