using System.Globalization;

namespace SuperOverlay.Dashboards.Runtime;

public static class DashboardRuntimeValueResolver
{
    public static bool TryResolveValue(DashboardRuntimeState state, DashboardFieldBinding? binding, out object? value)
    {
        value = null;
        if (binding is null || string.IsNullOrWhiteSpace(binding.FieldPath))
        {
            return false;
        }

        var source = binding.Source switch
        {
            DashboardFieldSource.TelemetryRaw => state.TelemetryRaw,
            DashboardFieldSource.SessionInfo => state.SessionInfo,
            _ => null
        };

        return source is not null && source.TryGetValue(binding.FieldPath, out value);
    }

    public static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string text => text,
            char ch => ch.ToString(),
            bool boolean => boolean ? "True" : "False",
            Enum enumValue => enumValue.ToString(),
            float single => single.ToString(CultureInfo.InvariantCulture),
            double dbl => dbl.ToString(CultureInfo.InvariantCulture),
            decimal dec => dec.ToString(CultureInfo.InvariantCulture),
            IEnumerable<object?> sequence => string.Join(", ", sequence.Select(FormatValue)),
            System.Collections.IEnumerable enumerable when value is not string => string.Join(", ", enumerable.Cast<object?>().Select(FormatValue)),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    public static bool TryResolveDouble(DashboardRuntimeState state, DashboardFieldBinding? binding, out double value)
    {
        value = 0;
        if (!TryResolveValue(state, binding, out var raw) || raw is null)
        {
            return false;
        }

        switch (raw)
        {
            case byte typed:
                value = typed;
                return true;
            case sbyte typed:
                value = typed;
                return true;
            case short typed:
                value = typed;
                return true;
            case ushort typed:
                value = typed;
                return true;
            case int typed:
                value = typed;
                return true;
            case uint typed:
                value = typed;
                return true;
            case long typed:
                value = typed;
                return true;
            case ulong typed:
                value = typed;
                return true;
            case float typed:
                value = typed;
                return true;
            case double typed:
                value = typed;
                return true;
            case decimal typed:
                value = (double)typed;
                return true;
            case string text when double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsed):
                value = parsed;
                return true;
            default:
                return false;
        }
    }
}
