namespace SuperOverlay.Dashboards.Items.Gear;

public sealed record GearDashboardSettings(
    bool ShowNeutralAsN = true,
    double CornerTopLeft = 0,
    double CornerTopRight = 0,
    double CornerBottomRight = 0,
    double CornerBottomLeft = 0);
