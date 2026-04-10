using SuperOverlay.Dashboards.Items.DecorativePanel;
using SuperOverlay.Dashboards.Items.Gear;
using SuperOverlay.Dashboards.Items.ShiftLeds;
using SuperOverlay.Dashboards.Items.Speed;
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
    private readonly OverlayShellMode _shellMode;

    private LayoutDocument _layout;
    private Guid? _selectedItemId;
    private HashSet<Guid> _selectedItemIds = new();
    private bool _snappingEnabled = true;
    private LayoutDocument? _clipboardLayout;
    private IReadOnlyList<Guid> _clipboardItemIds = Array.Empty<Guid>();
    private int _pasteSequence;

    public OverlayRuntimeSession(
        LayoutHost layoutHost,
        DashboardRegistry registry,
        LayoutRuntimeComposer composer,
        LayoutFileStore fileStore,
        LayoutMutationService mutationService,
        string layoutPath,
        LayoutDocument layout,
        OverlayShellMode shellMode)
    {
        _layoutHost = layoutHost ?? throw new ArgumentNullException(nameof(layoutHost));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _composer = composer ?? throw new ArgumentNullException(nameof(composer));
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _mutationService = mutationService ?? throw new ArgumentNullException(nameof(mutationService));
        _layoutPath = string.IsNullOrWhiteSpace(layoutPath) ? throw new ArgumentException("Layout path is required.", nameof(layoutPath)) : layoutPath;
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _shellMode = shellMode;

        RefreshRuntime();
    }

    public void Update(object runtimeState) => _layoutHost.Update(runtimeState);
    public void SetSnappingEnabled(bool enabled) => _snappingEnabled = enabled;
    public Guid? GetSelectedItemId() => _selectedItemId;
    public IReadOnlyList<Guid> GetSelectedItemIds() => _selectedItemIds.ToList();
    public bool HasSelection => _selectedItemIds.Count > 0;
    public bool CanPaste => _clipboardLayout is not null && _clipboardItemIds.Count > 0;
    public bool HasLockedSelection => _selectedItemIds.Any(id => _layout.Items.Any(x => x.Id == id && x.IsLocked));

    public IReadOnlyList<DashboardCatalogItem> GetCatalog() => _registry.GetCatalog().OrderBy(x => x.DisplayName).ToList();

    public LayoutSelectedItemProperties? GetSelectedItemProperties()
    {
        if (_selectedItemId is null)
        {
            return null;
        }

        var item = _layout.Items.FirstOrDefault(x => x.Id == _selectedItemId.Value);
        var placement = _layout.Placements.FirstOrDefault(x => x.ItemId == _selectedItemId.Value);
        if (item is null || placement is null)
        {
            return null;
        }

        var definition = _registry.Get(item.TypeId);
        var isGrouped = _layout.Links.Any(x => x.SourceItemId == item.Id || x.TargetItemId == item.Id);
        return new LayoutSelectedItemProperties(
            item.Id,
            item.TypeId,
            definition.DisplayName,
            placement.X,
            placement.Y,
            placement.Width,
            placement.Height,
            placement.ZIndex,
            item.IsLocked,
            _selectedItemIds.Count,
            isGrouped);
    }


    public WidgetCornerSettings? GetSelectedWidgetCornerSettings()
    {
        if (_selectedItemId is null)
        {
            return null;
        }

        var item = _layout.Items.FirstOrDefault(x => x.Id == _selectedItemId.Value);
        if (item is null)
        {
            return null;
        }

        return item.TypeId switch
        {
            "dashboard.shift-leds" => _registry.Get(item.TypeId).MaterializeSettings(item.Settings) is ShiftLedDashboardSettings shift
                ? new WidgetCornerSettings(shift.CornerTopLeft, shift.CornerTopRight, shift.CornerBottomRight, shift.CornerBottomLeft)
                : null,
            "dashboard.gear" => _registry.Get(item.TypeId).MaterializeSettings(item.Settings) is GearDashboardSettings gear
                ? new WidgetCornerSettings(gear.CornerTopLeft, gear.CornerTopRight, gear.CornerBottomRight, gear.CornerBottomLeft)
                : null,
            "dashboard.speed" => _registry.Get(item.TypeId).MaterializeSettings(item.Settings) is SpeedDashboardSettings speed
                ? new WidgetCornerSettings(speed.CornerTopLeft, speed.CornerTopRight, speed.CornerBottomRight, speed.CornerBottomLeft)
                : null,
            "dashboard.decorative-panel" => _registry.Get(item.TypeId).MaterializeSettings(item.Settings) is DecorativePanelDashboardSettings panel
                ? new WidgetCornerSettings(panel.CornerTopLeft, panel.CornerTopRight, panel.CornerBottomRight, panel.CornerBottomLeft)
                : null,
            _ => null
        };
    }

    public bool UpdateSelectedWidgetCornerSettings(double topLeft, double topRight, double bottomRight, double bottomLeft)
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        var item = _layout.Items.FirstOrDefault(x => x.Id == _selectedItemId.Value);
        if (item is null)
        {
            return false;
        }

        object? updatedSettings = item.TypeId switch
        {
            "dashboard.shift-leds" => _registry.Get(item.TypeId).MaterializeSettings(item.Settings) is ShiftLedDashboardSettings shift
                ? shift with
                {
                    CornerTopLeft = topLeft,
                    CornerTopRight = topRight,
                    CornerBottomRight = bottomRight,
                    CornerBottomLeft = bottomLeft
                }
                : null,
            "dashboard.gear" => _registry.Get(item.TypeId).MaterializeSettings(item.Settings) is GearDashboardSettings gear
                ? gear with
                {
                    CornerTopLeft = topLeft,
                    CornerTopRight = topRight,
                    CornerBottomRight = bottomRight,
                    CornerBottomLeft = bottomLeft
                }
                : null,
            "dashboard.speed" => _registry.Get(item.TypeId).MaterializeSettings(item.Settings) is SpeedDashboardSettings speed
                ? speed with
                {
                    CornerTopLeft = topLeft,
                    CornerTopRight = topRight,
                    CornerBottomRight = bottomRight,
                    CornerBottomLeft = bottomLeft
                }
                : null,
            "dashboard.decorative-panel" => _registry.Get(item.TypeId).MaterializeSettings(item.Settings) is DecorativePanelDashboardSettings panel
                ? panel with
                {
                    CornerTopLeft = topLeft,
                    CornerTopRight = topRight,
                    CornerBottomRight = bottomRight,
                    CornerBottomLeft = bottomLeft
                }
                : null,
            _ => null
        };

        if (updatedSettings is null)
        {
            return false;
        }

        var changed = _mutationService.UpdateItemSettings(ref _layout, _selectedItemId.Value, updatedSettings);
        if (!changed)
        {
            return false;
        }

        RefreshRuntime();
        SyncSelectionState();
        return true;
    }

    public ShiftLedDashboardSettings? GetSelectedShiftLedSettings()
    {
        if (_selectedItemId is null)
        {
            return null;
        }

        var item = _layout.Items.FirstOrDefault(x => x.Id == _selectedItemId.Value);
        if (item is null || item.TypeId != "dashboard.shift-leds")
        {
            return null;
        }

        return _registry.Get(item.TypeId).MaterializeSettings(item.Settings) as ShiftLedDashboardSettings;
    }

    public bool UpdateSelectedShiftLedSettings(ShiftLedDashboardSettings settings)
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        var item = _layout.Items.FirstOrDefault(x => x.Id == _selectedItemId.Value);
        if (item is null || item.TypeId != "dashboard.shift-leds")
        {
            return false;
        }

        var changed = _mutationService.UpdateItemSettings(ref _layout, _selectedItemId.Value, settings);
        if (!changed)
        {
            return false;
        }

        RefreshRuntime();
        SyncSelectionState();
        return true;
    }

    public DecorativePanelDashboardSettings? GetSelectedDecorativePanelSettings()
    {
        if (_selectedItemId is null)
        {
            return null;
        }

        var item = _layout.Items.FirstOrDefault(x => x.Id == _selectedItemId.Value);
        if (item is null || item.TypeId != "dashboard.decorative-panel")
        {
            return null;
        }

        return _registry.Get(item.TypeId).MaterializeSettings(item.Settings) as DecorativePanelDashboardSettings;
    }

    public bool UpdateSelectedDecorativePanelSettings(DecorativePanelDashboardSettings settings)
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        var item = _layout.Items.FirstOrDefault(x => x.Id == _selectedItemId.Value);
        if (item is null || item.TypeId != "dashboard.decorative-panel")
        {
            return false;
        }

        var changed = _mutationService.UpdateItemSettings(ref _layout, _selectedItemId.Value, settings);
        if (!changed)
        {
            return false;
        }

        RefreshRuntime();
        SyncSelectionState();
        return true;
    }

    public bool UpdateSelectedItemProperties(double x, double y, double width, double height, int zIndex, bool isLocked)
    {
        if (_selectedItemId is null)
        {
            return false;
        }

        var changed = _mutationService.UpdateItemProperties(ref _layout, _selectedItemId.Value, x, y, width, height, zIndex, isLocked);
        if (!changed)
        {
            return false;
        }

        RefreshRuntime();
        SyncSelectionState();
        return true;
    }

    public IReadOnlyList<LayoutEditorItem> GetLayoutItems()
    {
        return _layout.Items
            .Select(item =>
            {
                var definition = _registry.Get(item.TypeId);
                var isGrouped = _layout.Links.Any(x => x.SourceItemId == item.Id || x.TargetItemId == item.Id);
                return new LayoutEditorItem(item.Id, item.TypeId, definition.DisplayName, isGrouped, item.IsLocked);
            })
            .ToList();
    }

    public void SelectItem(Guid? itemId)
    {
        if (itemId is null)
        {
            _selectedItemId = null;
            _selectedItemIds = new HashSet<Guid>();
            SyncSelectionState();
            return;
        }

        SelectItems(GetSelectionUnitItemIds(itemId.Value), itemId.Value);
    }

    public void SelectItems(IEnumerable<Guid> itemIds, Guid? primaryItemId = null)
    {
        var normalized = NormalizeSelection(itemIds, primaryItemId);
        _selectedItemIds = normalized.ItemIds;
        _selectedItemId = normalized.PrimaryItemId;
        SyncSelectionState();
    }

    public void ToggleItemSelection(Guid itemId)
    {
        var targetUnit = GetSelectionUnitItemIds(itemId).ToHashSet();
        var currentGroupedIds = GetSelectedGroupedItemIds();
        var targetIsGrouped = targetUnit.Count > 1;

        if (targetIsGrouped)
        {
            if (currentGroupedIds.SetEquals(targetUnit))
            {
                SelectItems(Array.Empty<Guid>());
            }
            else
            {
                SelectItems(targetUnit, itemId);
            }

            return;
        }

        if (currentGroupedIds.Count > 0 && !currentGroupedIds.Contains(itemId))
        {
            SelectItems(targetUnit, itemId);
            return;
        }

        if (_selectedItemIds.Contains(itemId))
        {
            _selectedItemIds.Remove(itemId);
            if (_selectedItemId == itemId)
            {
                _selectedItemId = _selectedItemIds.FirstOrDefault();
                if (_selectedItemIds.Count == 0)
                {
                    _selectedItemId = null;
                }
            }
        }
        else
        {
            _selectedItemIds.Add(itemId);
            _selectedItemId = itemId;
        }

        SyncSelectionState();
    }

    public void SetPrimarySelection(Guid itemId)
    {
        SelectItems(GetSelectionUnitItemIds(itemId), itemId);
    }


    private (HashSet<Guid> ItemIds, Guid? PrimaryItemId) NormalizeSelection(IEnumerable<Guid> itemIds, Guid? primaryItemId)
    {
        var requested = itemIds.Distinct().ToList();
        if (requested.Count == 0)
        {
            return (new HashSet<Guid>(), null);
        }

        var groupedComponents = requested
            .Select(GetLinkedGroupItemIds)
            .Where(ids => ids.Count > 1)
            .Distinct(LinkedGroupComparer.Instance)
            .ToList();

        if (groupedComponents.Count > 1)
        {
            var preferredGroup = primaryItemId is not null
                ? GetLinkedGroupItemIds(primaryItemId.Value)
                : groupedComponents[0];

            return (preferredGroup.ToHashSet(), primaryItemId is not null && preferredGroup.Contains(primaryItemId.Value) ? primaryItemId.Value : preferredGroup.First());
        }

        var normalizedIds = new HashSet<Guid>();
        foreach (var id in requested)
        {
            foreach (var expandedId in GetSelectionUnitItemIds(id))
            {
                normalizedIds.Add(expandedId);
            }
        }

        Guid? resolvedPrimary = null;
        if (primaryItemId is not null && normalizedIds.Contains(primaryItemId.Value))
        {
            resolvedPrimary = primaryItemId.Value;
        }
        else if (normalizedIds.Count > 0)
        {
            resolvedPrimary = normalizedIds.First();
        }

        return (normalizedIds, resolvedPrimary);
    }

    private IReadOnlyCollection<Guid> GetSelectionUnitItemIds(Guid itemId)
    {
        var linked = GetLinkedGroupItemIds(itemId);
        return linked.Count > 1 ? linked : new[] { itemId };
    }

    private HashSet<Guid> GetSelectedGroupedItemIds()
    {
        return _selectedItemIds
            .Select(GetLinkedGroupItemIds)
            .Where(ids => ids.Count > 1)
            .SelectMany(ids => ids)
            .ToHashSet();
    }

    private sealed class LinkedGroupComparer : IEqualityComparer<IReadOnlyCollection<Guid>>
    {
        public static LinkedGroupComparer Instance { get; } = new();

        public bool Equals(IReadOnlyCollection<Guid>? x, IReadOnlyCollection<Guid>? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null || x.Count != y.Count)
            {
                return false;
            }

            return x.ToHashSet().SetEquals(y);
        }

        public int GetHashCode(IReadOnlyCollection<Guid> obj)
        {
            var hash = 17;
            foreach (var id in obj.Order())
            {
                hash = (hash * 31) + id.GetHashCode();
            }

            return hash;
        }
    }

    public Guid? HitTestItemId(object? hitSource)
    {
        return hitSource is null ? null : _layoutHost.HitTestItem(hitSource as System.Windows.DependencyObject)?.Item.Id;
    }

    public bool IsResizeHandleHit(object? hitSource, Guid itemId)
    {
        return hitSource is not null && _layoutHost.IsResizeHandleHit(hitSource as System.Windows.DependencyObject, itemId);
    }

    public bool IsLocked(Guid itemId) => _layout.Items.Any(x => x.Id == itemId && x.IsLocked);

    public IReadOnlyList<Guid> GetItemsInSelectionRect(double x, double y, double width, double height, bool requireFullContainment = false)
    {
        var left = Math.Min(x, x + width);
        var top = Math.Min(y, y + height);
        var right = Math.Max(x, x + width);
        var bottom = Math.Max(y, y + height);

        return _layout.Placements
            .Select(ResolvePlacementForShell)
            .Where(p =>
            {
                var itemLeft = p.X;
                var itemTop = p.Y;
                var itemRight = p.X + p.Width;
                var itemBottom = p.Y + p.Height;

                return requireFullContainment
                    ? itemLeft >= left && itemTop >= top && itemRight <= right && itemBottom <= bottom
                    : !(itemRight < left || itemBottom < top || itemLeft > right || itemTop > bottom);
            })
            .OrderBy(p => p.ZIndex)
            .Select(p => p.ItemId)
            .ToList();
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
        if (_selectedItemIds.Count == 0)
        {
            return false;
        }

        var changed = _mutationService.DeleteItems(ref _layout, _selectedItemIds);
        if (changed)
        {
            _selectedItemId = null;
            _selectedItemIds.Clear();
            RefreshRuntime();
        }

        return changed;
    }

    public bool DuplicateSelected()
    {
        if (_selectedItemIds.Count == 0)
        {
            return false;
        }

        var changed = _mutationService.DuplicateItems(ref _layout, _selectedItemIds, out var newItemIds);
        if (!changed)
        {
            return false;
        }

        RefreshRuntime();
        SelectItems(newItemIds, newItemIds.LastOrDefault());
        return true;
    }

    public bool GroupSelectedItems()
    {
        if (_selectedItemIds.Count < 2)
        {
            return false;
        }

        var orderedIds = _selectedItemIds.ToList();
        var anchorId = _selectedItemId is not null && _selectedItemIds.Contains(_selectedItemId.Value) ? _selectedItemId.Value : orderedIds[0];
        var changed = false;
        foreach (var itemId in orderedIds.Where(x => x != anchorId))
        {
            changed |= _mutationService.GroupItems(ref _layout, anchorId, itemId);
        }

        if (changed)
        {
            RefreshRuntime();
            SelectItems(orderedIds, anchorId);
        }

        return changed;
    }

    public bool UngroupSelected()
    {
        if (_selectedItemIds.Count == 0)
        {
            return false;
        }

        var changed = false;
        foreach (var itemId in _selectedItemIds.ToList())
        {
            changed |= _mutationService.UngroupItem(ref _layout, itemId);
        }

        if (changed)
        {
            RefreshRuntime();
            SyncSelectionState();
        }

        return changed;
    }

    public bool SetLockSelected(bool isLocked)
    {
        if (_selectedItemIds.Count == 0)
        {
            return false;
        }

        var changed = _mutationService.ToggleLockItems(ref _layout, _selectedItemIds, isLocked);
        if (changed)
        {
            RefreshRuntime();
            SyncSelectionState();
        }

        return changed;
    }

    public bool CopySelected()
    {
        if (_selectedItemIds.Count == 0)
        {
            return false;
        }

        _clipboardLayout = _layout;
        _clipboardItemIds = _selectedItemIds.OrderBy(x => x).ToList();
        _pasteSequence = 0;
        return true;
    }

    public bool PasteClipboard()
    {
        if (_clipboardLayout is null || _clipboardItemIds.Count == 0)
        {
            return false;
        }

        _pasteSequence++;
        var offset = 24 * _pasteSequence;
        var changed = _mutationService.PasteItemsFromLayout(ref _layout, _clipboardLayout, _clipboardItemIds, offset, offset, out var newItemIds);
        if (!changed)
        {
            return false;
        }

        RefreshRuntime();
        SelectItems(newItemIds, newItemIds.LastOrDefault());
        return true;
    }

    public bool BringToFrontSelected() => ReorderSelected((all, selected) =>
    {
        var selectedIds = selected.Select(x => x.ItemId).ToHashSet();
        var ordered = all.Where(x => !selectedIds.Contains(x.ItemId)).Concat(selected).ToList();
        return BuildZMap(ordered);
    });

    public bool SendToBackSelected() => ReorderSelected((all, selected) =>
    {
        var selectedIds = selected.Select(x => x.ItemId).ToHashSet();
        var ordered = selected.Concat(all.Where(x => !selectedIds.Contains(x.ItemId))).ToList();
        return BuildZMap(ordered);
    });

    public bool BringForwardSelected() => ReorderSelected((all, selected) =>
    {
        var list = all.ToList();
        var selectedIds = selected.Select(x => x.ItemId).ToHashSet();
        for (var i = list.Count - 2; i >= 0; i--)
        {
            if (selectedIds.Contains(list[i].ItemId) && !selectedIds.Contains(list[i + 1].ItemId))
            {
                (list[i], list[i + 1]) = (list[i + 1], list[i]);
            }
        }
        return BuildZMap(list);
    });

    public bool SendBackwardSelected() => ReorderSelected((all, selected) =>
    {
        var list = all.ToList();
        var selectedIds = selected.Select(x => x.ItemId).ToHashSet();
        for (var i = 1; i < list.Count; i++)
        {
            if (selectedIds.Contains(list[i].ItemId) && !selectedIds.Contains(list[i - 1].ItemId))
            {
                (list[i], list[i - 1]) = (list[i - 1], list[i]);
            }
        }
        return BuildZMap(list);
    });

    public LayoutMoveResult MoveSelectedWithSnap(double deltaX, double deltaY, double canvasWidth, double canvasHeight, bool bypassSnap)
    {
        if (_selectedItemId is null)
        {
            return new LayoutMoveResult(false, null, null);
        }

        if (IsRuntimeShell)
        {
            var runtimeMoveIds = GetActiveMoveItemIds().Where(id => !IsLocked(id)).ToList();
            if (runtimeMoveIds.Count == 0)
            {
                return new LayoutMoveResult(false, null, null);
            }

            var runtimePlacements = _layout.Placements
                .Where(x => runtimeMoveIds.Contains(x.ItemId))
                .Select(ResolvePlacementForShell)
                .ToList();

            if (runtimePlacements.Count == 0)
            {
                return new LayoutMoveResult(false, null, null);
            }

            var runtimeBounds = GetBounds(runtimePlacements);
            var targetX = runtimeBounds.X + deltaX;
            var targetY = runtimeBounds.Y + deltaY;
            var finalX = Math.Clamp(targetX, 0, Math.Max(0, canvasWidth - runtimeBounds.Width));
            var finalY = Math.Clamp(targetY, 0, Math.Max(0, canvasHeight - runtimeBounds.Height));
            var deltaAppliedX = finalX - runtimeBounds.X;
            var deltaAppliedY = finalY - runtimeBounds.Y;

            var changed = _mutationService.MoveItemsRuntimeDeltaBy(ref _layout, runtimeMoveIds, deltaAppliedX, deltaAppliedY);
            if (changed)
            {
                SyncPlacementsToRuntime(runtimeMoveIds);
            }

            return new LayoutMoveResult(changed, null, null);
        }

        var groupItemIds = GetActiveMoveItemIds();
        var movableItemIds = groupItemIds.Where(id => !IsLocked(id)).ToList();
        if (movableItemIds.Count == 0)
        {
            return new LayoutMoveResult(false, null, null);
        }

        var groupPlacements = _layout.Placements.Where(x => movableItemIds.Contains(x.ItemId)).Select(ResolvePlacementForShell).ToList();
        if (groupPlacements.Count == 0)
        {
            return new LayoutMoveResult(false, null, null);
        }

        var groupBounds = GetBounds(groupPlacements);
        var targetGroupX = groupBounds.X + deltaX;
        var targetGroupY = groupBounds.Y + deltaY;
        var finalGroupX = targetGroupX;
        var finalGroupY = targetGroupY;
        double? snapX = null;
        double? snapY = null;

        if (_snappingEnabled && !bypassSnap)
        {
            var snapped = _snapService.SnapGroupPosition(_layout, movableItemIds, targetGroupX, targetGroupY, groupBounds.Width, groupBounds.Height, canvasWidth, canvasHeight);
            finalGroupX = snapped.X;
            finalGroupY = snapped.Y;
            snapX = snapped.SnapX;
            snapY = snapped.SnapY;
        }

        var appliedDeltaX = finalGroupX - groupBounds.X;
        var appliedDeltaY = finalGroupY - groupBounds.Y;
        var changedGroup = _mutationService.MoveItemsBy(ref _layout, movableItemIds, appliedDeltaX, appliedDeltaY);
        if (changedGroup)
        {
            SyncPlacementsToRuntime(movableItemIds);
        }

        return new LayoutMoveResult(changedGroup, snapX, snapY);
    }

    public LayoutMoveResult ResizeSelectedWithSnap(double deltaWidth, double deltaHeight, double canvasWidth, double canvasHeight, bool bypassSnap)
    {
        if (_selectedItemId is null || IsLocked(_selectedItemId.Value))
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
            var snapped = _snapService.SnapResize(_layout, _selectedItemId.Value, targetWidth, targetHeight, canvasWidth, canvasHeight);
            finalWidth = snapped.Width;
            finalHeight = snapped.Height;
            snapX = snapped.SnapX;
            snapY = snapped.SnapY;
        }

        var changed = _mutationService.ResizeItemTo(ref _layout, _selectedItemId.Value, finalWidth, finalHeight);
        if (changed)
        {
            SyncPlacementToRuntime(_selectedItemId.Value);
        }

        return new LayoutMoveResult(changed, snapX, snapY);
    }

    public void EndDrag() => _snapService.EndDrag();
    public void SaveLayout() => _fileStore.Save(_layoutPath, _layout);

    public void ReloadLayout()
    {
        _layout = _fileStore.Load(_layoutPath);
        var validIds = _layout.Items.Select(x => x.Id).ToHashSet();
        _selectedItemIds.RemoveWhere(x => !validIds.Contains(x));
        if (_selectedItemId is not null && !validIds.Contains(_selectedItemId.Value))
        {
            _selectedItemId = _selectedItemIds.FirstOrDefault();
            if (_selectedItemIds.Count == 0)
            {
                _selectedItemId = null;
            }
        }

        RefreshRuntime();
    }

    private bool ReorderSelected(Func<IReadOnlyList<LayoutItemPlacement>, IEnumerable<LayoutItemPlacement>, Dictionary<Guid, int>> reorder)
    {
        if (_selectedItemIds.Count == 0)
        {
            return false;
        }

        var changed = _mutationService.SetZIndex(ref _layout, _selectedItemIds, reorder);
        if (changed)
        {
            RefreshRuntime();
            SyncSelectionState();
        }

        return changed;
    }

    private void RefreshRuntime()
    {
        var runtimeItems = _composer.Compose(_layout);
        _layoutHost.Load(runtimeItems);
        SyncSelectionState();
    }

    private void SyncSelectionState()
    {
        _layoutHost.SetSelectedItems(_selectedItemId, _selectedItemIds);
        _layoutHost.SetGroupHighlight(GetHighlightedGroupItemIds(_selectedItemId));
    }

    private IReadOnlyList<Guid> GetActiveMoveItemIds()
    {
        if (_selectedItemIds.Count > 1)
        {
            return _selectedItemIds.ToList();
        }

        return _selectedItemId is null ? Array.Empty<Guid>() : GetLinkedGroupItemIds(_selectedItemId.Value);
    }

    private IReadOnlyList<Guid> GetHighlightedGroupItemIds(Guid? itemId)
    {
        if (itemId is null)
        {
            return Array.Empty<Guid>();
        }

        var groupIds = GetLinkedGroupItemIds(itemId.Value);
        return groupIds.Count <= 1 ? Array.Empty<Guid>() : groupIds;
    }

    private IReadOnlyList<Guid> GetLinkedGroupItemIds(Guid anchorItemId)
    {
        var visited = new HashSet<Guid> { anchorItemId };
        var queue = new Queue<Guid>();
        queue.Enqueue(anchorItemId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in _layout.Links
                         .Where(x => x.SourceItemId == current || x.TargetItemId == current)
                         .Select(x => x.SourceItemId == current ? x.TargetItemId : x.SourceItemId))
            {
                if (visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return visited.ToList();
    }

    private void SyncPlacementsToRuntime(IReadOnlyCollection<Guid> itemIds)
    {
        foreach (var itemId in itemIds)
        {
            SyncPlacementToRuntime(itemId);
        }
    }

    private void SyncPlacementToRuntime(Guid itemId)
    {
        var placement = _layout.Placements.FirstOrDefault(x => x.ItemId == itemId);
        if (placement is null)
        {
            return;
        }

        _layoutHost.TryUpdatePlacement(itemId, ResolvePlacementForShell(placement));
    }

    private void SyncAllPlacementsToRuntime()
    {
        foreach (var placement in _layout.Placements)
        {
            _layoutHost.TryUpdatePlacement(placement.ItemId, ResolvePlacementForShell(placement));
        }
    }

    private static (double X, double Y, double Width, double Height) GetBounds(IReadOnlyList<LayoutItemPlacement> placements)
    {
        var left = placements.Min(x => x.X);
        var top = placements.Min(x => x.Y);
        var right = placements.Max(x => x.X + x.Width);
        var bottom = placements.Max(x => x.Y + x.Height);
        return (left, top, right - left, bottom - top);
    }


    private bool IsRuntimeShell => _shellMode != OverlayShellMode.Editor;

    private LayoutItemPlacement ResolvePlacementForShell(LayoutItemPlacement placement)
    {
        return LayoutPlacementResolver.ResolveForShell(placement, _layout.Canvas, _shellMode);
    }

    private static Dictionary<Guid, int> BuildZMap(IReadOnlyList<LayoutItemPlacement> orderedPlacements)
    {
        var map = new Dictionary<Guid, int>();
        for (var i = 0; i < orderedPlacements.Count; i++)
        {
            map[orderedPlacements[i].ItemId] = i;
        }
        return map;
    }
}
