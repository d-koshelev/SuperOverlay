namespace SuperOverlay.Dashboards.Runtime;

public sealed record DashboardFieldBinding(
    DashboardFieldSource Source,
    string FieldPath)
{
    public static DashboardFieldBinding Telemetry(string fieldPath)
        => new(DashboardFieldSource.TelemetryRaw, fieldPath);

    public static DashboardFieldBinding SessionInfo(string fieldPath)
        => new(DashboardFieldSource.SessionInfo, fieldPath);
}
