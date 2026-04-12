using System.Windows;
using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.Runtime;

namespace SuperOverlay.Core.Layouts.Editing;

public class LayoutEditorEngine : ILayoutEditorEngine
{
    private readonly LayoutMutationCore _mutationService;
    private readonly OverlaySelectionService _selectionService;
    private readonly LayoutSnapService _snapService;
    private readonly OverlayMovementService _movementService;
    private readonly OverlaySelectionState _selection = new();
    private static readonly IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> EmptyCompiledPanels = new Dictionary<Guid, IReadOnlyList<Guid>>();
    private LayoutDocument _layout;

    public LayoutEditorEngine()
    {
        _mutationService = new LayoutMutationCore();
        _selectionService = new OverlaySelectionService();
        _snapService = new LayoutSnapService();
        _movementService = new OverlayMovementService(_mutationService, _snapService, _selectionService);
        _layout = CreateEmptyDocument(new Size(1920, 1080));
    }

    public void SyncWidgets(IReadOnlyCollection<LayoutEditorEngineWidgetInput> widgets, Size canvasSize)
    {
        _layout = CreateDocumentFromWidgets(widgets, canvasSize);
    }

    public void SyncSelection(IReadOnlyCollection<Guid> selectedIds, Guid? primaryId)
    {
        var normalized = _selectionService.NormalizeSelection(_layout, selectedIds, primaryId, null, EmptyCompiledPanels);
        _selection.Replace(normalized.ItemIds, normalized.PrimaryItemId);
    }

    public LayoutEditorEngineMoveResult MoveSelection(double deltaX, double deltaY, Size canvasSize, bool bypassSnap = false)
    {
        var result = _movementService.MoveSelectedWithSnap(
            ref _layout,
            _selection,
            null,
            EmptyCompiledPanels,
            deltaX,
            deltaY,
            canvasSize.Width,
            canvasSize.Height,
            true,
            bypassSnap,
            OverlayShellMode.Editor);

        return new LayoutEditorEngineMoveResult(result.Moved, result.SnapX, result.SnapY);
    }

    public LayoutEditorEngineMoveResult ResizeSelection(double deltaX, double deltaY, Size canvasSize, bool bypassSnap = false)
    {
        var result = _movementService.ResizeSelectedWithSnap(
            ref _layout,
            _selection,
            deltaX,
            deltaY,
            canvasSize.Width,
            canvasSize.Height,
            true,
            bypassSnap);

        return new LayoutEditorEngineMoveResult(result.Moved, result.SnapX, result.SnapY);
    }

    public IReadOnlyList<Guid> GetItemsInSelectionRect(Rect rect, bool requireFullContainment = false)
    {
        var left = Math.Min(rect.Left, rect.Right);
        var top = Math.Min(rect.Top, rect.Bottom);
        var right = Math.Max(rect.Left, rect.Right);
        var bottom = Math.Max(rect.Top, rect.Bottom);

        return _layout.Placements
            .Select(x => LayoutPlacementResolver.ResolveForShell(x, _layout.Canvas, OverlayShellMode.Editor))
            .Where(p =>
            {
                var itemLeft = p.X;
                var itemTop = p.Y;
                var itemRight = p.X + p.Width;
                var itemBottom = p.Y + p.Height;

                return requireFullContainment
                    ? itemLeft >= left && itemTop >= top && itemRight <= right && itemBottom <= bottom
                    : !(itemRight < left || itemBottom < top || itemLeft > right || itemTop > bottom);
            })
            .OrderBy(p => p.ZIndex)
            .Select(p => p.ItemId)
            .ToList();
    }

    public IReadOnlyCollection<Guid> NormalizeSelection(IReadOnlyCollection<Guid> selectedIds, Guid? primaryId)
    {
        var normalized = _selectionService.NormalizeSelection(_layout, selectedIds, primaryId, null, EmptyCompiledPanels);
        return normalized.ItemIds;
    }

