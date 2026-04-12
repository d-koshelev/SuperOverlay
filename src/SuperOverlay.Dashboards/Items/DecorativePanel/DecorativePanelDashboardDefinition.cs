using SuperOverlay.Dashboards.Contracts;
using SuperOverlay.Core.Layouts.Contracts;

namespace SuperOverlay.Dashboards.Items.DecorativePanel;

public sealed class DecorativePanelDashboardDefinition : IDashboardDefinition
{
    public string TypeId => "dashboard.decorative-panel";
    public string DisplayName => "Decorative Panel";
    public Type SettingsType => typeof(DecorativePanelDashboardSettings);

    public object CreateDefaultSettings() => new DecorativePanelDashboardSettings();

    public object MaterializeSettings(object rawSettings) => DashboardSettingsMaterializer.Materialize<DecorativePanelDashboardSettings>(rawSettings);

    public ILayoutItemPresenter CreatePresenter()
    {
        return new DecorativePanelDashboardPresenter();
    }
}
