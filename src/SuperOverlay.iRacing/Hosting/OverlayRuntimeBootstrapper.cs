using System.IO;
using System.Windows.Controls;
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
        var runtimeItems = composer.Compose(layout);

        var layoutHost = new LayoutHost(root);
        layoutHost.Load(runtimeItems);

        return new OverlayRuntimeSession(layoutHost);
    }
}
