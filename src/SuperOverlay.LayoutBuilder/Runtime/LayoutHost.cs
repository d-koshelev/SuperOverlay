using SuperOverlay.LayoutBuilder.Layout;
using System.Windows;
using System.Windows.Controls;

namespace SuperOverlay.LayoutBuilder.Runtime;

public sealed class LayoutHost
{
    private readonly Grid _root;
    private readonly List<RuntimeLayoutItem> _items = new();
    private Guid? _selectedItemId;

    public LayoutHost(Grid root)
    {
        ArgumentNullException.ThrowIfNull(root);
        _root = root;
    }

    public IReadOnlyList<RuntimeLayoutItem> Items => _items;

    public void Clear()
    {
        _root.Children.Clear();
        _items.Clear();
        _selectedItemId = null;
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
        item.SetSelected(item.Item.Id == _selectedItemId);

        _items.Add(item);
        _root.Children.Add(item.View);
    }

    public void SelectItem(Guid? itemId)
    {
        _selectedItemId = itemId;

        foreach (var item in _items)
        {
            item.SetSelected(item.Item.Id == itemId);
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
