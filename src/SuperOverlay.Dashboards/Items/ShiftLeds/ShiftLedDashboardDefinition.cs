using SuperOverlay.Dashboards.Contracts;
using SuperOverlay.Core.Layouts.Contracts;

namespace SuperOverlay.Dashboards.Items.ShiftLeds;

public sealed class ShiftLedDashboardDefinition : IDashboardDefinition
{
    public string TypeId => "dashboard.shift-leds";
    public string DisplayName => "LED Shift Panel";
    public Type SettingsType => typeof(ShiftLedDashboardSettings);

    public object CreateDefaultSettings() => new ShiftLedDashboardSettings();

    public object MaterializeSettings(object rawSettings) => DashboardSettingsMaterializer.Materialize<ShiftLedDashboardSettings>(rawSettings);

    public ILayoutItemPresenter CreatePresenter()
    {
        return new ShiftLedDashboardPresenter();
    }
}
