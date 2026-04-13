using System.Windows;
using SuperOverlay.Dashboards.Runtime;

namespace SuperOverlay.Dashboards.Items.RawValue;

public sealed record RawValueDashboardSettings
{
    public DashboardFieldBinding ValueBinding { get; init; } = DashboardFieldBinding.Telemetry("Speed");
    public double FontSize { get; init; } = 32;
    public TextAlignment TextAlignment { get; init; } = TextAlignment.Left;
    public bool AutoSizeToContent { get; init; } = false;
    public double CornerTopLeft { get; init; } = 0;
    public double CornerTopRight { get; init; } = 0;
    public double CornerBottomRight { get; init; } = 0;
    public double CornerBottomLeft { get; init; } = 0;
}
