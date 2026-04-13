using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SuperOverlay.Core.Layouts.Contracts;
using SuperOverlay.Dashboards.Runtime;

namespace SuperOverlay.Dashboards.Items.RawValue;

public sealed class RawValueDashboardPresenter : ILayoutItemPresenter
{
    private readonly TextBlock _text = new()
    {
        FontSize = 32,
        FontWeight = FontWeights.SemiBold,
        Text = "0",
        Foreground = Brushes.White,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        TextAlignment = TextAlignment.Left,
        TextTrimming = TextTrimming.CharacterEllipsis
    };

    private readonly Border _view;
    private RawValueDashboardSettings _settings = new();

    public RawValueDashboardPresenter()
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
        if (settings is not RawValueDashboardSettings typed)
        {
            return;
        }

        _settings = typed with
        {
            FontSize = Math.Clamp(typed.FontSize, 8, 160),
            CornerTopLeft = Math.Clamp(typed.CornerTopLeft, 0, 64),
            CornerTopRight = Math.Clamp(typed.CornerTopRight, 0, 64),
            CornerBottomRight = Math.Clamp(typed.CornerBottomRight, 0, 64),
            CornerBottomLeft = Math.Clamp(typed.CornerBottomLeft, 0, 64)
        };

        _text.FontSize = _settings.FontSize;
        _text.TextAlignment = _settings.TextAlignment;
        _view.CornerRadius = new CornerRadius(_settings.CornerTopLeft, _settings.CornerTopRight, _settings.CornerBottomRight, _settings.CornerBottomLeft);
    }

    public void Update(object runtimeState)
    {
        if (runtimeState is not DashboardRuntimeState state)
        {
            return;
        }

        if (DashboardRuntimeValueResolver.TryResolveValue(state, _settings.ValueBinding, out var rawValue))
        {
            _text.Text = DashboardRuntimeValueResolver.FormatValue(rawValue);
            return;
        }

        _text.Text = string.Empty;
    }
}
