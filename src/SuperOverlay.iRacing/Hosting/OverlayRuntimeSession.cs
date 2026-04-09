using SuperOverlay.Dashboards.Registry;
using SuperOverlay.Dashboards.Runtime;
using SuperOverlay.LayoutBuilder.Layout;
using SuperOverlay.LayoutBuilder.Persistence;
using SuperOverlay.LayoutBuilder.Runtime;

namespace SuperOverlay.iRacing.Hosting;

public sealed class OverlayRuntimeSession
{
    private readonly LayoutHost _layoutHost;
    private readonly DashboardRegistry _registry;
    private readonly LayoutRuntimeComposer _composer;
    private readonly LayoutFileStore _fileStore;
    private readonly LayoutMutationService _mutationService;
    private readonly string _layoutPath;

    private LayoutDocument _layout;
    private Guid? _selectedItemId;

    public OverlayRuntimeSession(
        LayoutHost layoutHost,
        DashboardRegistry registry,
        LayoutRuntimeComposer composer,
        LayoutFileStore fileStore,
        LayoutMutationService mutationService,
        string layoutPath,
        LayoutDocument layout)
    {
        ArgumentNullException.ThrowIfNull(layoutHost);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(composer);
        ArgumentNullException.ThrowIfNull(fileStore);
        ArgumentNullException.ThrowIfNull(mutationService);
        ArgumentException.ThrowIfNullOrWhiteSpace(layoutPath);
        ArgumentNullException.ThrowIfNull(layout);

        _layoutHost = layoutHost;
        _registry = registry;
        _composer = composer;
        _fileStore = fileStore;
        _mutationService = mutationService;
        _layoutPath = layoutPath;
        _layout = layout;

        RefreshRuntime();
    }

    public void Update(DashboardRuntimeState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _layoutHost.Update(state);
    }

    public IReadOnlyList<DashboardCatalogItem> GetCatalog() => _registry.GetCatalog();

    public IReadOnlyList<LayoutEditorItem> GetLayoutItems()
    {
        return _layout.Items
            .Select(x =>
            {
                var definition = _registry.Get(x.TypeId);
                return new LayoutEditorItem(x.Id, x.TypeId, definition.DisplayName);
            })
            .OrderBy(x => x.DisplayName)
            .ToList();
    }

    public Guid? GetSelectedItemId() => _selectedItemId;

    public void SelectItem(Guid? itemId)
    {
        _selectedItemId = itemId;
        _layoutHost.SelectItem(itemId);
    }

    public bool AddItem(string typeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);

        var offset = _layout.Items.Count * 18;
        _layout = _mutationService.AddItem(_layout, typeId, 40 + offset, 72 + offset, 160, 60, 20);

        _selectedItemId = _layout.Items.LastOrDefault()?.Id;
        RefreshRuntime();
        return _selectedItemId is not null;
    }

    public bool MoveSelected(double deltaX, double deltaY)
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        _layout = _mutationService.MoveItem(_layout, _selectedItemId.Value, deltaX, deltaY);
        RefreshRuntime();
        return true;
    }

    public bool ResizeSelected(double deltaWidth, double deltaHeight)
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        _layout = _mutationService.ResizeItem(_layout, _selectedItemId.Value, deltaWidth, deltaHeight);
        RefreshRuntime();
        return true;
    }

    public bool DeleteSelected()
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        _layout = _mutationService.DeleteItem(_layout, _selectedItemId.Value);
        _selectedItemId = null;
        RefreshRuntime();
        return true;
    }

    public void SaveLayout() => _fileStore.Save(_layoutPath, _layout);

    public void ReloadLayout()
    {
        _layout = _fileStore.Load(_layoutPath);

        if (_selectedItemId is not null && _layout.Items.All(x => x.Id != _selectedItemId.Value))
        {
            _selectedItemId = null;
        }

        RefreshRuntime();
    }

    private void RefreshRuntime()
    {
        var runtimeItems = _composer.Compose(_layout);
        _layoutHost.Load(runtimeItems);
        _layoutHost.SelectItem(_selectedItemId);
    }
}
