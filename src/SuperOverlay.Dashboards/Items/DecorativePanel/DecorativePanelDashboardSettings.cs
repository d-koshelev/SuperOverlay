namespace SuperOverlay.Dashboards.Items.DecorativePanel;

public sealed record DecorativePanelDashboardSettings(
    string BackgroundColor = "#CC1F2937",
    double CornerTopLeft = 0,
    double CornerTopRight = 0,
    double CornerBottomRight = 0,
    double CornerBottomLeft = 0);
