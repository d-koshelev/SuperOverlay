using SuperOverlay.Core.Layouts.Editing;
using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.PanelLayouts;

namespace SuperOverlay.Core.Layouts.Editor;

public sealed class LayoutSelectionPresentationService
{
    private readonly ILayoutItemMetadataResolver _metadataResolver;
    private readonly OverlayEditCommandsService _editCommandsService;

    public LayoutSelectionPresentationService(ILayoutItemMetadataResolver metadataResolver, OverlayEditCommandsService editCommandsService)
    {
        _metadataResolver = metadataResolver ?? throw new ArgumentNullException(nameof(metadataResolver));
        _editCommandsService = editCommandsService ?? throw new ArgumentNullException(nameof(editCommandsService));
    }

    public LayoutSelectedItemProperties? GetSelectedItemProperties(
        LayoutDocument layout,
        OverlaySelectionState selection,
        PanelLayoutDocument? panelLayout,
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

        var isGrouped = layout.Links.Any(x => x.SourceItemId == item.Id || x.TargetItemId == item.Id);
        return new LayoutSelectedItemProperties(
            item.Id,
            item.TypeId,
            _metadataResolver.GetDisplayName(item.TypeId),
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
                var isGrouped = layout.Links.Any(x => x.SourceItemId == item.Id || x.TargetItemId == item.Id);
                return new LayoutEditorItem(item.Id, item.TypeId, _metadataResolver.GetDisplayName(item.TypeId), isGrouped, item.IsLocked);
            })
            .ToList();
    }

    public IReadOnlyList<Guid> GetSelectedPanelIds(
        OverlaySelectionState selection,
        PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        return _editCommandsService.GetSelectedPanelIds(selection, panelLayout, compiledPanelItemMap);
    }

    public IReadOnlyList<PanelLayoutInstance> GetSelectedPanels(
        OverlaySelectionState selection,
        PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        return _editCommandsService.GetSelectedPanels(selection, panelLayout, compiledPanelItemMap);
    }

    public PanelLayoutInstance? ResolveSelectedPanel(
        Guid? itemId,
        PanelLayoutDocument? panelLayout,
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
