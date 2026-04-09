using SuperOverlay.Dashboards.Contracts;

namespace SuperOverlay.Dashboards.Registry;

public sealed class DashboardRegistry
{
    private readonly Dictionary<string, IDashboardDefinition> _definitions = new();

    public void Register(IDashboardDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _definitions[definition.TypeId] = definition;
    }

    public IDashboardDefinition Get(string typeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);

        if (_definitions.TryGetValue(typeId, out var definition))
        {
            return definition;
        }

        throw new InvalidOperationException($"Dashboard definition '{typeId}' is not registered.");
    }

    public IReadOnlyCollection<IDashboardDefinition> GetAll()
    {
        return _definitions.Values;
    }

    public IReadOnlyList<DashboardCatalogItem> GetCatalog()
    {
        return _definitions.Values
            .Select(x => new DashboardCatalogItem(x.TypeId, x.DisplayName, x.SettingsType))
            .OrderBy(x => x.DisplayName)
            .ToList();
    }
}
