using SuperOverlay.Dashboards.Registry;
using SuperOverlay.LayoutBuilder.Layout;
using SuperOverlay.LayoutBuilder.Runtime;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutRuntimeComposer
{
    private readonly DashboardRegistry _registry;

    public LayoutRuntimeComposer(DashboardRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    public IReadOnlyList<RuntimeLayoutItem> Compose(LayoutDocument layout)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var result = new List<RuntimeLayoutItem>(layout.Items.Count);
        var placementIndex = new LayoutPlacementIndex(layout.Placements);

        foreach (var item in layout.Items)
        {
            var placement = placementIndex.GetRequired(item.Id);
            var definition = _registry.Get(item.TypeId);
            var presenter = definition.CreatePresenter();
            var settings = definition.MaterializeSettings(item.Settings);

            var runtimeItem = new RuntimeLayoutItem(
                item with { Settings = settings },
                placement,
                presenter);

            result.Add(runtimeItem);
        }

        return result;
    }
}
