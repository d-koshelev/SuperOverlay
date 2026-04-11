using SuperOverlay.LayoutBuilder.Layout;
using SuperOverlay.LayoutBuilder.PanelLayouts;

namespace SuperOverlay.iRacing.Hosting;

public sealed class OverlayEditCommandsService
{
    private readonly LayoutMutationService _mutationService;

    public OverlayEditCommandsService(LayoutMutationService mutationService)
    {
        _mutationService = mutationService ?? throw new ArgumentNullException(nameof(mutationService));
    }

    public bool DeleteSelected(
        ref LayoutDocument layout,
        ref PanelLayoutDocument? panelLayout,
        OverlaySelectionState selection,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        if (selection.ItemIds.Count == 0)
        {
            return false;
        }

        if (panelLayout is not null)
        {
            var selectedPanelIds = GetSelectedPanelIds(selection, panelLayout, compiledPanelItemMap);
            if (selectedPanelIds.Count == 0)
            {
                return false;
            }

            var updatedPanels = panelLayout.Panels.Where(x => !selectedPanelIds.Contains(x.Id)).ToList();
            if (updatedPanels.Count == panelLayout.Panels.Count)
            {
                return false;
            }

            panelLayout = panelLayout with { Panels = updatedPanels };
            return true;
        }

        return _mutationService.DeleteItems(ref layout, selection.ItemIds);
    }

    public bool DuplicateSelected(
        ref LayoutDocument layout,
        ref PanelLayoutDocument? panelLayout,
        OverlaySelectionState selection,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap,
        out IReadOnlyList<Guid> newSelectionIds,
        out Guid? primarySelectionId)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        newSelectionIds = Array.Empty<Guid>();
        primarySelectionId = null;

        if (selection.ItemIds.Count == 0)
        {
            return false;
        }

        if (panelLayout is not null)
        {
            var selectedPanels = GetSelectedPanels(selection, panelLayout, compiledPanelItemMap);
            if (selectedPanels.Count == 0)
            {
                return false;
            }

            var nextZ = panelLayout.Panels.Count == 0 ? 0 : panelLayout.Panels.Max(x => x.ZIndex) + 1;
            var duplicates = selectedPanels
                .OrderBy(x => x.ZIndex)
                .Select((panel, index) => panel with
                {
                    Id = Guid.NewGuid(),
                    X = panel.X + 20,
                    Y = panel.Y + 20,
                    ZIndex = nextZ + index
                })
                .ToList();

            panelLayout = panelLayout with { Panels = panelLayout.Panels.Concat(duplicates).ToList() };
            return true;
        }

        var changed = _mutationService.DuplicateItems(ref layout, selection.ItemIds, out var newItemIds);
        if (!changed)
        {
            return false;
        }

