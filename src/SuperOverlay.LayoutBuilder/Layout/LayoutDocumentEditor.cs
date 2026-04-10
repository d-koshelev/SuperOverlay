namespace SuperOverlay.LayoutBuilder.Layout;

public sealed class LayoutDocumentEditor
{
    public LayoutDocument UpdateCanvas(LayoutDocument document, LayoutCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(canvas);

        return document with { Canvas = canvas };
    }

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

    public LayoutDocument UpdateItem(LayoutDocument document, LayoutItemInstance item)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(item);

        return document with
        {
            Items = document.Items.Select(x => x.Id == item.Id ? item : x).ToList()
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

    public LayoutDocument AddLink(LayoutDocument document, LayoutItemLink link)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(link);

        var exists = document.Links.Any(x =>
            (x.SourceItemId == link.SourceItemId && x.TargetItemId == link.TargetItemId) ||
            (x.SourceItemId == link.TargetItemId && x.TargetItemId == link.SourceItemId));

        if (exists)
        {
            return document;
        }

        return document with
        {
            Links = document.Links.Concat(new[] { link }).ToList()
        };
    }

    public LayoutDocument RemoveLinksForItem(LayoutDocument document, Guid itemId)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document with
        {
            Links = document.Links
                .Where(x => x.SourceItemId != itemId && x.TargetItemId != itemId)
                .ToList()
        };
    }

    public LayoutDocument RemoveLink(LayoutDocument document, Guid firstItemId, Guid secondItemId)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document with
        {
            Links = document.Links
                .Where(x => !((x.SourceItemId == firstItemId && x.TargetItemId == secondItemId) ||
                              (x.SourceItemId == secondItemId && x.TargetItemId == firstItemId)))
                .ToList()
        };
    }
}
