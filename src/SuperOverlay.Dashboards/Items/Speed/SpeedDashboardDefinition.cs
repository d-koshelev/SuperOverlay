using SuperOverlay.Dashboards.Speed;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.Speed;

public sealed class SpeedDashboardDefinition : ILayoutItemDefinition
{
    public string TypeId => "dashboard.speed";
    public string DisplayName => "Speed";

    public object CreateDefaultSettings() => new object();

    public ILayoutItemPresenter CreatePresenter()
    {
        return new SpeedDashboardPresenter();
    }
}