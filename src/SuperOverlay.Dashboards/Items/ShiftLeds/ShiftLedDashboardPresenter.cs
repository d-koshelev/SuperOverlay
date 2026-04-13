using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SuperOverlay.Dashboards.Runtime;
using SuperOverlay.Core.Layouts.Contracts;

namespace SuperOverlay.Dashboards.Items.ShiftLeds;

public sealed class ShiftLedDashboardPresenter : ILayoutItemPresenter
{
    private static readonly string[] DefaultLedPalette =
    [
        "#006C8E",
        "#0097C9",
        "#00C2FF",
        "#00FF9C",
        "#B4FF00",
        "#FFD400",
        "#FFB000",
        "#FF7A00",
        "#FF4E00",
        "#FF3C00",
        "#FF2A00",
        "#FF1E00"
    ];

    private readonly Border _host;
    private readonly UniformGrid _ledGrid;
    private readonly List<Border> _leds = new();
    private readonly List<SolidColorBrush> _onBrushes = new();
    private ShiftLedDashboardSettings _settings = new();
    private bool _blinkOn = true;
    private SolidColorBrush _offBrush = new(ParseColor("#0F0F0F", Color.FromRgb(15, 15, 15)));
    private SolidColorBrush _fallbackOnBrush = new(ParseColor("#FF1E00", Color.FromRgb(255, 30, 0)));
    private SolidColorBrush _panelBrush = new(ParseColor("#E61F2937", Color.FromArgb(230, 31, 41, 55)));

    public ShiftLedDashboardPresenter()
    {
        _ledGrid = new UniformGrid
        {
            Rows = 1,
            Columns = _settings.LedCount,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _host = new Border
        {
            Background = _panelBrush,
            CornerRadius = new CornerRadius(0),
            Padding = new Thickness(10, 8, 10, 8),
            Child = _ledGrid
        };

        RebuildLeds();
    }

    public object View => _host;

    public void ApplySettings(object settings)
    {
        if (settings is not ShiftLedDashboardSettings typed)
        {
            return;
        }

        var normalized = typed with
        {
            LedCount = Math.Clamp(typed.LedCount, 4, 24),
            StartPercent = Math.Clamp(typed.StartPercent, 0.1, 0.95),
            BlinkPercent = Math.Clamp(typed.BlinkPercent, 0.1, 1.0),
            BackgroundColor = string.IsNullOrWhiteSpace(typed.BackgroundColor) ? "#E61F2937" : typed.BackgroundColor.Trim(),
            LedOffColor = string.IsNullOrWhiteSpace(typed.LedOffColor) ? "#0F0F0F" : typed.LedOffColor.Trim(),
            LedOnColor = string.IsNullOrWhiteSpace(typed.LedOnColor) ? "#FF1E00" : typed.LedOnColor.Trim(),
            LedOnColors = string.IsNullOrWhiteSpace(typed.LedOnColors) ? string.Join(",", DefaultLedPalette) : typed.LedOnColors.Trim(),
            CornerTopLeft = Math.Clamp(typed.CornerTopLeft, 0, 64),
            CornerTopRight = Math.Clamp(typed.CornerTopRight, 0, 64),
            CornerBottomRight = Math.Clamp(typed.CornerBottomRight, 0, 64),
            CornerBottomLeft = Math.Clamp(typed.CornerBottomLeft, 0, 64)
        };

        _settings = normalized;
        _panelBrush = new SolidColorBrush(ParseColor(normalized.BackgroundColor, Color.FromArgb(230, 31, 41, 55)));
        _offBrush = new SolidColorBrush(ParseColor(normalized.LedOffColor, Color.FromRgb(15, 15, 15)));
        _fallbackOnBrush = new SolidColorBrush(ParseColor(normalized.LedOnColor, Color.FromRgb(255, 30, 0)));
        RebuildOnBrushes();

        _host.Background = normalized.ShowBackground ? _panelBrush : Brushes.Transparent;
        _host.CornerRadius = new CornerRadius(normalized.CornerTopLeft, normalized.CornerTopRight, normalized.CornerBottomRight, normalized.CornerBottomLeft);
        RebuildLeds();
    }

    public void Update(object runtimeState)
    {
        if (runtimeState is not DashboardRuntimeState state)
        {
            return;
        }

        var shiftPercent = DashboardRuntimeValueResolver.TryResolveDouble(state, _settings.ValueBinding, out var rawValue)
            ? Math.Clamp(rawValue, 0.0, 1.0)
            : Math.Clamp(state.Vehicle.ShiftLightPercent, 0.0, 1.0);
        var activeCount = CalculateActiveCount(shiftPercent);
        var shouldBlink = shiftPercent >= _settings.BlinkPercent;
        if (shouldBlink)
        {
            _blinkOn = !_blinkOn;
        }
        else
        {
            _blinkOn = true;
        }

        for (var i = 0; i < _leds.Count; i++)
        {
            var led = _leds[i];
            if (i >= activeCount || (shouldBlink && !_blinkOn))
            {
                led.Background = _offBrush;
            }
            else
            {
                led.Background = i < _onBrushes.Count ? _onBrushes[i] : _fallbackOnBrush;
            }
        }
    }

    private int CalculateActiveCount(double shiftPercent)
    {
        if (shiftPercent <= _settings.StartPercent)
        {
            return 0;
        }

        var range = Math.Max(0.001, 1.0 - _settings.StartPercent);
        var normalized = Math.Clamp((shiftPercent - _settings.StartPercent) / range, 0.0, 1.0);
        return (int)Math.Ceiling(normalized * _leds.Count);
    }

    private void RebuildLeds()
    {
        _ledGrid.Children.Clear();
        _leds.Clear();
        _ledGrid.Columns = _settings.LedCount;

        for (var i = 0; i < _settings.LedCount; i++)
        {
            var led = new Border
            {
                Background = _offBrush,
                CornerRadius = new CornerRadius(0),
                Margin = new Thickness(2, 0, 2, 0),
                MinHeight = 18,
                BorderThickness = new Thickness(0)
            };

            _leds.Add(led);
            _ledGrid.Children.Add(led);
        }
    }

    private void RebuildOnBrushes()
    {
        _onBrushes.Clear();
        List<Color> colors;
        if (_settings.UsePerLedColors)
        {
            colors = ParseColorList(_settings.LedOnColors)
                .DefaultIfEmpty(ParseColor(_settings.LedOnColor, Color.FromRgb(255, 30, 0)))
                .ToList();
        }
        else
        {
            colors = [ParseColor(_settings.LedOnColor, Color.FromRgb(255, 30, 0))];
        }

        for (var i = 0; i < _settings.LedCount; i++)
        {
            var color = i < colors.Count ? colors[i] : colors[^1];
            _onBrushes.Add(new SolidColorBrush(color));
        }
    }

    private static IEnumerable<Color> ParseColorList(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        var parts = text.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            yield return ParseColor(part, Color.FromRgb(255, 30, 0));
        }
    }

    private static Color ParseColor(string text, Color fallback)
    {
        try
        {
            var value = ColorConverter.ConvertFromString(text);
            return value is Color color ? color : fallback;
        }
        catch (FormatException)
        {
            return fallback;
        }
        catch (NotSupportedException)
        {
            return fallback;
        }
    }
}
