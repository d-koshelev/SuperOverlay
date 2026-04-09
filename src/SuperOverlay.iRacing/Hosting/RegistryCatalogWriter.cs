using SuperOverlay.Dashboards.Registry;

namespace SuperOverlay.iRacing.Hosting;

public static class RegistryCatalogWriter
{
    public static string BuildDebugText(DashboardRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var lines = registry.GetCatalog()
            .Select(x => $"{x.DisplayName} ({x.TypeId})")
            .ToArray();

        return string.Join(Environment.NewLine, lines);
    }
}
