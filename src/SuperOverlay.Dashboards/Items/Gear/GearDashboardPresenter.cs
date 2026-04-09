using System.Windows;
using System.Windows.Controls;
using SuperOverlay.Dashboards.Runtime;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.Gear;

public sealed class GearDashboardPresenter : ILayoutItemPresenter
{
    private readonly TextBlock _view = new()
    {
        FontSize = 80,
        Text = "-",
        Foreground = System.Windows.Media.Brushes.White,
        Margin = new Thickness(0)
    };

    private GearDashboardSettings _settings = new();

    public object View => _view;

    public void ApplySettings(object settings)
    {
        if (settings is GearDashboardSettings typed)
        {
            _settings = typed;
        }
    }

    public void Update(object runtimeState)
    {
        if (runtimeState is not DashboardRuntimeState state)
            return;

        var gear = state.Vehicle.Gear;

        _view.Text = gear switch
        {
            < 0 => "R",
            0 => _settings.ShowNeutralAsN ? "N" : "0",
            _ => gear.ToString()
        };
    }
}
