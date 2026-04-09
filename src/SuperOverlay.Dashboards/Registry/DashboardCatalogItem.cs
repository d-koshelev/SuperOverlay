namespace SuperOverlay.Dashboards.Registry;

public sealed record DashboardCatalogItem(
    string TypeId,
    string DisplayName,
    Type SettingsType);
