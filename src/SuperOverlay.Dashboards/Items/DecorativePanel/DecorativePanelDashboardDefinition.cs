using System.Text.Json;
using SuperOverlay.Dashboards.Contracts;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.DecorativePanel;

public sealed class DecorativePanelDashboardDefinition : IDashboardDefinition
{
    public string TypeId => "dashboard.decorative-panel";
    public string DisplayName => "Decorative Panel";
    public Type SettingsType => typeof(DecorativePanelDashboardSettings);

    public object CreateDefaultSettings() => new DecorativePanelDashboardSettings();

    public object MaterializeSettings(object rawSettings)
    {
        if (rawSettings is DecorativePanelDashboardSettings typed)
        {
            return typed;
        }

        if (rawSettings is string json && !string.IsNullOrWhiteSpace(json))
        {
            try
            {
                return JsonSerializer.Deserialize<DecorativePanelDashboardSettings>(json)
                       ?? new DecorativePanelDashboardSettings();
            }
            catch (JsonException)
            {
                return new DecorativePanelDashboardSettings();
            }
        }

        return new DecorativePanelDashboardSettings();
    }

    public ILayoutItemPresenter CreatePresenter()
    {
        return new DecorativePanelDashboardPresenter();
    }
}
