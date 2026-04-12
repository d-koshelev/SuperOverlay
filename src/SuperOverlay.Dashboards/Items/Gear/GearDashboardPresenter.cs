using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SuperOverlay.Dashboards.Runtime;
using SuperOverlay.Core.Layouts.Contracts;

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
            CornerRadius = new CornerRadius(0),
            Padding = new Thickness(10),
            Child = _text
        };
    }

    public object View => _view;

    public void ApplySettings(object settings)
    {
        if (settings is GearDashboardSettings typed)
        {
            _settings = typed with
            {
                CornerTopLeft = Math.Clamp(typed.CornerTopLeft, 0, 64),
                CornerTopRight = Math.Clamp(typed.CornerTopRight, 0, 64),
                CornerBottomRight = Math.Clamp(typed.CornerBottomRight, 0, 64),
                CornerBottomLeft = Math.Clamp(typed.CornerBottomLeft, 0, 64)
            };
            _view.CornerRadius = new CornerRadius(_settings.CornerTopLeft, _settings.CornerTopRight, _settings.CornerBottomRight, _settings.CornerBottomLeft);
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
