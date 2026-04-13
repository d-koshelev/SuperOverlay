using SuperOverlay.Dashboards.Runtime;

namespace SuperOverlay.Dashboards.Items.ShiftLeds;

public sealed record ShiftLedDashboardSettings(
    int LedCount = 12,
    double StartPercent = 0.55,
    double BlinkPercent = 0.985,
    bool ShowBackground = true,
    string BackgroundColor = "#E61F2937",
    string LedOffColor = "#0F0F0F",
    string LedOnColor = "#FF1E00",
    bool UsePerLedColors = true,
    string LedOnColors = "#006C8E,#0097C9,#00C2FF,#00FF9C,#B4FF00,#FFD400,#FFB000,#FF7A00,#FF4E00,#FF3C00,#FF2A00,#FF1E00",
    DashboardFieldBinding? valueBinding = null,
    double CornerTopLeft = 0,
    double CornerTopRight = 0,
    double CornerBottomRight = 0,
    double CornerBottomLeft = 0)
{
    public DashboardFieldBinding ValueBinding { get; init; } = valueBinding ?? DashboardFieldBinding.Telemetry("ShiftIndicatorPct");
}
