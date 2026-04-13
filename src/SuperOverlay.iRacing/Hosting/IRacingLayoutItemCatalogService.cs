using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Dashboards.Registry;

namespace SuperOverlay.iRacing.Hosting;

public sealed class IRacingLayoutItemCatalogService
{
    private readonly DashboardRegistry _registry;
    private readonly LayoutDocumentEditor _editor = new();

    public IRacingLayoutItemCatalogService(DashboardRegistry registry)
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
            definition.CreateDefaultSettings(),
            false);

        var nextZ = document.Placements.Count == 0 ? 0 : document.Placements.Max(x => x.ZIndex) + 1;
        var (width, height) = typeId switch
        {
            "dashboard.shift-leds" => (280d, 42d),
            "dashboard.raw-value" => (160d, 80d),
            "dashboard.decorative-panel" => (260d, 18d),
            _ => (160d, 80d)
        };
        var placement = new LayoutItemPlacement(itemId, 40, 40, width, height, nextZ);

        document = _editor.AddItem(document, item, placement);
        return true;
    }
}
