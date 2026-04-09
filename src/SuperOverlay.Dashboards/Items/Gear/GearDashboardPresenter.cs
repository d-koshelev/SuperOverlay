using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SuperOverlay.Dashboards.Runtime;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.Gear;

public sealed class GearDashboardPresenter : ILayoutItemPresenter
{
    private readonly TextBlock _text = new()
    {
        FontSize = 80,
        FontWeight = FontWeights.SemiBold,
        Text = "-",
        Foreground = Brushes.White,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        TextAlignment = TextAlignment.Center
    };

    private readonly Border _view;
    private GearDashboardSettings _settings = new();

    public GearDashboardPresenter()
    {
        _view = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(31, 41, 55)),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10),
            Child = _text
        };
    }

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

        _text.Text = gear switch
        {
            < 0 => "R",
            0 => _settings.ShowNeutralAsN ? "N" : "0",
            _ => gear.ToString()
        };
    }
}
