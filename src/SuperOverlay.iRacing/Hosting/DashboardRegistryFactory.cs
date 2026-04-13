using SuperOverlay.Dashboards.Items.DecorativePanel;
using SuperOverlay.Dashboards.Items.RawValue;
using SuperOverlay.Dashboards.Items.ShiftLeds;
using SuperOverlay.Dashboards.Registry;

namespace SuperOverlay.iRacing.Hosting;

public static class DashboardRegistryFactory
{
    public static DashboardRegistry Create()
    {
        var registry = new DashboardRegistry();

        registry.Register(new DecorativePanelDashboardDefinition());
        registry.Register(new RawValueDashboardDefinition());
        registry.Register(new ShiftLedDashboardDefinition());

        return registry;
    }
}
