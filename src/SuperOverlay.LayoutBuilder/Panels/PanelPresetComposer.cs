namespace SuperOverlay.LayoutBuilder.Panels;

using SuperOverlay.LayoutBuilder.Layout;

public sealed class PanelPresetComposer
{
    private readonly LayoutDocumentEditor _editor = new();

    public PanelPresetDocument CreateFromLayoutSelection(
        LayoutDocument sourceDocument,
        IReadOnlyCollection<Guid> itemIds,
        string name,
        string category = "Custom")
    {
        ArgumentNullException.ThrowIfNull(sourceDocument);
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var selectedPlacements = sourceDocument.Placements
            .Where(x => itemIds.Contains(x.ItemId))
            .OrderBy(x => x.ZIndex)
            .ToList();

        if (selectedPlacements.Count == 0)
        {
            throw new InvalidOperationException("Cannot create a panel preset from an empty selection.");
        }

        var selectedItemMap = sourceDocument.Items
            .Where(x => itemIds.Contains(x.Id))
            .ToDictionary(x => x.Id);

        var minX = selectedPlacements.Min(x => x.X);
        var minY = selectedPlacements.Min(x => x.Y);
        var maxX = selectedPlacements.Max(x => x.X + x.Width);
        var maxY = selectedPlacements.Max(x => x.Y + x.Height);

        var placements = selectedPlacements
            .Select((placement, index) => placement with
            {
                X = placement.X - minX,
                Y = placement.Y - minY,
                ZIndex = index,
                RuntimeDeltaX = 0,
                RuntimeDeltaY = 0,
                RuntimeX = null,
                RuntimeY = null,
                HasRuntimeOverride = false
            })
            .ToList();

        var items = selectedPlacements
            .Select(x => selectedItemMap[x.ItemId])
            .ToList();

        var links = sourceDocument.Links
            .Where(x => itemIds.Contains(x.SourceItemId) && itemIds.Contains(x.TargetItemId))
            .ToList();

        return new PanelPresetDocument(
            Version: "1.0",
            Metadata: new PanelPresetMetadata(
                Guid.NewGuid(),
                name.Trim(),
                string.IsNullOrWhiteSpace(category) ? "Custom" : category.Trim(),
                maxX - minX,
                maxY - minY),
            Items: items,
            Placements: placements,
            Links: links);
    }

    public LayoutDocument InsertIntoLayout(
        LayoutDocument targetDocument,
        PanelPresetDocument preset,
        double x,
        double y,
        out IReadOnlyList<Guid> insertedItemIds)
    {
        ArgumentNullException.ThrowIfNull(targetDocument);
        ArgumentNullException.ThrowIfNull(preset);

        var sourcePlacements = preset.Placements
            .OrderBy(xPlacement => xPlacement.ZIndex)
            .ToList();

        if (sourcePlacements.Count == 0)
        {
            insertedItemIds = Array.Empty<Guid>();
            return targetDocument;
        }

        var sourceItemMap = preset.Items.ToDictionary(xItem => xItem.Id);
        var nextZ = targetDocument.Placements.Count == 0 ? 0 : targetDocument.Placements.Max(xPlacement => xPlacement.ZIndex) + 1;
        var idMap = new Dictionary<Guid, Guid>();
        var document = targetDocument;

        foreach (var placement in sourcePlacements)
        {
            if (!sourceItemMap.TryGetValue(placement.ItemId, out var sourceItem))
            {
                continue;
            }

            var newItemId = Guid.NewGuid();
            idMap[placement.ItemId] = newItemId;

            var insertedItem = sourceItem with { Id = newItemId };
            var insertedPlacement = placement with
            {
                ItemId = newItemId,
                X = x + placement.X,
                Y = y + placement.Y,
                ZIndex = nextZ++,
                RuntimeDeltaX = 0,
                RuntimeDeltaY = 0,
                RuntimeX = null,
                RuntimeY = null,
                HasRuntimeOverride = false
            };

            document = _editor.AddItem(document, insertedItem, insertedPlacement);
        }

        foreach (var link in preset.Links)
        {
            if (idMap.TryGetValue(link.SourceItemId, out var newSourceId) && idMap.TryGetValue(link.TargetItemId, out var newTargetId))
            {
                document = _editor.AddLink(document, link with { SourceItemId = newSourceId, TargetItemId = newTargetId });
            }
        }

        insertedItemIds = sourcePlacements
            .Where(xPlacement => idMap.ContainsKey(xPlacement.ItemId))
            .Select(xPlacement => idMap[xPlacement.ItemId])
            .ToList();

        return document;
    }
}
