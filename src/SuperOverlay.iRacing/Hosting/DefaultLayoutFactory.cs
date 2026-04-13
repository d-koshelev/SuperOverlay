using System.Windows;
using System.Text.Json;
using SuperOverlay.Dashboards.Items.RawValue;
using SuperOverlay.Core.Layouts.Layout;

namespace SuperOverlay.iRacing.Hosting;

public static class DefaultLayoutFactory
{
    public static LayoutDocument Create()
    {
        var speedId = Guid.NewGuid();
        var gearId = Guid.NewGuid();
        var trackId = Guid.NewGuid();

        var items = new List<LayoutItemInstance>
        {
            new(
                speedId,
                "dashboard.raw-value",
                JsonSerializer.Serialize(new RawValueDashboardSettings
                {
                    ValueBinding = SuperOverlay.Dashboards.Runtime.DashboardFieldBinding.Telemetry("Speed")
                })),

            new(
                gearId,
                "dashboard.raw-value",
                JsonSerializer.Serialize(new RawValueDashboardSettings
                {
                    ValueBinding = SuperOverlay.Dashboards.Runtime.DashboardFieldBinding.Telemetry("Gear"),
                    FontSize = 80,
                    TextAlignment = TextAlignment.Center
                })),

            new(
                trackId,
                "dashboard.raw-value",
                JsonSerializer.Serialize(new RawValueDashboardSettings
                {
                    ValueBinding = SuperOverlay.Dashboards.Runtime.DashboardFieldBinding.SessionInfo("WeekendInfo.TrackDisplayName"),
                    FontSize = 22,
                    TextAlignment = TextAlignment.Left
                }))
        };

        var placements = new List<LayoutItemPlacement>
        {
            new(
                speedId,
                20,
                20,
                160,
                50,
                10),

            new(
                gearId,
                150,
                100,
                120,
                120,
                10),

            new(
                trackId,
                20,
                210,
                320,
                50,
                10)
        };

        var links = new List<LayoutItemLink>
        {
            new(
                gearId,
                speedId,
                LayoutDockSide.Left,
                LayoutDockSide.Right,
                10)
        };

        var canvasWidth = Math.Max(1280, Math.Round(SystemParameters.PrimaryScreenWidth));
        var canvasHeight = Math.Max(720, Math.Round(SystemParameters.PrimaryScreenHeight));

        return new LayoutDocument(
            "1.0",
            "Default Layout",
            new LayoutCanvas(canvasWidth, canvasHeight),
            items,
            placements,
            links);
    }
}
