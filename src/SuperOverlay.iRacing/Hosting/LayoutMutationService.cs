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

    public bool AddItem(ref LayoutDocument document, string typeId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);

        var definition = _registry.Get(typeId);
        var itemId = Guid.NewGuid();

        var item = new LayoutItemInstance(
            itemId,
            definition.TypeId,
            definition.CreateDefaultSettings());

        var placement = new LayoutItemPlacement(
            itemId,
            40,
            40,
            160,
            80,
            10);

        document = _editor.AddItem(document, item, placement);
        return true;
    }

    public bool MoveItem(ref LayoutDocument document, Guid itemId, double deltaX, double deltaY)
    {
        ArgumentNullException.ThrowIfNull(document);

        var placement = document.Placements.FirstOrDefault(x => x.ItemId == itemId);
        if (placement is null)
        {
            return false;
        }

        return MoveItemTo(ref document, itemId, placement.X + deltaX, placement.Y + deltaY);
    }

    public bool MoveItemTo(ref LayoutDocument document, Guid itemId, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(document);

        var placement = document.Placements.FirstOrDefault(xp => xp.ItemId == itemId);
        if (placement is null)
        {
            return false;
        }

        var updatedPlacement = placement with
        {
            X = x,
            Y = y
        };

        document = _editor.UpdatePlacement(document, updatedPlacement);
        return true;
    }

    public bool ResizeItem(ref LayoutDocument document, Guid itemId, double deltaWidth, double deltaHeight)
    {
        ArgumentNullException.ThrowIfNull(document);

        var placement = document.Placements.FirstOrDefault(x => x.ItemId == itemId);
        if (placement is null)
        {
            return false;
        }

        var updatedPlacement = placement with
        {
            Width = Math.Max(40, placement.Width + deltaWidth),
            Height = Math.Max(30, placement.Height + deltaHeight)
        };

        document = _editor.UpdatePlacement(document, updatedPlacement);
        return true;
    }

    public bool DeleteItem(ref LayoutDocument document, Guid itemId)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Items.Any(x => x.Id == itemId))
        {
            return false;
        }

        document = _editor.RemoveItem(document, itemId);
        return true;
    }

    public bool DuplicateItem(ref LayoutDocument document, Guid itemId, out Guid newItemId)
    {
        ArgumentNullException.ThrowIfNull(document);

        var sourceItem = document.Items.FirstOrDefault(x => x.Id == itemId);
        var sourcePlacement = document.Placements.FirstOrDefault(x => x.ItemId == itemId);

        if (sourceItem is null || sourcePlacement is null)
        {
            newItemId = Guid.Empty;
            return false;
        }

        newItemId = Guid.NewGuid();

        var duplicatedItem = sourceItem with { Id = newItemId };
        var duplicatedPlacement = sourcePlacement with
        {
            ItemId = newItemId,
            X = sourcePlacement.X + 20,
            Y = sourcePlacement.Y + 20,
            ZIndex = sourcePlacement.ZIndex + 1
        };

        document = _editor.AddItem(document, duplicatedItem, duplicatedPlacement);
        return true;
    }
}
