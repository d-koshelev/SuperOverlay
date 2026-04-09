using System.IO;
using System.Windows.Controls;
using SuperOverlay.LayoutBuilder.Persistence;
using SuperOverlay.LayoutBuilder.Runtime;

namespace SuperOverlay.iRacing.Hosting;

public sealed class OverlayRuntimeBootstrapper
{
    public OverlayRuntimeSession Build(Grid root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var registry = DashboardRegistryFactory.Create();

        var layoutPath = Path.Combine(
            AppContext.BaseDirectory,
            "Layouts",
            "default-layout.json");

        var layoutProvider = new LayoutDocumentProvider();
        var layout = layoutProvider.GetOrCreateDefault(layoutPath);

        var composer = new LayoutRuntimeComposer(registry);
        var mutationService = new LayoutMutationService(registry);
        var fileStore = new LayoutFileStore();
        var layoutHost = new LayoutHost(root);

        return new OverlayRuntimeSession(
            layoutHost,
            registry,
            composer,
            fileStore,
            mutationService,
            layoutPath,
            layout);
    }
}