        newSelectionIds = newItemIds;
        primarySelectionId = newItemIds.LastOrDefault();
        return true;
    }

    public bool GroupSelectedItems(ref LayoutDocument layout, OverlaySelectionState selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (selection.ItemIds.Count < 2)
        {
            return false;
        }

        var orderedIds = selection.ItemIds.ToList();
        var anchorId = selection.PrimaryItemId is not null && selection.ItemIds.Contains(selection.PrimaryItemId.Value)
            ? selection.PrimaryItemId.Value
            : orderedIds[0];

        var changed = false;
        foreach (var itemId in orderedIds.Where(x => x != anchorId))
        {
            changed |= _mutationService.GroupItems(ref layout, anchorId, itemId);
        }

        return changed;
    }

    public bool UngroupSelected(ref LayoutDocument layout, OverlaySelectionState selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (selection.ItemIds.Count == 0)
        {
            return false;
        }

        var changed = false;
        foreach (var itemId in selection.ItemIds.ToList())
        {
            changed |= _mutationService.UngroupItem(ref layout, itemId);
        }

        return changed;
    }

    public bool SetLockSelected(
        ref LayoutDocument layout,
        ref PanelLayoutDocument? panelLayout,
        OverlaySelectionState selection,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap,
        bool isLocked)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        if (selection.ItemIds.Count == 0)
        {
            return false;
        }

        if (panelLayout is not null)
        {
            var selectedPanelIds = GetSelectedPanelIds(selection, panelLayout, compiledPanelItemMap);
            if (selectedPanelIds.Count == 0)
            {
                return false;
            }

            var changed = false;
            var updatedPanels = panelLayout.Panels
                .Select(panel =>
                {
                    if (!selectedPanelIds.Contains(panel.Id) || panel.IsLocked == isLocked)
                    {
                        return panel;
                    }

                    changed = true;
                    return panel with { IsLocked = isLocked };
                })
                .ToList();

            if (!changed)
            {
                return false;
            }

            panelLayout = panelLayout with { Panels = updatedPanels };
            return true;
        }

        return _mutationService.ToggleLockItems(ref layout, selection.ItemIds, isLocked);
    }

    public bool ReorderSelected(
        ref LayoutDocument layout,
        ref PanelLayoutDocument? panelLayout,
        OverlaySelectionState selection,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap,
        Func<IReadOnlyList<LayoutItemPlacement>, IEnumerable<LayoutItemPlacement>, Dictionary<Guid, int>> reorder)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);
        ArgumentNullException.ThrowIfNull(reorder);

        if (selection.ItemIds.Count == 0)
        {
            return false;
        }

        if (panelLayout is not null)
        {
            var selectedPanelIds = GetSelectedPanelIds(selection, panelLayout, compiledPanelItemMap).ToHashSet();
            if (selectedPanelIds.Count == 0)
            {
                return false;
            }

            var allPanels = panelLayout.Panels.OrderBy(x => x.ZIndex).ToList();
            var selectedPanels = allPanels.Where(x => selectedPanelIds.Contains(x.Id)).ToList();
            if (selectedPanels.Count == 0)
            {
                return false;
            }

            var allPlacements = allPanels
                .Select(panel => new LayoutItemPlacement(panel.Id, panel.X, panel.Y, 1, 1, panel.ZIndex))
                .ToList();
            var selectedPlacements = allPlacements.Where(x => selectedPanelIds.Contains(x.ItemId)).ToList();
            var zMap = reorder(allPlacements, selectedPlacements);
            if (zMap.Count == 0)
            {
                return false;
            }

            var changed = false;
            var updatedPanels = panelLayout.Panels
                .Select(panel =>
                {
                    if (!zMap.TryGetValue(panel.Id, out var newZ) || panel.ZIndex == newZ)
                    {
                        return panel;
                    }

                    changed = true;
                    return panel with { ZIndex = newZ };
                })
                .ToList();

            if (!changed)
            {
                return false;
            }

            panelLayout = panelLayout with { Panels = updatedPanels };
            return true;
        }

        return _mutationService.SetZIndex(ref layout, selection.ItemIds, reorder);
    }

    public IReadOnlyList<Guid> GetSelectedPanelIds(
        OverlaySelectionState selection,
        PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        if (panelLayout is null || selection.ItemIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        return compiledPanelItemMap
            .Where(x => x.Value.Any(selection.ItemIds.Contains))
            .Select(x => x.Key)
            .ToList();
    }

    public IReadOnlyList<PanelLayoutInstance> GetSelectedPanels(
        OverlaySelectionState selection,
        PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        if (panelLayout is null)
        {
            return Array.Empty<PanelLayoutInstance>();
        }

        var selectedIds = GetSelectedPanelIds(selection, panelLayout, compiledPanelItemMap).ToHashSet();
        if (selectedIds.Count == 0)
        {
            return Array.Empty<PanelLayoutInstance>();
        }

        return panelLayout.Panels.Where(x => selectedIds.Contains(x.Id)).OrderBy(x => x.ZIndex).ToList();
    }
}
