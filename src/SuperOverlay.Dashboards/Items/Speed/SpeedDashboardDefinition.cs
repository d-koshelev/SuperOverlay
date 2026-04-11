using SuperOverlay.Dashboards.Contracts;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.Speed;

public sealed class SpeedDashboardDefinition : IDashboardDefinition
{
    public string TypeId => "dashboard.speed";
    public string DisplayName => "Speed";
    public Type SettingsType => typeof(SpeedDashboardSettings);

    public object CreateDefaultSettings() => new SpeedDashboardSettings();

    public object MaterializeSettings(object rawSettings) => DashboardSettingsMaterializer.Materialize<SpeedDashboardSettings>(rawSettings);

    public ILayoutItemPresenter CreatePresenter()
    {
        return new SpeedDashboardPresenter();
    }
}