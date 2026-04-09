using System.Text.Json;
using SuperOverlay.Dashboards.Contracts;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.Gear;

public sealed class GearDashboardDefinition : IDashboardDefinition
{
    public string TypeId => "dashboard.gear";
    public string DisplayName => "Gear";
    public Type SettingsType => typeof(GearDashboardSettings);

    public object CreateDefaultSettings() => new GearDashboardSettings();

    public object MaterializeSettings(object rawSettings)
    {
        if (rawSettings is GearDashboardSettings typed)
        {
            return typed;
        }

        if (rawSettings is string json && !string.IsNullOrWhiteSpace(json))
        {
            try
            {
                return JsonSerializer.Deserialize<GearDashboardSettings>(json)
                       ?? new GearDashboardSettings();
            }
            catch (JsonException)
            {
                return new GearDashboardSettings();
            }
        }

        return new GearDashboardSettings();
    }

    public ILayoutItemPresenter CreatePresenter()
    {
        return new GearDashboardPresenter();
    }
}