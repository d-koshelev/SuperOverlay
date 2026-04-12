using SuperOverlay.LayoutBuilder.Layout;
using SuperOverlay.LayoutBuilder.PanelLayouts;
using SuperOverlay.LayoutBuilder.Runtime;

namespace SuperOverlay.iRacing.Hosting;

public sealed class OverlayMovementService
{
    private readonly LayoutMutationService _mutationService;
    private readonly LayoutSnapService _snapService;
    private readonly OverlaySelectionService _selectionService;

    public OverlayMovementService(
        LayoutMutationService mutationService,
        LayoutSnapService snapService,
        OverlaySelectionService selectionService)
    {
        _mutationService = mutationService ?? throw new ArgumentNullException(nameof(mutationService));
        _snapService = snapService ?? throw new ArgumentNullException(nameof(snapService));
        _selectionService = selectionService ?? throw new ArgumentNullException(nameof(selectionService));
    }

    public bool UpdateSelectedItemProperties(
        ref LayoutDocument layout,
        ref PanelLayoutDocument? panelLayout,
        OverlaySelectionState selection,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap,
        double x,
        double y,
        double width,
        double height,
        int zIndex,
        bool isLocked)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        if (panelLayout is not null)
        {
            var selectedPanelIds = GetSelectedPanelIds(selection, panelLayout, compiledPanelItemMap);
            if (selectedPanelIds.Count == 0)
            {
                return false;
            }

            var primaryPanel = ResolveSelectedPanel(selection.PrimaryItemId, panelLayout, compiledPanelItemMap);
            var changed = false;
            var updatedPanels = panelLayout.Panels
                .Select(panel =>
                {
                    if (!selectedPanelIds.Contains(panel.Id))
                    {
                        return panel;
                    }

                    var updatedPanel = primaryPanel is not null && panel.Id == primaryPanel.Id
                        ? panel with { X = x, Y = y, ZIndex = zIndex, IsLocked = isLocked }
                        : panel with { IsLocked = isLocked };

                    if (!Equals(updatedPanel, panel))
                    {
                        changed = true;
                    }

                    return updatedPanel;
                })
                .ToList();

            if (!changed)
            {
                return false;
            }

            panelLayout = panelLayout with { Panels = updatedPanels };
            return true;
        }

        if (selection.PrimaryItemId is null)
        {
            return false;
        }

