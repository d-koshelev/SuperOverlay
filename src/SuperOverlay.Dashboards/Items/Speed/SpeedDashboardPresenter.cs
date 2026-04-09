using System.Windows;
using System.Windows.Controls;
using SuperOverlay.LayoutBuilder.Contracts;
using SuperOverlay.Dashboards.Runtime;

namespace SuperOverlay.Dashboards.Items.Speed;

public sealed class SpeedDashboardPresenter : ILayoutItemPresenter
{
    private readonly TextBlock _view = new()
    {
        FontSize = 32,
        Text = "0",
        Foreground = System.Windows.Media.Brushes.White,
        Margin = new Thickness(20, 20, 0, 0)
    };

    public object View => _view;

    public void ApplySettings(object settings)
    {
    }

    public void Update(object runtimeState)
    {
        if (runtimeState is not DashboardRuntimeState state)
            return;

        _view.Text = $"{state.Vehicle.SpeedKph}";
    }
}