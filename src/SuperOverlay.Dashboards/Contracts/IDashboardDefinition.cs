using SuperOverlay.LayoutBuilder.Contracts;
using System;

namespace SuperOverlay.Dashboards.Contracts;

public interface IDashboardDefinition : ILayoutItemDefinition
{
    Type SettingsType { get; }
}