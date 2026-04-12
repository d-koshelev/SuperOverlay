using SuperOverlay.Dashboards.Registry;
using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.Runtime;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutRuntimeComposer
{
    private readonly DashboardRegistry _registry;
    private readonly OverlayShellMode _shellMode;

    public LayoutRuntimeComposer(DashboardRegistry registry, OverlayShellMode shellMode = OverlayShellMode.Editor)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
        _shellMode = shellMode;
    }

    public IReadOnlyList<RuntimeLayoutItem> Compose(LayoutDocument layout)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var result = new List<RuntimeLayoutItem>(layout.Items.Count);
        var placementIndex = new LayoutPlacementIndex(layout.Placements);

        foreach (var item in layout.Items)
        {
            var placement = LayoutPlacementResolver.ResolveForShell(placementIndex.GetRequired(item.Id), layout.Canvas, _shellMode);
            var definition = _registry.Get(item.TypeId);
            var presenter = definition.CreatePresenter();
            var settings = definition.MaterializeSettings(item.Settings);

            var runtimeItem = new RuntimeLayoutItem(
                item with { Settings = settings },
                placement,
                presenter,
                _shellMode);

            result.Add(runtimeItem);
        }

        return result;
    }
}
