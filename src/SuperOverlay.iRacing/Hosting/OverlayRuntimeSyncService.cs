using SuperOverlay.LayoutBuilder.Layout;
using SuperOverlay.LayoutBuilder.Runtime;

namespace SuperOverlay.iRacing.Hosting;

public sealed class OverlayRuntimeSyncService
{
    private readonly LayoutHost _layoutHost;
    private readonly LayoutRuntimeComposer _composer;
    private readonly OverlaySelectionService _selectionService;
    private readonly OverlayShellMode _shellMode;

    public OverlayRuntimeSyncService(
        LayoutHost layoutHost,
        LayoutRuntimeComposer composer,
        OverlaySelectionService selectionService,
        OverlayShellMode shellMode)
    {
        _layoutHost = layoutHost ?? throw new ArgumentNullException(nameof(layoutHost));
        _composer = composer ?? throw new ArgumentNullException(nameof(composer));
        _selectionService = selectionService ?? throw new ArgumentNullException(nameof(selectionService));
        _shellMode = shellMode;
    }

    public void RefreshRuntime(LayoutDocument layout, OverlaySelectionState selection)
    {
        var runtimeItems = _composer.Compose(layout);
        _layoutHost.Load(runtimeItems);
        SyncSelectionState(layout, selection);
    }

    public void SyncSelectionState(LayoutDocument layout, OverlaySelectionState selection)
    {
        _layoutHost.SetSelectedItems(selection.PrimaryItemId, selection.ItemIds);
        _layoutHost.SetGroupHighlight(_selectionService.GetHighlightedGroupItemIds(layout, selection.PrimaryItemId));
    }

    public void SyncPlacementsToRuntime(LayoutDocument layout, IReadOnlyCollection<Guid> itemIds)
    {
        foreach (var itemId in itemIds)
        {
            SyncPlacementToRuntime(layout, itemId);
        }
    }

    public void SyncPlacementToRuntime(LayoutDocument layout, Guid itemId)
    {
        var placement = layout.Placements.FirstOrDefault(x => x.ItemId == itemId);
        if (placement is null)
        {
            return;
        }

        _layoutHost.TryUpdatePlacement(itemId, ResolvePlacementForShell(layout.Canvas, placement));
    }

    public void SyncAllPlacementsToRuntime(LayoutDocument layout)
    {
        foreach (var placement in layout.Placements)
        {
            _layoutHost.TryUpdatePlacement(placement.ItemId, ResolvePlacementForShell(layout.Canvas, placement));
        }
    }

    public Guid? HitTestItemId(object? hitSource)
    {
        return hitSource is null ? null : _layoutHost.HitTestItem(hitSource as System.Windows.DependencyObject)?.Item.Id;
    }

    public bool IsResizeHandleHit(object? hitSource, Guid itemId)
    {
        return hitSource is not null && _layoutHost.IsResizeHandleHit(hitSource as System.Windows.DependencyObject, itemId);
    }

    private LayoutItemPlacement ResolvePlacementForShell(LayoutCanvas canvas, LayoutItemPlacement placement)
    {
        return LayoutPlacementResolver.ResolveForShell(placement, canvas, _shellMode);
    }
}
