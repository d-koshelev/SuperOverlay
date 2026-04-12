using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SuperOverlay.Core.Layouts.Contracts;

namespace SuperOverlay.Dashboards.Items.DecorativePanel;

public sealed class DecorativePanelDashboardPresenter : ILayoutItemPresenter
{
    private readonly Border _view;
    private DecorativePanelDashboardSettings _settings = new();

    public DecorativePanelDashboardPresenter()
    {
        _view = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(204, 31, 41, 55)),
            CornerRadius = new CornerRadius(0),
            Opacity = 1
        };
    }

    public object View => _view;

    public void ApplySettings(object settings)
    {
        if (settings is not DecorativePanelDashboardSettings typed)
        {
            return;
        }

        _settings = typed with
        {
            BackgroundColor = string.IsNullOrWhiteSpace(typed.BackgroundColor) ? "#CC1F2937" : typed.BackgroundColor.Trim(),
            Opacity = Math.Clamp(typed.Opacity, 0, 1),
            CornerTopLeft = Math.Clamp(typed.CornerTopLeft, 0, 64),
            CornerTopRight = Math.Clamp(typed.CornerTopRight, 0, 64),
            CornerBottomRight = Math.Clamp(typed.CornerBottomRight, 0, 64),
            CornerBottomLeft = Math.Clamp(typed.CornerBottomLeft, 0, 64)
        };

        _view.Background = new SolidColorBrush(ParseColor(_settings.BackgroundColor, Color.FromArgb(204, 31, 41, 55)));
        _view.Opacity = _settings.Opacity;
        _view.CornerRadius = new CornerRadius(_settings.CornerTopLeft, _settings.CornerTopRight, _settings.CornerBottomRight, _settings.CornerBottomLeft);
    }

    public void Update(object runtimeState)
    {
    }

    private static Color ParseColor(string text, Color fallback)
    {
        try
        {
            var value = ColorConverter.ConvertFromString(text);
            return value is Color color ? color : fallback;
        }
        catch
        {
            return fallback;
        }
    }
}
