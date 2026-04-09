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
                new GearDashboardSettings()),

            new(
                speedId,
                "dashboard.speed",
                new SpeedDashboardSettings())
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

        return new LayoutDocument(
            "1.0",
            "Default Layout",
            new LayoutCanvas(400, 300),
            items,
            placements);
    }
}