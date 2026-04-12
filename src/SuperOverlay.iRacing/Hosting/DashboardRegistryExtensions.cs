using SuperOverlay.Dashboards.Contracts;
using SuperOverlay.Dashboards.Registry;

namespace SuperOverlay.iRacing.Hosting;

internal static class DashboardRegistryExtensions
{
    public static TSettings GetItemSettings<TSettings>(this DashboardRegistry registry, string typeId, object? rawSettings)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);

        // Validate that the item exists in the registry; callers rely on registry-backed type ids.
        _ = registry.Get(typeId);
        return DashboardSettingsMaterializer.Materialize<TSettings>(rawSettings);
    }
}
