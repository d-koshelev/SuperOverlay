using System.Reflection;
using System.Text.Json;

namespace SuperOverlay.Dashboards.Contracts;

public static class DashboardSettingsMaterializer
{
    public static T Materialize<T>(object? rawSettings)
    {
        if (rawSettings is T typed)
        {
            return typed;
        }

        if (rawSettings is JsonElement jsonElement)
        {
            try
            {
                var deserialized = jsonElement.Deserialize<T>();
                if (deserialized is not null)
                {
                    return deserialized;
                }
            }
            catch (JsonException)
            {
            }
        }

        if (rawSettings is string json && !string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var deserialized = JsonSerializer.Deserialize<T>(json);
                if (deserialized is not null)
                {
                    return deserialized;
                }
            }
            catch (JsonException)
            {
            }
        }

        return CreateDefault<T>();
    }

    private static T CreateDefault<T>()
    {
        var type = typeof(T);

        if (type.IsValueType)
        {
            return default!;
        }

        var parameterlessConstructor = type.GetConstructor(Type.EmptyTypes);
        if (parameterlessConstructor is not null)
        {
            return (T)parameterlessConstructor.Invoke(null);
        }

        var optionalConstructor = type
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(ctor => ctor.GetParameters().Length)
            .FirstOrDefault(ctor => ctor.GetParameters().All(parameter => parameter.IsOptional || parameter.HasDefaultValue));

        if (optionalConstructor is not null)
        {
            var arguments = optionalConstructor
                .GetParameters()
                .Select(GetDefaultArgumentValue)
                .ToArray();

            return (T)optionalConstructor.Invoke(arguments);
        }

        throw new InvalidOperationException(
            $"Type '{type.FullName}' cannot be materialized because it does not have a public parameterless constructor and no public constructor with only optional/default-valued parameters was found.");
    }

    private static object? GetDefaultArgumentValue(ParameterInfo parameter)
    {
        if (parameter.HasDefaultValue)
        {
            return parameter.DefaultValue;
        }

        if (parameter.ParameterType.IsValueType)
        {
            return Activator.CreateInstance(parameter.ParameterType);
        }

        return null;
    }
}
