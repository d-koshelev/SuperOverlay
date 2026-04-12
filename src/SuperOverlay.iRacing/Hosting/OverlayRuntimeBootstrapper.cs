using System.IO;
using System.Windows.Controls;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.Core.Layouts.Persistence;
using SuperOverlay.Core.Layouts.Runtime;

using SuperOverlay.Core.Layouts.Editing;
namespace SuperOverlay.iRacing.Hosting;

public sealed class OverlayRuntimeBootstrapper
{
    public OverlayRuntimeSession Build(Grid root, OverlayShellMode shellMode = OverlayShellMode.Editor)
    {
        ArgumentNullException.ThrowIfNull(root);

        var registry = DashboardRegistryFactory.Create();

        var layoutPath = Path.Combine(
            AppContext.BaseDirectory,
            "Layouts",
            "default-layout.json");

        var layoutProvider = new LayoutDocumentProvider();
        var layout = layoutProvider.GetOrCreateDefault(layoutPath);

        var composer = new LayoutRuntimeComposer(registry, shellMode);
        var fileStore = new LayoutFileStore();
        var mutationService = new LayoutMutationCore();
        var itemCatalogService = new IRacingLayoutItemCatalogService(registry);

        var layoutHost = new LayoutHost(root, shellMode);

        return new OverlayRuntimeSession(
            layoutHost,
            registry,
            composer,
            fileStore,
            mutationService,
            itemCatalogService,
            layoutPath,
            layout,
            shellMode);
    }
}