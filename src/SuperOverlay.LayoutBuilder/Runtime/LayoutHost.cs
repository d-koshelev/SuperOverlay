using System.Windows.Controls;

namespace SuperOverlay.LayoutBuilder.Runtime;

public sealed class LayoutHost
{
    private readonly Grid _root;
    private readonly List<RuntimeLayoutItem> _items = new();

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
    }

    public void Load(IEnumerable<RuntimeLayoutItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        Clear();

        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    public void AddItem(RuntimeLayoutItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        item.ApplySettings();
        item.ApplyPlacement();

        _items.Add(item);
        _root.Children.Add(item.View);
    }

    public void Update(object runtimeState)
    {
        foreach (var item in _items)
        {
            item.Update(runtimeState);
        }
    }
}
