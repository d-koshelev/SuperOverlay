using System.Collections.ObjectModel;

namespace SuperOverlay.Core.Layouts.Layout;

public sealed class LayoutPlacementIndex
{
    private readonly IReadOnlyDictionary<Guid, LayoutItemPlacement> _placements;

    public LayoutPlacementIndex(IEnumerable<LayoutItemPlacement> placements)
    {
        ArgumentNullException.ThrowIfNull(placements);

        var dictionary = new Dictionary<Guid, LayoutItemPlacement>();

        foreach (var placement in placements)
        {
            if (dictionary.ContainsKey(placement.ItemId))
            {
                throw new InvalidOperationException(
                    $"Duplicate placement detected for item '{placement.ItemId}'.");
            }

            dictionary[placement.ItemId] = placement;
        }

        _placements = new ReadOnlyDictionary<Guid, LayoutItemPlacement>(dictionary);
    }

    public LayoutItemPlacement GetRequired(Guid itemId)
    {
        if (_placements.TryGetValue(itemId, out var placement))
        {
            return placement;
        }

        throw new InvalidOperationException(
            $"Placement for item '{itemId}' was not found.");
    }
}
