namespace SuperOverlay.Core.Layouts.Editing;

public sealed class OverlaySelectionState
{
    private HashSet<Guid> _itemIds = new();

    public Guid? PrimaryItemId { get; private set; }
    public IReadOnlyCollection<Guid> ItemIds => _itemIds;
    public bool HasSelection => _itemIds.Count > 0;

    public void Clear()
    {
        _itemIds = new HashSet<Guid>();
        PrimaryItemId = null;
    }

    public void Replace(IEnumerable<Guid> itemIds, Guid? primaryItemId)
    {
        ArgumentNullException.ThrowIfNull(itemIds);

        _itemIds = itemIds.Distinct().ToHashSet();
        PrimaryItemId = primaryItemId is not null && _itemIds.Contains(primaryItemId.Value)
            ? primaryItemId
            : _itemIds.FirstOrDefault();

        if (_itemIds.Count == 0)
        {
            PrimaryItemId = null;
        }
    }

    public bool Contains(Guid itemId) => _itemIds.Contains(itemId);

    public void Remove(Guid itemId)
    {
        if (!_itemIds.Remove(itemId))
        {
            return;
        }

        if (PrimaryItemId == itemId)
        {
            PrimaryItemId = _itemIds.FirstOrDefault();
            if (_itemIds.Count == 0)
            {
                PrimaryItemId = null;
            }
        }
    }
}
