using SuperOverlay.Dashboards.Registry;
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
    private readonly LayoutSnapService _snapService = new();
    private readonly string _layoutPath;

    private LayoutDocument _layout;
    private Guid? _selectedItemId;
    private bool _snappingEnabled = true;

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

        ReloadRuntime();
    }

    public void Update(object runtimeState)
    {
        _layoutHost.Update(runtimeState);
    }

    public void SetSnappingEnabled(bool enabled)
    {
        _snappingEnabled = enabled;
    }

    public IReadOnlyList<DashboardCatalogItem> GetCatalog()
    {
        return _registry
            .GetCatalog()
            .OrderBy(x => x.DisplayName)
            .ToList();
    }

    public IReadOnlyList<LayoutEditorItem> GetLayoutItems()
    {
        return _layout.Items
            .Select(item =>
            {
                var definition = _registry.Get(item.TypeId);

                return new LayoutEditorItem(
                    item.Id,
                    item.TypeId,
                    definition.DisplayName);
            })
            .ToList();
    }

    public Guid? GetSelectedItemId()
    {
        return _selectedItemId;
    }

    public void SelectItem(Guid? itemId)
    {
        _selectedItemId = itemId;
    }

    public LayoutDocument GetCurrentLayout()
    {
        return _layout;
    }

    public bool AddItem(string typeId)
    {
        var changed = _mutationService.AddItem(ref _layout, typeId);

        if (changed)
        {
            ReloadRuntime();
        }

        return changed;
    }

    public bool DuplicateSelected()
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        var item = _layout.Items.FirstOrDefault(x => x.Id == _selectedItemId.Value);
        var placement = _layout.Placements.FirstOrDefault(x => x.ItemId == _selectedItemId.Value);

        if (item is null || placement is null)
        {
            return false;
        }

        var duplicatedId = Guid.NewGuid();

        var duplicatedItem = new LayoutItemInstance(
            duplicatedId,
            item.TypeId,
            item.Settings);

        var duplicatedPlacement = new LayoutItemPlacement(
            duplicatedId,
            placement.X + 20,
            placement.Y + 20,
            placement.Width,
            placement.Height,
            placement.ZIndex);

        var editor = new LayoutDocumentEditor();
        _layout = editor.AddItem(_layout, duplicatedItem, duplicatedPlacement);
        _selectedItemId = duplicatedId;

        ReloadRuntime();
        return true;
    }

    public bool MoveSelected(double deltaX, double deltaY)
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        var changed = _mutationService.MoveItem(ref _layout, _selectedItemId.Value, deltaX, deltaY);

        if (changed)
        {
            ReloadRuntime();
        }

        return changed;
    }

    public LayoutMoveResult MoveSelectedWithSnap(
        double deltaX,
        double deltaY,
        double canvasWidth,
        double canvasHeight,
        bool bypassSnap)
    {
        if (_selectedItemId is null)
        {
            return new LayoutMoveResult(false, null, null);
        }

        var placement = _layout.Placements.FirstOrDefault(x => x.ItemId == _selectedItemId.Value);
        if (placement is null)
        {
            return new LayoutMoveResult(false, null, null);
        }

        var targetX = placement.X + deltaX;
        var targetY = placement.Y + deltaY;

        double finalX = targetX;
        double finalY = targetY;
        double? snapX = null;
        double? snapY = null;

        if (_snappingEnabled && !bypassSnap)
        {
            var snapped = _snapService.SnapPosition(
                _layout,
                _selectedItemId.Value,
                targetX,
                targetY,
                canvasWidth,
                canvasHeight);

            finalX = snapped.X;
            finalY = snapped.Y;
            snapX = snapped.SnapX;
            snapY = snapped.SnapY;
        }

        var changed = _mutationService.MoveItem(
            ref _layout,
            _selectedItemId.Value,
            finalX - placement.X,
            finalY - placement.Y);

        if (changed)
        {
            ReloadRuntime();
        }

        return new LayoutMoveResult(changed, snapX, snapY);
    }

    public void EndDrag()
    {
        _snapService.EndDrag();
    }

    public bool ResizeSelected(double deltaWidth, double deltaHeight)
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        var changed = _mutationService.ResizeItem(
            ref _layout,
            _selectedItemId.Value,
            deltaWidth,
            deltaHeight);

        if (changed)
        {
            ReloadRuntime();
        }

        return changed;
    }

    public bool DeleteSelected()
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        var itemId = _selectedItemId.Value;
        var changed = _mutationService.DeleteItem(ref _layout, itemId);

        if (changed)
        {
            _selectedItemId = null;
            ReloadRuntime();
        }

        return changed;
    }

    public void SaveLayout()
    {
        _fileStore.Save(_layoutPath, _layout);
    }

    public void ReloadLayout()
    {
        _layout = _fileStore.Load(_layoutPath);

        if (_selectedItemId is not null &&
            !_layout.Items.Any(x => x.Id == _selectedItemId.Value))
        {
            _selectedItemId = null;
        }

        ReloadRuntime();
    }

    private void ReloadRuntime()
    {
        var runtimeItems = _composer.Compose(_layout);
        _layoutHost.Load(runtimeItems);
    }
}