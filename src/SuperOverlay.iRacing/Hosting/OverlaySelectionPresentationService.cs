using SuperOverlay.Dashboards.Registry;
using SuperOverlay.LayoutBuilder.Layout;

namespace SuperOverlay.iRacing.Hosting;

public sealed class OverlaySelectionPresentationService
{
    private readonly DashboardRegistry _registry;
    private readonly OverlayEditCommandsService _editCommandsService;

    public OverlaySelectionPresentationService(DashboardRegistry registry, OverlayEditCommandsService editCommandsService)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _editCommandsService = editCommandsService ?? throw new ArgumentNullException(nameof(editCommandsService));
    }

    public LayoutSelectedItemProperties? GetSelectedItemProperties(
        LayoutDocument layout,
        OverlaySelectionState selection,
        global::SuperOverlay.LayoutBuilder.PanelLayouts.PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        if (panelLayout is not null)
        {
            var selectedPanels = GetSelectedPanels(selection, panelLayout, compiledPanelItemMap);
            if (selectedPanels.Count == 0)
            {
                return null;
            }

            var primaryPanel = ResolveSelectedPanel(selection.PrimaryItemId, panelLayout, compiledPanelItemMap) ?? selectedPanels[0];
            var compiledItemIds = compiledPanelItemMap.TryGetValue(primaryPanel.Id, out var itemIds) ? itemIds : Array.Empty<Guid>();
            var placements = layout.Placements.Where(x => compiledItemIds.Contains(x.ItemId)).ToList();
            if (placements.Count == 0)
            {
                return null;
            }

            var bounds = GetBounds(placements);
            return new LayoutSelectedItemProperties(
                primaryPanel.Id,
                "panel.instance",
                primaryPanel.PanelName,
                primaryPanel.X,
                primaryPanel.Y,
                bounds.Width,
                bounds.Height,
                primaryPanel.ZIndex,
                primaryPanel.IsLocked,
                selectedPanels.Count,
                true);
        }

        if (selection.PrimaryItemId is null)
        {
            return null;
        }

        var item = layout.Items.FirstOrDefault(x => x.Id == selection.PrimaryItemId.Value);
        var placement = layout.Placements.FirstOrDefault(x => x.ItemId == selection.PrimaryItemId.Value);
        if (item is null || placement is null)
        {
            return null;
        }

        var definition = _registry.Get(item.TypeId);
        var isGrouped = layout.Links.Any(x => x.SourceItemId == item.Id || x.TargetItemId == item.Id);
        return new LayoutSelectedItemProperties(
            item.Id,
            item.TypeId,
            definition.DisplayName,
            placement.X,
            placement.Y,
            placement.Width,
            placement.Height,
            placement.ZIndex,
            item.IsLocked,
            selection.ItemIds.Count,
            isGrouped);
    }

    public IReadOnlyList<LayoutEditorItem> GetLayoutItems(LayoutDocument layout)
    {
        return layout.Items
            .Select(item =>
            {
                var definition = _registry.Get(item.TypeId);
                var isGrouped = layout.Links.Any(x => x.SourceItemId == item.Id || x.TargetItemId == item.Id);
                return new LayoutEditorItem(item.Id, item.TypeId, definition.DisplayName, isGrouped, item.IsLocked);
            })
            .ToList();
    }

    public IReadOnlyList<Guid> GetSelectedPanelIds(
        OverlaySelectionState selection,
        global::SuperOverlay.LayoutBuilder.PanelLayouts.PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        return _editCommandsService.GetSelectedPanelIds(selection, panelLayout, compiledPanelItemMap);
    }

    public IReadOnlyList<global::SuperOverlay.LayoutBuilder.PanelLayouts.PanelLayoutInstance> GetSelectedPanels(
        OverlaySelectionState selection,
        global::SuperOverlay.LayoutBuilder.PanelLayouts.PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        return _editCommandsService.GetSelectedPanels(selection, panelLayout, compiledPanelItemMap);
    }

    public global::SuperOverlay.LayoutBuilder.PanelLayouts.PanelLayoutInstance? ResolveSelectedPanel(
        Guid? itemId,
        global::SuperOverlay.LayoutBuilder.PanelLayouts.PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        if (panelLayout is null || itemId is null)
        {
            return null;
        }

        foreach (var panel in panelLayout.Panels)
        {
            if (compiledPanelItemMap.TryGetValue(panel.Id, out var itemIds) && itemIds.Contains(itemId.Value))
            {
                return panel;
            }
        }

        return null;
    }

    private static (double X, double Y, double Width, double Height) GetBounds(IReadOnlyList<LayoutItemPlacement> placements)
    {
        var left = placements.Min(x => x.X);
        var top = placements.Min(x => x.Y);
        var right = placements.Max(x => x.X + x.Width);
        var bottom = placements.Max(x => x.Y + x.Height);
        return (left, top, right - left, bottom - top);
    }
}
