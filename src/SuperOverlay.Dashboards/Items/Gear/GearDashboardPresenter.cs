using System.Windows;
using System.Windows.Controls;
using SuperOverlay.LayoutBuilder.Contracts;
using SuperOverlay.Dashboards.Runtime;

namespace SuperOverlay.Dashboards.Items.Gear;

public sealed class GearDashboardPresenter : ILayoutItemPresenter
{
    private readonly TextBlock _view = new()
    {
        FontSize = 80,
        Text = "-",
        Foreground = System.Windows.Media.Brushes.White,
        Margin = new Thickness(150, 100, 0, 0)
    };

    public object View => _view;

    public void ApplySettings(object settings)
    {
    }

    public void Update(object runtimeState)
    {
        if (runtimeState is not DashboardRuntimeState state)
            return;

        _view.Text = state.Vehicle.Gear.ToString();
    }
}