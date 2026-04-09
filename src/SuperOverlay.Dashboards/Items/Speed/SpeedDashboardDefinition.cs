using System.Text.Json;
using SuperOverlay.Dashboards.Contracts;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.Speed;

public sealed class SpeedDashboardDefinition : IDashboardDefinition
{
    public string TypeId => "dashboard.speed";
    public string DisplayName => "Speed";
    public Type SettingsType => typeof(SpeedDashboardSettings);

    public object CreateDefaultSettings() => new SpeedDashboardSettings();

    public object MaterializeSettings(object rawSettings)
    {
        if (rawSettings is SpeedDashboardSettings typed)
        {
            return typed;
        }

        if (rawSettings is string json && !string.IsNullOrWhiteSpace(json))
        {
            return JsonSerializer.Deserialize<SpeedDashboardSettings>(json)
                   ?? new SpeedDashboardSettings();
        }

        return new SpeedDashboardSettings();
    }

    public ILayoutItemPresenter CreatePresenter()
    {
        return new SpeedDashboardPresenter();
    }
}