        return _mutationService.UpdateItemProperties(ref layout, selection.PrimaryItemId.Value, x, y, width, height, zIndex, isLocked);
    }

    public LayoutMoveResult MoveSelectedWithSnap(
        ref LayoutDocument layout,
        OverlaySelectionState selection,
        PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap,
        double deltaX,
        double deltaY,
        double canvasWidth,
        double canvasHeight,
        bool snappingEnabled,
        bool bypassSnap,
        OverlayShellMode shellMode)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        if (selection.PrimaryItemId is null)
        {
            return new LayoutMoveResult(false, null, null);
        }

        var layoutSnapshot = layout;

        if (shellMode != OverlayShellMode.Editor)
        {
            var runtimeMoveIds = _selectionService.GetActiveMoveItemIds(layoutSnapshot, selection, panelLayout, compiledPanelItemMap)
                .Where(id => !IsLocked(layoutSnapshot, id))
                .ToList();
            if (runtimeMoveIds.Count == 0)
            {
                return new LayoutMoveResult(false, null, null);
            }

            var runtimePlacements = layoutSnapshot.Placements
                .Where(x => runtimeMoveIds.Contains(x.ItemId))
                .Select(x => ResolvePlacementForShell(layoutSnapshot, x, shellMode))
                .ToList();
            if (runtimePlacements.Count == 0)
            {
                return new LayoutMoveResult(false, null, null);
            }

            var runtimeBounds = GetBounds(runtimePlacements);
            var targetX = runtimeBounds.X + deltaX;
            var targetY = runtimeBounds.Y + deltaY;
            var finalX = Math.Clamp(targetX, 0, Math.Max(0, canvasWidth - runtimeBounds.Width));
            var finalY = Math.Clamp(targetY, 0, Math.Max(0, canvasHeight - runtimeBounds.Height));
            var deltaAppliedX = finalX - runtimeBounds.X;
            var deltaAppliedY = finalY - runtimeBounds.Y;

            var runtimeChanged = _mutationService.MoveItemsRuntimeDeltaBy(ref layout, runtimeMoveIds, deltaAppliedX, deltaAppliedY);
            return new LayoutMoveResult(runtimeChanged, null, null);
        }

        var groupItemIds = _selectionService.GetActiveMoveItemIds(layoutSnapshot, selection, panelLayout, compiledPanelItemMap);
        var movableItemIds = groupItemIds.Where(id => !IsLocked(layoutSnapshot, id)).ToList();
        if (movableItemIds.Count == 0)
        {
            return new LayoutMoveResult(false, null, null);
        }

        var groupPlacements = layoutSnapshot.Placements
            .Where(x => movableItemIds.Contains(x.ItemId))
            .Select(x => ResolvePlacementForShell(layoutSnapshot, x, shellMode))
            .ToList();
        if (groupPlacements.Count == 0)
        {
            return new LayoutMoveResult(false, null, null);
        }

        var groupBounds = GetBounds(groupPlacements);
        var targetGroupX = groupBounds.X + deltaX;
        var targetGroupY = groupBounds.Y + deltaY;
        var finalGroupX = targetGroupX;
        var finalGroupY = targetGroupY;
        double? snapX = null;
        double? snapY = null;

        if (snappingEnabled && !bypassSnap)
        {
            var snapped = _snapService.SnapGroupPosition(layout, movableItemIds, targetGroupX, targetGroupY, groupBounds.Width, groupBounds.Height, canvasWidth, canvasHeight);
            finalGroupX = snapped.X;
            finalGroupY = snapped.Y;
            snapX = snapped.SnapX;
            snapY = snapped.SnapY;
        }

        var appliedDeltaX = finalGroupX - groupBounds.X;
        var appliedDeltaY = finalGroupY - groupBounds.Y;
        var changedGroup = _mutationService.MoveItemsBy(ref layout, movableItemIds, appliedDeltaX, appliedDeltaY);
        return new LayoutMoveResult(changedGroup, snapX, snapY);
    }

    public LayoutMoveResult ResizeSelectedWithSnap(
        ref LayoutDocument layout,
        OverlaySelectionState selection,
        double deltaWidth,
        double deltaHeight,
        double canvasWidth,
        double canvasHeight,
        bool snappingEnabled,
        bool bypassSnap)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (selection.PrimaryItemId is null || IsLocked(layout, selection.PrimaryItemId.Value))
        {
            return new LayoutMoveResult(false, null, null);
        }

        var placement = layout.Placements.FirstOrDefault(x => x.ItemId == selection.PrimaryItemId.Value);
        if (placement is null)
        {
            return new LayoutMoveResult(false, null, null);
        }

        var targetWidth = placement.Width + deltaWidth;
        var targetHeight = placement.Height + deltaHeight;
        var finalWidth = targetWidth;
        var finalHeight = targetHeight;
        double? snapX = null;
        double? snapY = null;

        if (snappingEnabled && !bypassSnap)
        {
            var snapped = _snapService.SnapResize(layout, selection.PrimaryItemId.Value, targetWidth, targetHeight, canvasWidth, canvasHeight);
            finalWidth = snapped.Width;
            finalHeight = snapped.Height;
            snapX = snapped.SnapX;
            snapY = snapped.SnapY;
        }

        var changed = _mutationService.ResizeItemTo(ref layout, selection.PrimaryItemId.Value, finalWidth, finalHeight);
        return new LayoutMoveResult(changed, snapX, snapY);
    }

    private static bool IsLocked(LayoutDocument layout, Guid itemId)
    {
        return layout.Items.Any(x => x.Id == itemId && x.IsLocked);
    }

    private static LayoutItemPlacement ResolvePlacementForShell(LayoutDocument layout, LayoutItemPlacement placement, OverlayShellMode shellMode)
    {
        return LayoutPlacementResolver.ResolveForShell(placement, layout.Canvas, shellMode);
    }

    private static (double X, double Y, double Width, double Height) GetBounds(IReadOnlyList<LayoutItemPlacement> placements)
    {
        var left = placements.Min(x => x.X);
        var top = placements.Min(x => x.Y);
        var right = placements.Max(x => x.X + x.Width);
        var bottom = placements.Max(x => x.Y + x.Height);
        return (left, top, right - left, bottom - top);
    }

    private static PanelLayoutInstance? ResolveSelectedPanel(
        Guid? selectedItemId,
        PanelLayoutDocument panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        if (selectedItemId is null)
        {
            return null;
        }

        foreach (var panel in panelLayout.Panels)
        {
            if (compiledPanelItemMap.TryGetValue(panel.Id, out var itemIds) && itemIds.Contains(selectedItemId.Value))
            {
                return panel;
            }
        }

        return null;
    }

    private static IReadOnlyList<Guid> GetSelectedPanelIds(
        OverlaySelectionState selection,
        PanelLayoutDocument panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        return compiledPanelItemMap
            .Where(x => x.Value.Any(selection.ItemIds.Contains))
            .Select(x => x.Key)
            .Where(id => panelLayout.Panels.Any(panel => panel.Id == id))
            .ToList();
    }
}
