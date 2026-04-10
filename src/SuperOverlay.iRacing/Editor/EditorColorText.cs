using MediaColor = System.Windows.Media.Color;
using MediaColorConverter = System.Windows.Media.ColorConverter;

namespace SuperOverlay.iRacing.Editor;

internal static class EditorColorText
{
    public static bool IsValidColorValue(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        try
        {
            return MediaColorConverter.ConvertFromString(text.Trim()) is MediaColor;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    public static bool AreValidColorListValues(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        foreach (var part in text.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!IsValidColorValue(part))
            {
                return false;
            }
        }

        return true;
    }

    public static string NormalizeColorText(string? text, string fallback)
    {
        return string.IsNullOrWhiteSpace(text) ? fallback : text.Trim();
    }

    public static string NormalizeColorListText(string? text, string fallback)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return fallback;
        }

        var values = text.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(IsValidColorValue)
            .Select(x => x.Trim())
            .ToArray();

        return values.Length == 0 ? fallback : string.Join(",", values);
    }

    public static MediaColor ParseMediaColor(string? text, string fallback)
    {
        if (IsValidColorValue(text))
        {
            return (MediaColor)MediaColorConverter.ConvertFromString(text!.Trim())!;
        }

        return (MediaColor)MediaColorConverter.ConvertFromString(fallback)!;
    }
}
