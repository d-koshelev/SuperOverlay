using SuperOverlay.Core.Layouts.Layout;
using System.Windows;
using System.Windows.Controls;

namespace SuperOverlay.Core.Layouts.Runtime;

public sealed class LayoutHost
{
    private readonly Grid _root;
    private readonly OverlayShellMode _shellMode;
    private readonly List<RuntimeLayoutItem> _items = new();
    private Guid? _selectedItemId;
    private HashSet<Guid> _selectedItemIds = new();
    private HashSet<Guid> _highlightedGroupIds = new();

    public LayoutHost(Grid root, OverlayShellMode shellMode = OverlayShellMode.Editor)
    {
        ArgumentNullException.ThrowIfNull(root);
        _root = root;
        _shellMode = shellMode;
    }

    public IReadOnlyList<RuntimeLayoutItem> Items => _items;

    public void Clear()
    {
        _root.Children.Clear();
        _items.Clear();
        _selectedItemId = null;
        _selectedItemIds.Clear();
        _highlightedGroupIds.Clear();
    }

    public void Load(IEnumerable<RuntimeLayoutItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var selected = _selectedItemId;
        Clear();

        foreach (var item in items)
        {
            AddItem(item);
        }

        SelectItem(selected);
    }

    public void AddItem(RuntimeLayoutItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        item.ApplySettings();
        item.ApplyPlacement();
        item.SetSelectionState(item.Item.Id == _selectedItemId, _selectedItemIds.Contains(item.Item.Id), _highlightedGroupIds.Contains(item.Item.Id));

        _items.Add(item);
        _root.Children.Add(item.View);
    }

    public void SelectItem(Guid? itemId)
    {
        _selectedItemId = itemId;
        _selectedItemIds = itemId is null ? new HashSet<Guid>() : new HashSet<Guid> { itemId.Value };

        RefreshVisualStates();
    }

    public void SetSelectedItems(Guid? primaryItemId, IEnumerable<Guid> itemIds)
    {
        ArgumentNullException.ThrowIfNull(itemIds);

        _selectedItemId = primaryItemId;
        _selectedItemIds = itemIds.ToHashSet();

        if (primaryItemId is not null && !_selectedItemIds.Contains(primaryItemId.Value))
        {
            _selectedItemIds.Add(primaryItemId.Value);
        }

        RefreshVisualStates();
    }

    public void SetGroupHighlight(IEnumerable<Guid> itemIds)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        _highlightedGroupIds = itemIds.ToHashSet();
        RefreshVisualStates();
    }

    private void RefreshVisualStates()
    {
        foreach (var item in _items)
        {
            var showsEditorSelection = _shellMode != OverlayShellMode.Runtime;
            item.SetSelectionState(
                showsEditorSelection && item.Item.Id == _selectedItemId,
                showsEditorSelection && _selectedItemIds.Contains(item.Item.Id),
                showsEditorSelection && _highlightedGroupIds.Contains(item.Item.Id));
        }
    }

    public RuntimeLayoutItem? HitTestItem(DependencyObject? element)
    {
        for (var index = _items.Count - 1; index >= 0; index--)
        {
            var item = _items[index];
            if (item.ContainsElement(element))
            {
                return item;
            }
        }

        return null;
    }

    public bool IsResizeHandleHit(DependencyObject? element, Guid itemId)
    {
        var item = _items.FirstOrDefault(x => x.Item.Id == itemId);
        return item is not null && item.IsResizeHandleElement(element);
    }

    public bool TryUpdatePlacement(Guid itemId, LayoutItemPlacement placement)
    {
        var item = _items.FirstOrDefault(x => x.Item.Id == itemId);
        if (item is null)
        {
            return false;
        }

        item.UpdatePlacement(placement);
        return true;
    }

    public void Update(object runtimeState)
    {
        foreach (var item in _items)
        {
            item.Update(runtimeState);
        }
    }
}
