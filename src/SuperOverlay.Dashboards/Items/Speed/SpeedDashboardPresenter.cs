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
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14, 8, 14, 8),
            Child = _text
        };
    }

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

        _text.Text = _settings.ShowUnit
            ? $"{state.Vehicle.SpeedKph} {_settings.UnitText}"
            : state.Vehicle.SpeedKph.ToString();
    }
}
