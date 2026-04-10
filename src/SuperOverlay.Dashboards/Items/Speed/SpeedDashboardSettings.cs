namespace SuperOverlay.Dashboards.Items.Speed;

public sealed record SpeedDashboardSettings(
    bool ShowUnit = true,
    string UnitText = "km/h",
    double CornerTopLeft = 0,
    double CornerTopRight = 0,
    double CornerBottomRight = 0,
    double CornerBottomLeft = 0);
