namespace SuperOverlay.Dashboards.Items.Speed;

public sealed record SpeedDashboardSettings(
    bool ShowUnit = true,
    string UnitText = "km/h");