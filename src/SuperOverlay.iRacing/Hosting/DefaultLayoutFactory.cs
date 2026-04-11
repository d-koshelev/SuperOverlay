using System.Windows;
using System.Text.Json;
using SuperOverlay.Dashboards.Items.Gear;
using SuperOverlay.Dashboards.Items.Speed;
using SuperOverlay.LayoutBuilder.Layout;

namespace SuperOverlay.iRacing.Hosting;

public static class DefaultLayoutFactory
{
    public static LayoutDocument Create()
    {
        var gearId = Guid.NewGuid();
        var speedId = Guid.NewGuid();

        var items = new List<LayoutItemInstance>
        {
            new(
                gearId,
                "dashboard.gear",
                JsonSerializer.Serialize(new GearDashboardSettings())),

            new(
                speedId,
                "dashboard.speed",
                JsonSerializer.Serialize(new SpeedDashboardSettings()))
        };

        var placements = new List<LayoutItemPlacement>
        {
            new(
                gearId,
                150,
                100,
                120,
                120,
                10),

            new(
                speedId,
                20,
                20,
                160,
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
