using SuperOverlay.Dashboards.Contracts;
using SuperOverlay.LayoutBuilder.Contracts;
using System;

namespace SuperOverlay.Dashboards.Items.Gear;

public sealed class GearDashboardDefinition : IDashboardDefinition
{
    public string TypeId => "dashboard.gear";
    public string DisplayName => "Gear";
    public Type SettingsType => typeof(GearDashboardSettings);

    public object CreateDefaultSettings() => new GearDashboardSettings();

    public ILayoutItemPresenter CreatePresenter()
    {
        return new GearDashboardPresenter();
    }
}