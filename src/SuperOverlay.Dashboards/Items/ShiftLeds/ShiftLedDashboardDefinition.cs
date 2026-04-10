using System.Text.Json;
using SuperOverlay.Dashboards.Contracts;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.ShiftLeds;

public sealed class ShiftLedDashboardDefinition : IDashboardDefinition
{
    public string TypeId => "dashboard.shift-leds";
    public string DisplayName => "LED Shift Panel";
    public Type SettingsType => typeof(ShiftLedDashboardSettings);

    public object CreateDefaultSettings() => new ShiftLedDashboardSettings();

    public object MaterializeSettings(object rawSettings)
    {
        if (rawSettings is ShiftLedDashboardSettings typed)
        {
            return typed;
        }

        if (rawSettings is string json && !string.IsNullOrWhiteSpace(json))
        {
            try
            {
                return JsonSerializer.Deserialize<ShiftLedDashboardSettings>(json)
                       ?? new ShiftLedDashboardSettings();
            }
            catch (JsonException)
            {
                return new ShiftLedDashboardSettings();
            }
        }

        return new ShiftLedDashboardSettings();
    }

    public ILayoutItemPresenter CreatePresenter()
    {
        return new ShiftLedDashboardPresenter();
    }
}
