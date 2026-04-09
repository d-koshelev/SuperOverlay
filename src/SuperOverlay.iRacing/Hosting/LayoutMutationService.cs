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

        var updatedPlacement = placement with
        {
            X = placement.X + deltaX,
            Y = placement.Y + deltaY
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
}