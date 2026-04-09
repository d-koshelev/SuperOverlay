using SuperOverlay.Dashboards.Items.Gear;
using SuperOverlay.Dashboards.Items.Speed;
using SuperOverlay.Dashboards.Registry;

namespace SuperOverlay.iRacing.Hosting;

public static class DashboardRegistryFactory
{
    public static DashboardRegistry Create()
    {
        var registry = new DashboardRegistry();

        registry.Register(new GearDashboardDefinition());
        registry.Register(new SpeedDashboardDefinition());

        return registry;
    }
}
