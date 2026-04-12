using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.PanelLayouts;

namespace SuperOverlay.Core.Layouts.Editing;

public sealed class OverlaySelectionService
{
    public (HashSet<Guid> ItemIds, Guid? PrimaryItemId) NormalizeSelection(
        LayoutDocument layout,
        IEnumerable<Guid> itemIds,
        Guid? primaryItemId,
        PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        var requested = itemIds.Distinct().ToList();
        if (requested.Count == 0)
        {
            return (new HashSet<Guid>(), null);
        }

        var groupedComponents = requested
            .Select(id => GetLinkedGroupItemIds(layout, id))
            .Where(ids => ids.Count > 1)
            .Distinct(LinkedGroupComparer.Instance)
            .ToList();

        if (groupedComponents.Count > 1)
        {
            var preferredGroup = primaryItemId is not null
                ? GetLinkedGroupItemIds(layout, primaryItemId.Value)
                : groupedComponents[0];

            return (
                preferredGroup.ToHashSet(),
                primaryItemId is not null && preferredGroup.Contains(primaryItemId.Value)
                    ? primaryItemId.Value
                    : preferredGroup.First());
        }

        var normalizedIds = new HashSet<Guid>();
        foreach (var id in requested)
        {
            foreach (var expandedId in GetSelectionUnitItemIds(layout, id, panelLayout, compiledPanelItemMap))
            {
                normalizedIds.Add(expandedId);
            }
        }

        Guid? resolvedPrimary = null;
        if (primaryItemId is not null && normalizedIds.Contains(primaryItemId.Value))
        {
            resolvedPrimary = primaryItemId.Value;
        }
        else if (normalizedIds.Count > 0)
        {
            resolvedPrimary = normalizedIds.First();
        }

        return (normalizedIds, resolvedPrimary);
    }

    public IReadOnlyCollection<Guid> GetSelectionUnitItemIds(
        LayoutDocument layout,
        Guid itemId,
        PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        var panelItems = GetCompiledPanelItemIds(itemId, panelLayout, compiledPanelItemMap);
        if (panelItems.Count > 0)
        {
            return panelItems;
        }

        var linked = GetLinkedGroupItemIds(layout, itemId);
        return linked.Count > 1 ? linked : new[] { itemId };
    }

    public HashSet<Guid> GetSelectedGroupedItemIds(LayoutDocument layout, OverlaySelectionState selection)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(selection);

        return selection.ItemIds
            .Select(id => GetLinkedGroupItemIds(layout, id))
            .Where(ids => ids.Count > 1)
            .SelectMany(ids => ids)
            .ToHashSet();
    }

    public IReadOnlyList<Guid> GetActiveMoveItemIds(
        LayoutDocument layout,
        OverlaySelectionState selection,
        PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        if (selection.ItemIds.Count > 1)
        {
            return selection.ItemIds.ToList();
        }

        return selection.PrimaryItemId is null
            ? Array.Empty<Guid>()
            : GetLinkedGroupItemIds(layout, selection.PrimaryItemId.Value);
    }

    public IReadOnlyList<Guid> GetHighlightedGroupItemIds(LayoutDocument layout, Guid? itemId)
    {
        ArgumentNullException.ThrowIfNull(layout);

        if (itemId is null)
        {
            return Array.Empty<Guid>();
        }

        var groupIds = GetLinkedGroupItemIds(layout, itemId.Value);
        return groupIds.Count <= 1 ? Array.Empty<Guid>() : groupIds;
    }

    public IReadOnlyList<Guid> GetLinkedGroupItemIds(LayoutDocument layout, Guid anchorItemId)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var visited = new HashSet<Guid> { anchorItemId };
        var queue = new Queue<Guid>();
        queue.Enqueue(anchorItemId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in layout.Links
                         .Where(x => x.SourceItemId == current || x.TargetItemId == current)
                         .Select(x => x.SourceItemId == current ? x.TargetItemId : x.SourceItemId))
            {
                if (visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return visited.ToList();
    }

    public IReadOnlyList<Guid> GetCompiledPanelItemIds(
        Guid itemId,
        PanelLayoutDocument? panelLayout,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> compiledPanelItemMap)
    {
        ArgumentNullException.ThrowIfNull(compiledPanelItemMap);

        if (panelLayout is null)
        {
            return Array.Empty<Guid>();
        }

        foreach (var entry in compiledPanelItemMap)
        {
            if (entry.Value.Contains(itemId))
            {
                return entry.Value;
            }
        }

        return Array.Empty<Guid>();
    }

    private sealed class LinkedGroupComparer : IEqualityComparer<IReadOnlyCollection<Guid>>
    {
        public static LinkedGroupComparer Instance { get; } = new();

        public bool Equals(IReadOnlyCollection<Guid>? x, IReadOnlyCollection<Guid>? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null || x.Count != y.Count)
            {
                return false;
            }

            return x.ToHashSet().SetEquals(y);
        }

        public int GetHashCode(IReadOnlyCollection<Guid> obj)
        {
            var hash = 17;
            foreach (var id in obj.Order())
            {
                hash = (hash * 31) + id.GetHashCode();
            }

            return hash;
        }
    }
}
