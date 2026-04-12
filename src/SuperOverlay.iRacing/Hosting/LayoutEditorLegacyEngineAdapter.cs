using System.Windows;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.LayoutBuilder.Layout;
using SuperOverlay.LayoutBuilder.Runtime;
using SuperOverlay.LayoutEditor;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutEditorLegacyEngineAdapter : ILayoutEditorInteractionEngine
{
    private readonly DashboardRegistry _registry;
    private readonly LayoutMutationService _mutationService;
    private readonly OverlaySelectionService _selectionService;
    private readonly LayoutSnapService _snapService;
    private readonly OverlayMovementService _movementService;
    private readonly OverlaySelectionState _selection = new();
    private LayoutDocument _layout;

    public LayoutEditorLegacyEngineAdapter()
    {
        _registry = DashboardRegistryFactory.Create();
        _mutationService = new LayoutMutationService(_registry);
        _selectionService = new OverlaySelectionService();
        _snapService = new LayoutSnapService();
        _movementService = new OverlayMovementService(_mutationService, _snapService, _selectionService);
        _layout = CreateEmptyDocument(new System.Windows.Size(1920, 1080));
    }

    public void SyncWidgets(IReadOnlyCollection<LayoutEditorWidget> widgets, System.Windows.Size canvasSize)
    {
        _layout = CreateDocumentFromWidgets(widgets, canvasSize);
    }

    public void SyncSelection(IReadOnlyCollection<Guid> selectedIds, Guid? primaryId)
    {
        var normalized = _selectionService.NormalizeSelection(_layout, selectedIds, primaryId, null, EmptyCompiledPanels.Instance);
        _selection.Replace(normalized.ItemIds, normalized.PrimaryItemId);
    }

    public LayoutEditorEngineMoveResult MoveSelection(double deltaX, double deltaY, System.Windows.Size canvasSize, bool bypassSnap = false)
    {
        var result = _movementService.MoveSelectedWithSnap(
            ref _layout,
            _selection,
            null,
            EmptyCompiledPanels.Instance,
            deltaX,
            deltaY,
            canvasSize.Width,
            canvasSize.Height,
            true,
            bypassSnap,
            OverlayShellMode.Editor);

        return new LayoutEditorEngineMoveResult(result.Moved, result.SnapX, result.SnapY);
    }


    public LayoutEditorEngineMoveResult ResizeSelection(double deltaX, double deltaY, System.Windows.Size canvasSize, bool bypassSnap = false)
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
        var normalized = _selectionService.NormalizeSelection(_layout, selectedIds, primaryId, null, EmptyCompiledPanels.Instance);
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

            var component = _selectionService.GetLinkedGroupItemIds(_layout, placement.ItemId);
            foreach (var id in component)
            {
                visited.Add(id);
            }

            if (component.Count <= 1)
            {
                continue;
            }

            var groupKey = component.Order().First();
            foreach (var id in component)
            {
                result[id] = groupKey;
            }
        }

        return result;
    }

    private bool ReorderSelected(Func<IReadOnlyList<LayoutItemPlacement>, IEnumerable<LayoutItemPlacement>, Dictionary<Guid, int>> reorder)
    {
        if (_selection.ItemIds.Count == 0)
        {
            return false;
        }

        return _mutationService.SetZIndex(ref _layout, _selection.ItemIds, reorder);
    }

    private static Dictionary<Guid, int> BuildZMap(IReadOnlyList<LayoutItemPlacement> ordered)
    {
        var result = new Dictionary<Guid, int>(ordered.Count);
        for (var i = 0; i < ordered.Count; i++)
        {
            result[ordered[i].ItemId] = i;
        }

        return result;
    }

    private LayoutDocument CreateDocumentFromWidgets(IReadOnlyCollection<LayoutEditorWidget> widgets, System.Windows.Size canvasSize)
    {
        var canvas = new LayoutCanvas(
            Math.Max(320, Math.Round(canvasSize.Width)),
            Math.Max(180, Math.Round(canvasSize.Height)));

        var definition = _registry.Get("dashboard.decorative-panel");
        var items = widgets
            .Select(w => new LayoutItemInstance(w.Id, definition.TypeId, definition.CreateDefaultSettings(), false))
            .ToList();

        var placements = widgets
            .OrderBy(w => w.ZIndex)
            .ThenBy(w => w.Id)
            .Select(w => new LayoutItemPlacement(w.Id, w.X, w.Y, w.Width, w.Height, w.ZIndex))
            .ToList();

        var links = widgets
            .Where(w => w.IsGrouped && w.GroupId is not null)
            .GroupBy(w => w.GroupId!.Value)
            .SelectMany(g => BuildLinks(g.Select(x => x.Id).ToList()))
            .ToList();

        return new LayoutDocument("1.0", "LayoutEditor Mirror", canvas, items, placements, links);
    }

    private static IEnumerable<LayoutItemLink> BuildLinks(IReadOnlyList<Guid> ids)
    {
        for (var i = 1; i < ids.Count; i++)
        {
            yield return new LayoutItemLink(ids[i - 1], ids[i], LayoutDockSide.None, LayoutDockSide.None, 0);
        }
    }

    private static LayoutDocument CreateEmptyDocument(System.Windows.Size canvasSize)
    {
        return new LayoutDocument(
            "1.0",
            "LayoutEditor Mirror",
            new LayoutCanvas(Math.Max(320, Math.Round(canvasSize.Width)), Math.Max(180, Math.Round(canvasSize.Height))),
            Array.Empty<LayoutItemInstance>(),
            Array.Empty<LayoutItemPlacement>(),
            Array.Empty<LayoutItemLink>());
    }

    private sealed class EmptyCompiledPanels : Dictionary<Guid, IReadOnlyList<Guid>>
    {
        public static EmptyCompiledPanels Instance { get; } = new();
    }
}
