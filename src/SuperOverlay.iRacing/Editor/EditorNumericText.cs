using System.Globalization;

namespace SuperOverlay.iRacing.Editor;

internal static class EditorNumericText
{
    public static string FormatDouble(double value) => value.ToString("0.##", CultureInfo.CurrentCulture);

    public static bool TryParseDouble(string? text, out double value)
    {
        return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value);
    }

    public static bool TryParseNonNegativeDouble(string? text, out double value)
    {
        if (!TryParseDouble(text, out value))
        {
            return false;
        }

        return value >= 0;
    }

    public static bool TryParseUnitInterval(string? text, out double value)
    {
        if (!TryParseDouble(text, out value))
        {
            return false;
        }

        return value >= 0 && value <= 1;
    }
}
