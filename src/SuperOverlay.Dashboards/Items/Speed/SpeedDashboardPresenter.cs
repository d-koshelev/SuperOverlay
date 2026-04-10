using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SuperOverlay.Dashboards.Runtime;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.Dashboards.Items.Speed;

public sealed class SpeedDashboardPresenter : ILayoutItemPresenter
{
    private readonly TextBlock _text = new()
    {
        FontSize = 32,
        FontWeight = FontWeights.SemiBold,
        Text = "0",
        Foreground = Brushes.White,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Left
    };

    private readonly Border _view;
    private SpeedDashboardSettings _settings = new();

    public SpeedDashboardPresenter()
    {
        _view = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(31, 41, 55)),
            CornerRadius = new CornerRadius(0),
            Padding = new Thickness(14, 8, 14, 8),
            Child = _text
        };
    }

    public object View => _view;

    public void ApplySettings(object settings)
    {
        if (settings is SpeedDashboardSettings typed)
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

        _text.Text = _settings.ShowUnit
            ? $"{state.Vehicle.SpeedKph} {_settings.UnitText}"
            : state.Vehicle.SpeedKph.ToString();
    }
}
