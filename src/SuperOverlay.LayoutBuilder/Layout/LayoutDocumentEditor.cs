namespace SuperOverlay.LayoutBuilder.Layout;

public sealed class LayoutDocumentEditor
{
    public LayoutDocument AddItem(
        LayoutDocument document,
        LayoutItemInstance item,
        LayoutItemPlacement placement)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(placement);

        if (placement.ItemId != item.Id)
        {
            throw new InvalidOperationException("Placement.ItemId must match item.Id.");
        }

        return document with
        {
            Items = document.Items.Concat(new[] { item }).ToList(),
            Placements = document.Placements.Concat(new[] { placement }).ToList()
        };
    }

    public LayoutDocument RemoveItem(LayoutDocument document, Guid itemId)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document with
        {
            Items = document.Items.Where(x => x.Id != itemId).ToList(),
            Placements = document.Placements.Where(x => x.ItemId != itemId).ToList(),
            Links = document.Links
                .Where(x => x.SourceItemId != itemId && x.TargetItemId != itemId)
                .ToList()
        };
    }

    public LayoutDocument UpdatePlacement(LayoutDocument document, LayoutItemPlacement placement)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(placement);

        var placements = document.Placements
            .Select(x => x.ItemId == placement.ItemId ? placement : x)
            .ToList();

        return document with { Placements = placements };
    }
}
