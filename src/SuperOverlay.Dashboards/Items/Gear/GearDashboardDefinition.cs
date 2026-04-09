using SuperOverlay.Dashboards.Gear;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.Gear;

public sealed class GearDashboardDefinition : ILayoutItemDefinition
{
    public string TypeId => "dashboard.gear";
    public string DisplayName => "Gear";

    public object CreateDefaultSettings() => new object();

    public ILayoutItemPresenter CreatePresenter()
    {
        return new GearDashboardPresenter();
    }
}