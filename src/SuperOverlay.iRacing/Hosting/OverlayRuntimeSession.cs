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

        RefreshRuntime();
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
        return _registry.GetCatalog()
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

    public Guid? GetSelectedItemId() => _selectedItemId;

    public void SelectItem(Guid? itemId)
    {
        _selectedItemId = itemId;
        _layoutHost.SelectItem(itemId);
    }

    public Guid? HitTestItemId(object? hitSource)
    {
        return hitSource is null
            ? null
            : _layoutHost.HitTestItem(hitSource as System.Windows.DependencyObject)?.Item.Id;
    }

    public bool IsResizeHandleHit(object? hitSource, Guid itemId)
    {
        return hitSource is not null
            && _layoutHost.IsResizeHandleHit(hitSource as System.Windows.DependencyObject, itemId);
    }

    public bool AddItem(string typeId)
    {
        var changed = _mutationService.AddItem(ref _layout, typeId);

        if (changed)
        {
            RefreshRuntime();
        }

        return changed;
    }

    public bool DeleteSelected()
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        var changed = _mutationService.DeleteItem(ref _layout, _selectedItemId.Value);

        if (changed)
        {
            _selectedItemId = null;
            RefreshRuntime();
        }

        return changed;
    }

    public bool DuplicateSelected()
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        var changed = _mutationService.DuplicateItem(ref _layout, _selectedItemId.Value, out var newItemId);
        if (!changed)
        {
            return false;
        }

        _selectedItemId = newItemId;
        RefreshRuntime();
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
            SyncPlacementToRuntime(_selectedItemId.Value);
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

        var finalX = targetX;
        var finalY = targetY;
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
            SyncPlacementToRuntime(_selectedItemId.Value);
        }

        return new LayoutMoveResult(changed, snapX, snapY);
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
            SyncPlacementToRuntime(_selectedItemId.Value);
        }

        return changed;
    }

    public LayoutMoveResult ResizeSelectedWithSnap(
        double deltaWidth,
        double deltaHeight,
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

        var targetWidth = placement.Width + deltaWidth;
        var targetHeight = placement.Height + deltaHeight;

        var finalWidth = targetWidth;
        var finalHeight = targetHeight;
        double? snapX = null;
        double? snapY = null;

        if (_snappingEnabled && !bypassSnap)
        {
            var snapped = _snapService.SnapResize(
                _layout,
                _selectedItemId.Value,
                targetWidth,
                targetHeight,
                canvasWidth,
                canvasHeight);

            finalWidth = snapped.Width;
            finalHeight = snapped.Height;
            snapX = snapped.SnapX;
            snapY = snapped.SnapY;
        }

        var changed = _mutationService.ResizeItemTo(
            ref _layout,
            _selectedItemId.Value,
            finalWidth,
            finalHeight);

        if (changed)
        {
            SyncPlacementToRuntime(_selectedItemId.Value);
        }

        return new LayoutMoveResult(changed, snapX, snapY);
    }

    public void EndDrag()
    {
        _snapService.EndDrag();
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

        RefreshRuntime();
    }

    private void RefreshRuntime()
    {
        var runtimeItems = _composer.Compose(_layout);
        _layoutHost.Load(runtimeItems);
        _layoutHost.SelectItem(_selectedItemId);
    }

    private void SyncPlacementToRuntime(Guid itemId)
    {
        var placement = _layout.Placements.FirstOrDefault(x => x.ItemId == itemId);
        if (placement is null)
        {
            return;
        }

        _layoutHost.TryUpdatePlacement(itemId, placement);
    }
}
