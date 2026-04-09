using System.Text.Json;
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

        return _editor.AddItem(document, item, placement);
    }

    public LayoutDocument MoveItem(
        LayoutDocument document,
        Guid itemId,
        double deltaX,
        double deltaY)
    {
        ArgumentNullException.ThrowIfNull(document);

        var existingPlacement = document.Placements.FirstOrDefault(x => x.ItemId == itemId)
            ?? throw new InvalidOperationException($"Placement for item '{itemId}' was not found.");

        var updatedPlacement = existingPlacement with
        {
            X = existingPlacement.X + deltaX,
            Y = existingPlacement.Y + deltaY
        };

        return _editor.UpdatePlacement(document, updatedPlacement);
    }

    public LayoutDocument ResizeItem(
        LayoutDocument document,
        Guid itemId,
        double deltaWidth,
        double deltaHeight)
    {
        ArgumentNullException.ThrowIfNull(document);

        var existingPlacement = document.Placements.FirstOrDefault(x => x.ItemId == itemId)
            ?? throw new InvalidOperationException($"Placement for item '{itemId}' was not found.");

        var updatedPlacement = existingPlacement with
        {
            Width = Math.Max(40, existingPlacement.Width + deltaWidth),
            Height = Math.Max(24, existingPlacement.Height + deltaHeight)
        };

        return _editor.UpdatePlacement(document, updatedPlacement);
    }

    public LayoutDocument DeleteItem(LayoutDocument document, Guid itemId)
    {
        ArgumentNullException.ThrowIfNull(document);
        return _editor.RemoveItem(document, itemId);
    }
}