    public IReadOnlyList<Guid> DuplicateSelection()
    {
        if (_selection.ItemIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        if (!_mutationService.DuplicateItems(ref _layout, _selection.ItemIds.ToList(), out var newItemIds) || newItemIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        _selection.Replace(newItemIds, newItemIds.FirstOrDefault());
        return newItemIds;
    }

    public bool DeleteSelection()
    {
        if (_selection.ItemIds.Count == 0)
        {
            return false;
        }

        var changed = _mutationService.DeleteItems(ref _layout, _selection.ItemIds.ToList());
        if (changed)
        {
            _selection.Replace(Array.Empty<Guid>(), null);
        }

        return changed;
    }

    public bool GroupSelection()
    {
        if (_selection.ItemIds.Count < 2 || _selection.PrimaryItemId is null)
        {
            return false;
        }

        var anchorId = _selection.PrimaryItemId.Value;
        var changed = false;
        foreach (var targetId in _selection.ItemIds.Where(x => x != anchorId))
        {
            changed |= _mutationService.GroupItems(ref _layout, anchorId, targetId);
        }

        return changed;
    }

    public bool UngroupSelection()
    {
        if (_selection.ItemIds.Count == 0)
        {
            return false;
        }

        var changed = false;
        foreach (var itemId in _selection.ItemIds.ToList())
        {
            changed |= _mutationService.UngroupItem(ref _layout, itemId);
        }

        return changed;
    }

    public bool BringToFrontSelection() => ReorderSelected((all, selected) =>
    {
        var selectedIds = selected.Select(x => x.ItemId).ToHashSet();
        var ordered = all.Where(x => !selectedIds.Contains(x.ItemId)).Concat(selected).ToList();
        return BuildZMap(ordered);
    });

    public bool SendToBackSelection() => ReorderSelected((all, selected) =>
    {
        var selectedIds = selected.Select(x => x.ItemId).ToHashSet();
        var ordered = selected.Concat(all.Where(x => !selectedIds.Contains(x.ItemId))).ToList();
        return BuildZMap(ordered);
    });

    public bool BringForwardSelection() => ReorderSelected((all, selected) =>
    {
        var list = all.ToList();
        var selectedIds = selected.Select(x => x.ItemId).ToHashSet();
        for (var i = list.Count - 2; i >= 0; i--)
        {
            if (selectedIds.Contains(list[i].ItemId) && !selectedIds.Contains(list[i + 1].ItemId))
            {
                (list[i], list[i + 1]) = (list[i + 1], list[i]);
            }
        }

        return BuildZMap(list);
    });

    public bool SendBackwardSelection() => ReorderSelected((all, selected) =>
    {
        var list = all.ToList();
        var selectedIds = selected.Select(x => x.ItemId).ToHashSet();
        for (var i = 1; i < list.Count; i++)
        {
            if (selectedIds.Contains(list[i].ItemId) && !selectedIds.Contains(list[i - 1].ItemId))
            {
                (list[i], list[i - 1]) = (list[i - 1], list[i]);
            }
        }

        return BuildZMap(list);
    });

    public bool SetLockSelection(bool isLocked)
    {
        if (_selection.ItemIds.Count == 0)
        {
            return false;
        }

        return _mutationService.ToggleLockItems(ref _layout, _selection.ItemIds.ToList(), isLocked);
    }

    public LayoutEditorEngineSnapshot GetSnapshot()
    {
        var groupKeys = BuildGroupKeys();
        var widgets = _layout.Placements.ToDictionary(
            x => x.ItemId,
            x => new LayoutEditorEngineWidgetState(
                x.ItemId,
                x.X,
                x.Y,
                x.Width,
                x.Height,
                x.ZIndex,
                _layout.Items.Any(i => i.Id == x.ItemId && i.IsLocked),
                groupKeys.TryGetValue(x.ItemId, out var groupKey) ? groupKey : null));

        return new LayoutEditorEngineSnapshot(widgets, _selection.ItemIds.ToList(), _selection.PrimaryItemId);
    }

    private Dictionary<Guid, Guid> BuildGroupKeys()
    {
        var result = new Dictionary<Guid, Guid>();
        var visited = new HashSet<Guid>();
        foreach (var placement in _layout.Placements)
        {
            if (!visited.Add(placement.ItemId))
            {
                continue;
            }

            var group = _selectionService.GetLinkedGroupItemIds(_layout, placement.ItemId);
            if (group.Count <= 1)
            {
                continue;
            }

            var key = group.OrderBy(x => x).First();
            foreach (var itemId in group)
            {
                visited.Add(itemId);
                result[itemId] = key;
            }
        }

        return result;
    }

    private bool ReorderSelected(Func<IReadOnlyList<LayoutItemPlacement>, IReadOnlyList<LayoutItemPlacement>, IReadOnlyDictionary<Guid, int>> builder)
    {
        if (_selection.ItemIds.Count == 0)
        {
            return false;
        }

        var all = _layout.Placements.OrderBy(x => x.ZIndex).ThenBy(x => x.ItemId).ToList();
        var selected = all.Where(x => _selection.ItemIds.Contains(x.ItemId)).ToList();
        if (selected.Count == 0)
        {
            return false;
        }

        var zMap = builder(all, selected);
        return _mutationService.SetZIndex(ref _layout, zMap);
    }

    private static IReadOnlyDictionary<Guid, int> BuildZMap(IReadOnlyList<LayoutItemPlacement> ordered)
    {
        var zMap = new Dictionary<Guid, int>(ordered.Count);
        for (var index = 0; index < ordered.Count; index++)
        {
            zMap[ordered[index].ItemId] = index;
        }

        return zMap;
    }

    private static LayoutDocument CreateDocumentFromWidgets(IReadOnlyCollection<LayoutEditorEngineWidgetInput> widgets, Size canvasSize)
    {
        var document = CreateEmptyDocument(canvasSize);
        var items = new List<LayoutItemInstance>();
        var placements = new List<LayoutItemPlacement>();
        var links = new List<LayoutItemLink>();

        foreach (var widget in widgets)
        {
            items.Add(new LayoutItemInstance(widget.Id, "dashboard.decorative-panel", widget.IsLocked));
            placements.Add(new LayoutItemPlacement(
                widget.Id,
                widget.X,
                widget.Y,
                widget.Width,
                widget.Height,
                widget.ZIndex));
        }

        foreach (var group in widgets.Where(x => x.LinkedGroupKey.HasValue).GroupBy(x => x.LinkedGroupKey!.Value))
        {
            var ordered = group.Select(x => x.Id).OrderBy(x => x).ToList();
            for (var index = 1; index < ordered.Count; index++)
            {
                links.Add(new LayoutItemLink(ordered[0], ordered[index], LayoutDockSide.None, LayoutDockSide.None, 0));
            }
        }

        return document with { Items = items, Placements = placements, Links = links };
    }

    private static LayoutDocument CreateEmptyDocument(Size canvasSize)
    {
        return new LayoutDocument(
            "1.0",
            "LayoutEditor",
            new LayoutCanvas(canvasSize.Width, canvasSize.Height),
            [],
            [],
            []);
    }
}
