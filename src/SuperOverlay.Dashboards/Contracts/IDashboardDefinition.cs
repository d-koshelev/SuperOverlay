using SuperOverlay.Core.Layouts.Contracts;

namespace SuperOverlay.Dashboards.Contracts;

public interface IDashboardDefinition : ILayoutItemDefinition
{
    Type SettingsType { get; }

    object MaterializeSettings(object rawSettings);
}
