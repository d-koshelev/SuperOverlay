using System.Text.Json;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.LayoutBuilder.Layout;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutMutationService
{
    private readonly DashboardRegistry _registry;

    public LayoutMutationService(DashboardRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    public LayoutDocument AddItem(
        LayoutDocument document,
        string typeId,
        double x,
        double y,
        double width,
        double height,
        int zIndex)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);

        var definition = _registry.Get(typeId);
        var defaultSettings = definition.CreateDefaultSettings();
        var serializedSettings = JsonSerializer.Serialize(defaultSettings, definition.SettingsType);

        var item = LayoutItemFactory.Create(typeId, serializedSettings);
        var placement = new LayoutItemPlacement(item.Id, x, y, width, height, zIndex);

        return document with
        {
            Items = document.Items.Concat(new[] { item }).ToList(),
            Placements = document.Placements.Concat(new[] { placement }).ToList()
        };
    }

    public LayoutDocument MoveItem(
        LayoutDocument document,
        Guid itemId,
        double deltaX,
        double deltaY)
    {
        ArgumentNullException.ThrowIfNull(document);

        var placements = document.Placements
            .Select(x => x.ItemId == itemId
                ? x with { X = x.X + deltaX, Y = x.Y + deltaY }
                : x)
            .ToList();

        return document with
        {
            Placements = placements
        };
    }
}
