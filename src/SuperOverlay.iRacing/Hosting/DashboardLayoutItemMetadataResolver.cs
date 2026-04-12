using SuperOverlay.Core.Layouts.Editor;
using SuperOverlay.Dashboards.Registry;

namespace SuperOverlay.iRacing.Hosting;

public sealed class DashboardLayoutItemMetadataResolver : ILayoutItemMetadataResolver
{
    private readonly DashboardRegistry _registry;

    public DashboardLayoutItemMetadataResolver(DashboardRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public string GetDisplayName(string typeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);
        return _registry.Get(typeId).DisplayName;
    }
}
