using System.Windows;
using System.Windows.Controls;
using SuperOverlay.Dashboards.Runtime;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.Speed;

public sealed class SpeedDashboardPresenter : ILayoutItemPresenter
{
    private readonly TextBlock _view = new()
    {
        FontSize = 32,
        Text = "0",
        Foreground = System.Windows.Media.Brushes.White,
        Margin = new Thickness(0)
    };

    private SpeedDashboardSettings _settings = new();

    public object View => _view;

    public void ApplySettings(object settings)
    {
        if (settings is SpeedDashboardSettings typed)
        {
            _settings = typed;
        }
    }

    public void Update(object runtimeState)
    {
        if (runtimeState is not DashboardRuntimeState state)
            return;

        _view.Text = _settings.ShowUnit
            ? $"{state.Vehicle.SpeedKph} {_settings.UnitText}"
            : state.Vehicle.SpeedKph.ToString();
    }
}