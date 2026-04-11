using SuperOverlay.Dashboards.Registry;

namespace SuperOverlay.iRacing.Hosting;

public static class DashboardRegistryExtensions
{
    public static T? GetItemSettings<T>(this DashboardRegistry registry, string typeId, object rawSettings)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);

        return registry.Get(typeId).MaterializeSettings(rawSettings) as T;
    }
}
