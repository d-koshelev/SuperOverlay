using System.Windows;
using SuperOverlay.Dashboards.Items.DecorativePanel;
using SuperOverlay.Dashboards.Items.Gear;
using SuperOverlay.Dashboards.Items.ShiftLeds;
using SuperOverlay.Dashboards.Items.Speed;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.LayoutBuilder.Layout;
using SuperOverlay.LayoutBuilder.Persistence;
using SuperOverlay.LayoutBuilder.Panels;
using SuperOverlay.LayoutBuilder.PanelLayouts;
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
    private readonly PanelLayoutCompiler _panelLayoutCompiler = new();
    private readonly PanelPresetLibrary _panelPresetLibrary = new();

    private LayoutDocument _layout;
    private PanelLayoutDocument? _panelLayout;
    private string? _panelLayoutPath;
    private IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> _compiledPanelItemMap = new Dictionary<Guid, IReadOnlyList<Guid>>();
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
    public bool HasPanelLayout => _panelLayout is not null;
    public string? GetPanelLayoutPath() => _panelLayoutPath;

    public LayoutCanvas GetCanvas() => _layout.Canvas;
    public bool UpdateCanvasSize(double width, double height)
    {
        width = Math.Round(width);
        height = Math.Round(height);

        if (width < 320 || height < 180)
        {
            return false;
        }

        var canvas = new LayoutCanvas(width, height);
        var changed = false;

        if (_panelLayout is not null)
        {
            if (_panelLayout.Canvas.Width != canvas.Width || _panelLayout.Canvas.Height != canvas.Height)
            {
                _panelLayout = _panelLayout with { Canvas = canvas };
                changed = true;
            }
        }

        if (_layout.Canvas.Width != canvas.Width || _layout.Canvas.Height != canvas.Height)
        {
            _layout = _layout with { Canvas = canvas };
            changed = true;
        }

        if (!changed)
        {
            return true;
        }

        RefreshRuntime();
        return true;
    }


    public bool HasPanelSelection => _panelLayout is not null && GetSelectedPanelIds().Count > 0;


    public IReadOnlyList<DashboardCatalogItem> GetCatalog() => _registry.GetCatalog().OrderBy(x => x.DisplayName).ToList();

    public LayoutSelectedItemProperties? GetSelectedItemProperties()
    {
        if (_panelLayout is not null)
        {
            var selectedPanels = GetSelectedPanels();
            if (selectedPanels.Count == 0)
            {
                return null;
            }

            var primaryPanel = ResolveSelectedPanel(_selectedItemId) ?? selectedPanels[0];
            var compiledItemIds = _compiledPanelItemMap.TryGetValue(primaryPanel.Id, out var itemIds) ? itemIds : Array.Empty<Guid>();
            var placements = _layout.Placements.Where(x => compiledItemIds.Contains(x.ItemId)).ToList();
            if (placements.Count == 0)
            {
                return null;
            }

            var bounds = GetBounds(placements);
            return new LayoutSelectedItemProperties(
                primaryPanel.Id,
                "panel.instance",
                primaryPanel.PanelName,
                primaryPanel.X,
                primaryPanel.Y,
                bounds.Width,
                bounds.Height,
                primaryPanel.ZIndex,
                primaryPanel.IsLocked,
                selectedPanels.Count,
                true);
        }

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
        if (_panelLayout is not null)
        {
            return null;
        }

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
        if (_panelLayout is not null)
        {
            return false;
        }

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
        if (_panelLayout is not null)
        {
            return null;
        }

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
        if (_panelLayout is not null)
        {
            return false;
        }

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
        if (_panelLayout is not null)
        {
            return null;
        }

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
        if (_panelLayout is not null)
        {
            return false;
        }

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
        if (_panelLayout is not null)
        {
            var selectedPanelIds = GetSelectedPanelIds();
            if (selectedPanelIds.Count == 0)
            {
                return false;
            }

            var primaryPanel = ResolveSelectedPanel(_selectedItemId);
            var updatedPanels = _panelLayout.Panels
                .Select(panel =>
                {
                    if (!selectedPanelIds.Contains(panel.Id))
                    {
                        return panel;
                    }

                    if (primaryPanel is not null && panel.Id == primaryPanel.Id)
                    {
                        return panel with { X = x, Y = y, ZIndex = zIndex, IsLocked = isLocked };
                    }

                    return panel with { IsLocked = isLocked };
                })
                .ToList();

            _panelLayout = _panelLayout with { Panels = updatedPanels };
            return RecompilePanelLayout(selectLastPanel: false, preserveSelection: true);
        }

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
        var panelItems = GetCompiledPanelItemIds(itemId);
        if (panelItems.Count > 0)
        {
            return panelItems;
        }

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
        if (_panelLayout is not null)
        {
            return false;
        }

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

        if (_panelLayout is not null)
        {
            var selectedPanelIds = GetSelectedPanelIds();
            if (selectedPanelIds.Count == 0)
            {
                return false;
            }

            var updatedPanels = _panelLayout.Panels.Where(x => !selectedPanelIds.Contains(x.Id)).ToList();
            if (updatedPanels.Count == _panelLayout.Panels.Count)
            {
                return false;
            }

            _panelLayout = _panelLayout with { Panels = updatedPanels };
            return RecompilePanelLayout(selectLastPanel: false, preserveSelection: false);
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

        if (_panelLayout is not null)
        {
            var selectedPanels = GetSelectedPanels();
            if (selectedPanels.Count == 0)
            {
                return false;
            }

            var nextZ = _panelLayout.Panels.Count == 0 ? 0 : _panelLayout.Panels.Max(x => x.ZIndex) + 1;
            var duplicates = selectedPanels
                .OrderBy(x => x.ZIndex)
                .Select((panel, index) => panel with
                {
                    Id = Guid.NewGuid(),
                    X = panel.X + 20,
                    Y = panel.Y + 20,
                    ZIndex = nextZ + index
                })
                .ToList();

            _panelLayout = _panelLayout with { Panels = _panelLayout.Panels.Concat(duplicates).ToList() };
            return RecompilePanelLayout(selectLastPanel: true, preserveSelection: false);
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


    public string GetLayoutPath() => _layoutPath;

    public PanelPresetDocument? CreateSelectedPanelPreset(string name, string category = "Custom")
    {
        if (_selectedItemIds.Count == 0)
        {
            return null;
        }

        var composer = new PanelPresetComposer();
        return composer.CreateFromLayoutSelection(_layout, _selectedItemIds, name, category);
    }

    public bool InsertPanelPreset(PanelPresetDocument preset, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(preset);

        var composer = new PanelPresetComposer();
        var updated = composer.InsertIntoLayout(_layout, preset, x, y, out var insertedItemIds);
        if (insertedItemIds.Count == 0)
        {
            return false;
        }

        _layout = updated;
        RefreshRuntime();
        SelectItems(insertedItemIds, insertedItemIds.LastOrDefault());
        return true;
    }

    public bool OpenPanelPresetForEditing(PanelPresetDocument preset, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(preset);

        var emptyLayout = new LayoutDocument(
            _layout.Version,
            $"Panel - {preset.Metadata.Name}",
            _layout.Canvas,
            Array.Empty<LayoutItemInstance>(),
            Array.Empty<LayoutItemPlacement>(),
            Array.Empty<LayoutItemLink>());

        var composer = new PanelPresetComposer();
        var updated = composer.InsertIntoLayout(emptyLayout, preset, x, y, out var insertedItemIds);
        if (insertedItemIds.Count == 0)
        {
            return false;
        }

        _panelLayout = null;
        _panelLayoutPath = null;
        _compiledPanelItemMap = new Dictionary<Guid, IReadOnlyList<Guid>>();
        _layout = updated;
        RefreshRuntime();
        SelectItems(insertedItemIds, insertedItemIds.LastOrDefault());
        return true;
    }

    public bool StartNewPanelLayout(string name = "Panel Layout")
    {
        var canvasWidth = Math.Max(1280, Math.Round(SystemParameters.PrimaryScreenWidth));
        var canvasHeight = Math.Max(720, Math.Round(SystemParameters.PrimaryScreenHeight));

        _panelLayout = new PanelLayoutDocument(
            Version: _layout.Version,
            Name: string.IsNullOrWhiteSpace(name) ? "Panel Layout" : name,
            Canvas: new LayoutCanvas(canvasWidth, canvasHeight),
            Panels: Array.Empty<PanelLayoutInstance>());
        _panelLayoutPath = null;
        return RecompilePanelLayout(selectLastPanel: false);
    }

    public bool OpenPanelLayout(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var library = new PanelLayoutLibrary();
        var panelLayout = library.Load(path);
        _panelLayout = panelLayout;
        _panelLayoutPath = path;
        return RecompilePanelLayout(selectLastPanel: false);
    }

    public bool SavePanelLayout(string path)
    {
        if (_panelLayout is null)
        {
            return false;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        SyncPanelLayoutFromCompiledLayout();
        var library = new PanelLayoutLibrary();
        library.Save(path, _panelLayout);
        _panelLayoutPath = path;
        return true;
    }

    public bool InsertPanelPresetAsPanelInstance(PanelPresetDocument preset, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(preset);

        if (_panelLayout is null)
        {
            return false;
        }

        var nextZ = _panelLayout.Panels.Count == 0 ? 0 : _panelLayout.Panels.Max(x => x.ZIndex) + 1;
        var panel = new PanelLayoutInstance(
            Id: Guid.NewGuid(),
            PanelPresetId: preset.Metadata.Id,
            PanelName: preset.Metadata.Name,
            Category: preset.Metadata.Category,
            X: x,
            Y: y,
            ZIndex: nextZ,
            IsLocked: false,
            Scale: 1.0,
            IsVisible: true);

        _panelLayout = _panelLayout with { Panels = _panelLayout.Panels.Concat(new[] { panel }).ToList() };
        return RecompilePanelLayout(selectLastPanel: true);
    }

    public void EndDrag() => _snapService.EndDrag();
    public void SaveLayout()
    {
        if (_panelLayout is not null && !string.IsNullOrWhiteSpace(_panelLayoutPath))
        {
            SavePanelLayout(_panelLayoutPath);
        }

        _fileStore.Save(_layoutPath, _layout);
    }

    public void ReloadLayout()
    {
        _panelLayout = null;
        _panelLayoutPath = null;
        _compiledPanelItemMap = new Dictionary<Guid, IReadOnlyList<Guid>>();
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

    private bool RecompilePanelLayout(bool selectLastPanel, bool preserveSelection = false)
    {
        if (_panelLayout is null)
        {
            return false;
        }

        var presetEntries = _panelPresetLibrary.List(_panelPresetLibrary.GetDefaultDirectory(_layoutPath))
            .GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.Last());

        _layout = _panelLayoutCompiler.Compile(
            _panelLayout,
            presetId => presetEntries.TryGetValue(presetId, out var entry) ? _panelPresetLibrary.Load(entry.Path) : null,
            out var panelItemMap);

        var previousSelectedPanelIds = preserveSelection ? GetSelectedPanelIds().ToHashSet() : new HashSet<Guid>();
        _compiledPanelItemMap = panelItemMap;
        RefreshRuntime();

        if (selectLastPanel && _panelLayout.Panels.Count > 0)
        {
            var panelId = _panelLayout.Panels.Last().Id;
            if (_compiledPanelItemMap.TryGetValue(panelId, out var itemIds) && itemIds.Count > 0)
            {
                SelectItems(itemIds, itemIds.Last());
            }
        }
        else if (preserveSelection && previousSelectedPanelIds.Count > 0)
        {
            var itemIds = previousSelectedPanelIds
                .Where(x => _compiledPanelItemMap.ContainsKey(x))
                .SelectMany(x => _compiledPanelItemMap[x])
                .ToList();

            if (itemIds.Count > 0)
            {
                SelectItems(itemIds, itemIds.Last());
            }
            else
            {
                SelectItem(null);
            }
        }
        else
        {
            SelectItem(null);
        }

        return true;
    }

    private IReadOnlyList<Guid> GetCompiledPanelItemIds(Guid itemId)
    {
        if (_panelLayout is null)
        {
            return Array.Empty<Guid>();
        }

        foreach (var entry in _compiledPanelItemMap)
        {
            if (entry.Value.Contains(itemId))
            {
                return entry.Value;
            }
        }

        return Array.Empty<Guid>();
    }

    private PanelLayoutInstance? ResolveSelectedPanel(Guid? itemId)
    {
        if (_panelLayout is null || itemId is null)
        {
            return null;
        }

        foreach (var panel in _panelLayout.Panels)
        {
            if (_compiledPanelItemMap.TryGetValue(panel.Id, out var itemIds) && itemIds.Contains(itemId.Value))
            {
                return panel;
            }
        }

        return null;
    }

    private IReadOnlyList<Guid> GetSelectedPanelIds()
    {
        if (_panelLayout is null || _selectedItemIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        return _compiledPanelItemMap
            .Where(x => x.Value.Any(_selectedItemIds.Contains))
            .Select(x => x.Key)
            .ToList();
    }

    private IReadOnlyList<PanelLayoutInstance> GetSelectedPanels()
    {
        if (_panelLayout is null)
        {
            return Array.Empty<PanelLayoutInstance>();
        }

        var selectedIds = GetSelectedPanelIds().ToHashSet();
        if (selectedIds.Count == 0)
        {
            return Array.Empty<PanelLayoutInstance>();
        }

        return _panelLayout.Panels.Where(x => selectedIds.Contains(x.Id)).OrderBy(x => x.ZIndex).ToList();
    }

    private void SyncPanelLayoutFromCompiledLayout()
    {
        if (_panelLayout is null || _compiledPanelItemMap.Count == 0)
        {
            return;
        }

        var updatedPanels = new List<PanelLayoutInstance>(_panelLayout.Panels.Count);
        foreach (var panel in _panelLayout.Panels)
        {
            if (!_compiledPanelItemMap.TryGetValue(panel.Id, out var itemIds) || itemIds.Count == 0)
            {
                updatedPanels.Add(panel);
                continue;
            }

            var placements = _layout.Placements.Where(x => itemIds.Contains(x.ItemId)).ToList();
            if (placements.Count == 0)
            {
                updatedPanels.Add(panel);
                continue;
            }

            var items = _layout.Items.Where(x => itemIds.Contains(x.Id)).ToList();
            updatedPanels.Add(panel with
            {
                X = placements.Min(x => x.X),
                Y = placements.Min(x => x.Y),
                ZIndex = placements.Min(x => x.ZIndex),
                IsLocked = items.Count > 0 && items.All(x => x.IsLocked)
            });
        }

        _panelLayout = _panelLayout with { Panels = updatedPanels };
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
