using SuperOverlay.Dashboards.Registry;
using SuperOverlay.LayoutBuilder.Layout;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutMutationService
{
    private readonly DashboardRegistry _registry;
    private readonly LayoutDocumentEditor _editor = new();

    public LayoutMutationService(DashboardRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    public bool MoveItemsRuntimeDeltaBy(ref LayoutDocument document, IReadOnlyCollection<Guid> itemIds, double deltaX, double deltaY)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(itemIds);

        if (itemIds.Count == 0)
        {
            return false;
        }

        if (Math.Abs(deltaX) < double.Epsilon && Math.Abs(deltaY) < double.Epsilon)
        {
            return false;
        }

        var lockedIds = document.Items.Where(x => x.IsLocked).Select(x => x.Id).ToHashSet();
        var changed = false;
        foreach (var placement in document.Placements.Where(x => itemIds.Contains(x.ItemId) && !lockedIds.Contains(x.ItemId)).ToList())
        {
            var updatedPlacement = placement with
            {
                RuntimeDeltaX = placement.RuntimeDeltaX + deltaX,
                RuntimeDeltaY = placement.RuntimeDeltaY + deltaY,
                RuntimeX = null,
                RuntimeY = null,
                HasRuntimeOverride = false
            };

            document = _editor.UpdatePlacement(document, updatedPlacement);
            changed = true;
        }

        return changed;
    }

    public bool AddItem(ref LayoutDocument document, string typeId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);

        var definition = _registry.Get(typeId);
        var itemId = Guid.NewGuid();

        var item = new LayoutItemInstance(
            itemId,
            definition.TypeId,
            definition.CreateDefaultSettings(),
            false);

        var nextZ = document.Placements.Count == 0 ? 0 : document.Placements.Max(x => x.ZIndex) + 1;
        var (width, height) = typeId switch
        {
            "dashboard.shift-leds" => (280d, 42d),
            "dashboard.decorative-panel" => (260d, 18d),
            _ => (160d, 80d)
        };
        var placement = new LayoutItemPlacement(itemId, 40, 40, width, height, nextZ);

        document = _editor.AddItem(document, item, placement);
        return true;
    }

    public bool MoveItemsBy(ref LayoutDocument document, IReadOnlyCollection<Guid> itemIds, double deltaX, double deltaY)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(itemIds);

        if (itemIds.Count == 0)
        {
            return false;
        }

        var lockedIds = document.Items.Where(x => x.IsLocked).Select(x => x.Id).ToHashSet();
        var changed = false;
        foreach (var placement in document.Placements.Where(x => itemIds.Contains(x.ItemId) && !lockedIds.Contains(x.ItemId)).ToList())
        {
            var updatedPlacement = placement with
            {
                X = placement.X + deltaX,
                Y = placement.Y + deltaY
            };

            document = _editor.UpdatePlacement(document, updatedPlacement);
            changed = true;
        }

        return changed;
    }

    public bool MoveItemsByRuntime(ref LayoutDocument document, IReadOnlyCollection<Guid> itemIds, double deltaX, double deltaY)
    {
        return MoveItemsRuntimeDeltaBy(ref document, itemIds, deltaX, deltaY);
    }

    public bool ResizeItemTo(ref LayoutDocument document, Guid itemId, double width, double height)
    {
        ArgumentNullException.ThrowIfNull(document);

        var item = document.Items.FirstOrDefault(x => x.Id == itemId);
        var placement = document.Placements.FirstOrDefault(x => x.ItemId == itemId);
        if (placement is null || item is null || item.IsLocked)
        {
            return false;
        }

        var updatedPlacement = placement with
        {
            Width = Math.Max(40, width),
            Height = Math.Max(30, height)
        };

        document = _editor.UpdatePlacement(document, updatedPlacement);
        return true;
    }


    public bool UpdateItemProperties(ref LayoutDocument document, Guid itemId, double x, double y, double width, double height, int zIndex, bool isLocked)
    {
        ArgumentNullException.ThrowIfNull(document);

        var item = document.Items.FirstOrDefault(xItem => xItem.Id == itemId);
        var placement = document.Placements.FirstOrDefault(xPlacement => xPlacement.ItemId == itemId);
        if (item is null || placement is null)
        {
            return false;
        }

        var changed = false;
        var normalizedWidth = Math.Max(40, width);
        var normalizedHeight = Math.Max(30, height);
        var normalizedPlacement = placement with
        {
            X = x,
            Y = y,
            Width = normalizedWidth,
            Height = normalizedHeight
        };

        if (normalizedPlacement != placement)
        {
            document = _editor.UpdatePlacement(document, normalizedPlacement);
            changed = true;
        }

        var normalizedItem = item with { IsLocked = isLocked };
        if (normalizedItem != item)
        {
            document = _editor.UpdateItem(document, normalizedItem);
            changed = true;
        }

        var placementsOrdered = document.Placements.OrderBy(p => p.ZIndex).ThenBy(p => p.ItemId).ToList();
        var currentIndex = placementsOrdered.FindIndex(p => p.ItemId == itemId);
        var targetIndex = Math.Clamp(zIndex, 0, Math.Max(0, placementsOrdered.Count - 1));
        if (currentIndex >= 0 && currentIndex != targetIndex)
        {
            var moved = placementsOrdered[currentIndex];
            placementsOrdered.RemoveAt(currentIndex);
            placementsOrdered.Insert(targetIndex, moved);

            for (var i = 0; i < placementsOrdered.Count; i++)
            {
                var current = placementsOrdered[i];
                if (current.ZIndex == i)
                {
                    continue;
                }

                document = _editor.UpdatePlacement(document, current with { ZIndex = i });
            }

            changed = true;
        }

        return changed;
    }


    public bool UpdateItemSettings(ref LayoutDocument document, Guid itemId, object settings)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(settings);

        var item = document.Items.FirstOrDefault(x => x.Id == itemId);
        if (item is null)
        {
            return false;
        }

        var updatedItem = item with { Settings = settings };
        if (Equals(updatedItem, item))
        {
            return false;
        }

        document = _editor.UpdateItem(document, updatedItem);
        return true;
    }
    public bool DeleteItems(ref LayoutDocument document, IReadOnlyCollection<Guid> itemIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(itemIds);

        var changed = false;
        foreach (var itemId in itemIds.ToList())
        {
            if (!document.Items.Any(x => x.Id == itemId))
            {
                continue;
            }

            document = _editor.RemoveItem(document, itemId);
            changed = true;
        }

        return changed;
    }

    public bool DeleteItem(ref LayoutDocument document, Guid itemId) => DeleteItems(ref document, new[] { itemId });

    public bool DuplicateItems(ref LayoutDocument document, IReadOnlyCollection<Guid> itemIds, out IReadOnlyList<Guid> newItemIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(itemIds);

        var sourcePlacements = document.Placements
            .Where(x => itemIds.Contains(x.ItemId))
            .OrderBy(x => x.ZIndex)
            .ToList();

        if (sourcePlacements.Count == 0)
        {
            newItemIds = Array.Empty<Guid>();
            return false;
        }

        var sourceItemMap = document.Items.Where(x => itemIds.Contains(x.Id)).ToDictionary(x => x.Id);
        var nextZ = document.Placements.Count == 0 ? 0 : document.Placements.Max(x => x.ZIndex) + 1;
        var idMap = new Dictionary<Guid, Guid>();
        var changed = false;

        foreach (var placement in sourcePlacements)
        {
            if (!sourceItemMap.TryGetValue(placement.ItemId, out var sourceItem))
            {
                continue;
            }

            var newItemId = Guid.NewGuid();
            idMap[placement.ItemId] = newItemId;

            var duplicatedItem = sourceItem with { Id = newItemId };
            var duplicatedPlacement = placement with
            {
                ItemId = newItemId,
                X = placement.X + 24,
                Y = placement.Y + 24,
                ZIndex = nextZ++,
                RuntimeDeltaX = 0,
                RuntimeDeltaY = 0,
                RuntimeX = null,
                RuntimeY = null,
                HasRuntimeOverride = false
            };

            document = _editor.AddItem(document, duplicatedItem, duplicatedPlacement);
            changed = true;
        }

        if (changed)
        {
            foreach (var link in document.Links.ToList())
            {
                if (idMap.TryGetValue(link.SourceItemId, out var newSourceId) && idMap.TryGetValue(link.TargetItemId, out var newTargetId))
                {
                    document = _editor.AddLink(document, link with { SourceItemId = newSourceId, TargetItemId = newTargetId });
                }
            }
        }

        newItemIds = sourcePlacements.Where(x => idMap.ContainsKey(x.ItemId)).Select(x => idMap[x.ItemId]).ToList();
        return changed;
    }

    public bool DuplicateItem(ref LayoutDocument document, Guid itemId, out Guid newItemId)
    {
        var changed = DuplicateItems(ref document, new[] { itemId }, out var newItemIds);
        newItemId = newItemIds.FirstOrDefault();
        return changed && newItemId != Guid.Empty;
    }

    public bool PasteItemsFromLayout(ref LayoutDocument document, LayoutDocument sourceDocument, IReadOnlyCollection<Guid> itemIds, double offsetX, double offsetY, out IReadOnlyList<Guid> newItemIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(sourceDocument);
        ArgumentNullException.ThrowIfNull(itemIds);

        var sourcePlacements = sourceDocument.Placements
            .Where(x => itemIds.Contains(x.ItemId))
            .OrderBy(x => x.ZIndex)
            .ToList();

        if (sourcePlacements.Count == 0)
        {
            newItemIds = Array.Empty<Guid>();
            return false;
        }

        var sourceItemMap = sourceDocument.Items.Where(x => itemIds.Contains(x.Id)).ToDictionary(x => x.Id);
        var nextZ = document.Placements.Count == 0 ? 0 : document.Placements.Max(x => x.ZIndex) + 1;
        var idMap = new Dictionary<Guid, Guid>();

        foreach (var placement in sourcePlacements)
        {
            if (!sourceItemMap.TryGetValue(placement.ItemId, out var sourceItem))
            {
                continue;
            }

            var newItemId = Guid.NewGuid();
            idMap[placement.ItemId] = newItemId;
            var duplicatedItem = sourceItem with { Id = newItemId };
            var duplicatedPlacement = placement with
            {
                ItemId = newItemId,
                X = placement.X + offsetX,
                Y = placement.Y + offsetY,
                ZIndex = nextZ++,
                RuntimeDeltaX = 0,
                RuntimeDeltaY = 0,
                RuntimeX = null,
                RuntimeY = null,
                HasRuntimeOverride = false
            };

            document = _editor.AddItem(document, duplicatedItem, duplicatedPlacement);
        }

        foreach (var link in sourceDocument.Links)
        {
            if (idMap.TryGetValue(link.SourceItemId, out var newSourceId) && idMap.TryGetValue(link.TargetItemId, out var newTargetId))
            {
                document = _editor.AddLink(document, link with { SourceItemId = newSourceId, TargetItemId = newTargetId });
            }
        }

        newItemIds = sourcePlacements.Where(x => idMap.ContainsKey(x.ItemId)).Select(x => idMap[x.ItemId]).ToList();
        return newItemIds.Count > 0;
    }

    public bool GroupItems(ref LayoutDocument document, Guid anchorId, Guid targetId)
    {
        if (anchorId == targetId)
        {
            return false;
        }

        document = _editor.AddLink(document, new LayoutItemLink(anchorId, targetId, LayoutDockSide.None, LayoutDockSide.None, 0));
        return true;
    }

    public bool UngroupItem(ref LayoutDocument document, Guid itemId)
    {
        var before = document.Links.Count;
        document = _editor.RemoveLinksForItem(document, itemId);
        return document.Links.Count != before;
    }

    public bool ToggleLockItems(ref LayoutDocument document, IReadOnlyCollection<Guid> itemIds, bool isLocked)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(itemIds);

        var changed = false;
        foreach (var item in document.Items.Where(x => itemIds.Contains(x.Id)).ToList())
        {
            if (item.IsLocked == isLocked)
            {
                continue;
            }

            document = _editor.UpdateItem(document, item with { IsLocked = isLocked });
            changed = true;
        }

        return changed;
    }

    public bool SetZIndex(ref LayoutDocument document, IReadOnlyCollection<Guid> itemIds, Func<IReadOnlyList<LayoutItemPlacement>, IEnumerable<LayoutItemPlacement>, Dictionary<Guid, int>> reorder)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(reorder);

        var allPlacements = document.Placements.OrderBy(x => x.ZIndex).ToList();
        var selectedPlacements = allPlacements.Where(x => itemIds.Contains(x.ItemId)).ToList();
        if (selectedPlacements.Count == 0)
        {
            return false;
        }

        var newZById = reorder(allPlacements, selectedPlacements);
        if (newZById.Count == 0)
        {
            return false;
        }

        foreach (var placement in allPlacements)
        {
            if (newZById.TryGetValue(placement.ItemId, out var z))
            {
                document = _editor.UpdatePlacement(document, placement with { ZIndex = z });
            }
        }

        return true;
    }
}
