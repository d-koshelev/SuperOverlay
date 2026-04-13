using SuperOverlay.Core.Layouts.Contracts;
using SuperOverlay.Dashboards.Contracts;

namespace SuperOverlay.Dashboards.Items.RawValue;

public sealed class RawValueDashboardDefinition : IDashboardDefinition
{
    public string TypeId => "dashboard.raw-value";
    public string DisplayName => "Raw Value";
    public Type SettingsType => typeof(RawValueDashboardSettings);

    public object CreateDefaultSettings() => new RawValueDashboardSettings();

    public object MaterializeSettings(object rawSettings) => DashboardSettingsMaterializer.Materialize<RawValueDashboardSettings>(rawSettings);

    public ILayoutItemPresenter CreatePresenter()
    {
        return new RawValueDashboardPresenter();
    }
}
