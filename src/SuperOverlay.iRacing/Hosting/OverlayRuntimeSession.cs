using System.Windows;
using SuperOverlay.Dashboards.Items.DecorativePanel;
using SuperOverlay.Dashboards.Items.Gear;
using SuperOverlay.Dashboards.Items.ShiftLeds;
using SuperOverlay.Dashboards.Items.Speed;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.LayoutBuilder.Layout;
using SuperOverlay.LayoutBuilder.Persistence;
using SuperOverlay.LayoutBuilder.PanelLayouts;
using SuperOverlay.LayoutBuilder.Runtime;
using SuperOverlay.LayoutBuilder.Panels;
namespace SuperOverlay.iRacing.Hosting;

public sealed class OverlayRuntimeSession
{
    private readonly LayoutHost _layoutHost;
    private readonly DashboardRegistry _registry;
    private readonly LayoutFileStore _fileStore;
    private readonly LayoutMutationService _mutationService;
    private readonly LayoutSnapService _snapService = new();
    private readonly string _layoutPath;
    private readonly OverlayShellMode _shellMode;
    private readonly WidgetSettingsSessionService _widgetSettingsService;
    private readonly PanelLayoutSessionService _panelLayoutService;
    private readonly OverlaySelectionState _selection = new();
    private readonly OverlaySelectionService _selectionService = new();
    private readonly OverlayEditCommandsService _editCommandsService;
    private readonly OverlaySelectionPresentationService _selectionPresentationService;
    private readonly OverlayRuntimeSyncService _runtimeSyncService;
    private readonly OverlayMovementService _movementService;
    private readonly LayoutClipboardState _clipboard = new();

    private LayoutDocument _layout;
    private PanelLayoutDocument? _panelLayout;
    private string? _panelLayoutPath;
    private IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> _compiledPanelItemMap = new Dictionary<Guid, IReadOnlyList<Guid>>();
    private bool _snappingEnabled = true;

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
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _mutationService = mutationService ?? throw new ArgumentNullException(nameof(mutationService));
        _layoutPath = string.IsNullOrWhiteSpace(layoutPath) ? throw new ArgumentException("Layout path is required.", nameof(layoutPath)) : layoutPath;
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _shellMode = shellMode;
        _widgetSettingsService = new WidgetSettingsSessionService(_registry, _mutationService);
        _panelLayoutService = new PanelLayoutSessionService(_layoutPath);
        _editCommandsService = new OverlayEditCommandsService(_mutationService);
        _selectionPresentationService = new OverlaySelectionPresentationService(_registry, _editCommandsService);
        _runtimeSyncService = new OverlayRuntimeSyncService(_layoutHost, composer, _selectionService, _shellMode);
        _movementService = new OverlayMovementService(_mutationService, _snapService, _selectionService);

        RefreshRuntime();
    }

    public void Update(object runtimeState) => _layoutHost.Update(runtimeState);
    public void SetSnappingEnabled(bool enabled) => _snappingEnabled = enabled;
    public Guid? GetSelectedItemId() => _selection.PrimaryItemId;
    public IReadOnlyList<Guid> GetSelectedItemIds() => _selection.ItemIds.ToList();
    public bool HasSelection => _selection.HasSelection;
    public bool CanPaste => _clipboard.HasContent;
    public bool HasLockedSelection => _selection.ItemIds.Any(id => _layout.Items.Any(x => x.Id == id && x.IsLocked));
    public bool HasPanelLayout => _panelLayoutService.HasPanelLayout(_panelLayout);
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
        return _selectionPresentationService.GetSelectedItemProperties(_layout, _selection, _panelLayout, _compiledPanelItemMap);
    }

    public WidgetCornerSettings? GetSelectedWidgetCornerSettings()
    {
        if (_panelLayout is not null)
        {
            return null;
        }

        return _widgetSettingsService.GetSelectedCornerSettings(_layout, _selection.PrimaryItemId);
    }

    public bool UpdateSelectedWidgetCornerSettings(double topLeft, double topRight, double bottomRight, double bottomLeft)
    {
        if (_panelLayout is not null)
        {
            return false;
        }

        var changed = _widgetSettingsService.UpdateSelectedCornerSettings(ref _layout, _selection.PrimaryItemId, topLeft, topRight, bottomRight, bottomLeft);
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

        return _widgetSettingsService.GetSelectedSettings<ShiftLedDashboardSettings>(_layout, _selection.PrimaryItemId, "dashboard.shift-leds");
    }

    public bool UpdateSelectedShiftLedSettings(ShiftLedDashboardSettings settings)
    {
        if (_panelLayout is not null)
        {
            return false;
        }

        var changed = _widgetSettingsService.UpdateSelectedSettings(ref _layout, _selection.PrimaryItemId, "dashboard.shift-leds", settings);
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

        return _widgetSettingsService.GetSelectedSettings<DecorativePanelDashboardSettings>(_layout, _selection.PrimaryItemId, "dashboard.decorative-panel");
    }

    public bool UpdateSelectedDecorativePanelSettings(DecorativePanelDashboardSettings settings)
    {
        if (_panelLayout is not null)
        {
            return false;
        }

        var changed = _widgetSettingsService.UpdateSelectedSettings(ref _layout, _selection.PrimaryItemId, "dashboard.decorative-panel", settings);
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
        var changed = _movementService.UpdateSelectedItemProperties(
            ref _layout,
            ref _panelLayout,
            _selection,
            _compiledPanelItemMap,
            x,
            y,
            width,
            height,
            zIndex,
            isLocked);
        if (!changed)
        {
            return false;
        }

        if (_panelLayout is not null)
        {
            return RecompilePanelLayout(selectLastPanel: false, preserveSelection: true);
        }

        RefreshRuntime();
        SyncSelectionState();
        return true;
    }

    public IReadOnlyList<LayoutEditorItem> GetLayoutItems()
    {
        return _selectionPresentationService.GetLayoutItems(_layout);
    }

    public void SelectItem(Guid? itemId)
    {
        if (itemId is null)
        {
            _selection.Clear();
            SyncSelectionState();
            return;
        }

        SelectItems(_selectionService.GetSelectionUnitItemIds(_layout, itemId.Value, _panelLayout, _compiledPanelItemMap), itemId.Value);
    }

    public void SelectItems(IEnumerable<Guid> itemIds, Guid? primaryItemId = null)
    {
        var normalized = _selectionService.NormalizeSelection(_layout, itemIds, primaryItemId, _panelLayout, _compiledPanelItemMap);
        _selection.Replace(normalized.ItemIds, normalized.PrimaryItemId);
        SyncSelectionState();
    }

    public void ToggleItemSelection(Guid itemId)
    {
        var targetUnit = _selectionService.GetSelectionUnitItemIds(_layout, itemId, _panelLayout, _compiledPanelItemMap).ToHashSet();
        var currentGroupedIds = _selectionService.GetSelectedGroupedItemIds(_layout, _selection);
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

        if (_selection.Contains(itemId))
        {
            _selection.Remove(itemId);
        }
        else
        {
            _selection.Replace(_selection.ItemIds.Concat(new[] { itemId }), itemId);
        }

        SyncSelectionState();
    }

    public void SetPrimarySelection(Guid itemId)
    {
        SelectItems(_selectionService.GetSelectionUnitItemIds(_layout, itemId, _panelLayout, _compiledPanelItemMap), itemId);
    }


    public Guid? HitTestItemId(object? hitSource)
    {
        return _runtimeSyncService.HitTestItemId(hitSource);
    }

    public bool IsResizeHandleHit(object? hitSource, Guid itemId)
    {
        if (_panelLayout is not null)
        {
            return false;
        }

        return _runtimeSyncService.IsResizeHandleHit(hitSource, itemId);
    }

    public bool IsLocked(Guid itemId) => _layout.Items.Any(x => x.Id == itemId && x.IsLocked);

    public IReadOnlyList<Guid> GetItemsInSelectionRect(double x, double y, double width, double height, bool requireFullContainment = false)
    {
        var left = Math.Min(x, x + width);
        var top = Math.Min(y, y + height);
        var right = Math.Max(x, x + width);
        var bottom = Math.Max(y, y + height);

        return _layout.Placements
            .Select(x => LayoutPlacementResolver.ResolveForShell(x, _layout.Canvas, _shellMode))
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
        var changed = _editCommandsService.DeleteSelected(ref _layout, ref _panelLayout, _selection, _compiledPanelItemMap);
        if (!changed)
        {
            return false;
        }

        if (_panelLayout is not null)
        {
            return RecompilePanelLayout(selectLastPanel: false, preserveSelection: false);
        }

        _selection.Clear();
        RefreshRuntime();
        return true;
    }

    public bool DuplicateSelected()
    {
        var changed = _editCommandsService.DuplicateSelected(
            ref _layout,
            ref _panelLayout,
            _selection,
            _compiledPanelItemMap,
            out var newSelectionIds,
            out var primarySelectionId);

        if (!changed)
        {
            return false;
        }

        if (_panelLayout is not null)
        {
            return RecompilePanelLayout(selectLastPanel: true, preserveSelection: false);
        }

        RefreshRuntime();
        SelectItems(newSelectionIds, primarySelectionId);
        return true;
    }

    public bool GroupSelectedItems()
    {
        var changed = _editCommandsService.GroupSelectedItems(ref _layout, _selection);
        if (!changed)
        {
            return false;
        }

        var orderedIds = _selection.ItemIds.ToList();
        var anchorId = _selection.PrimaryItemId is not null && _selection.ItemIds.Contains(_selection.PrimaryItemId.Value) ? _selection.PrimaryItemId.Value : orderedIds[0];
        RefreshRuntime();
        SelectItems(orderedIds, anchorId);
        return true;
    }

    public bool UngroupSelected()
    {
        var changed = _editCommandsService.UngroupSelected(ref _layout, _selection);
        if (!changed)
        {
            return false;
        }

        RefreshRuntime();
        SyncSelectionState();
        return true;
    }

    public bool SetLockSelected(bool isLocked)
    {
        var changed = _editCommandsService.SetLockSelected(ref _layout, ref _panelLayout, _selection, _compiledPanelItemMap, isLocked);
        if (!changed)
        {
            return false;
        }

        if (_panelLayout is not null)
        {
            return RecompilePanelLayout(selectLastPanel: false, preserveSelection: true);
        }

        RefreshRuntime();
        SyncSelectionState();
        return true;
    }

    public bool CopySelected()
    {
        if (_selection.ItemIds.Count == 0)
        {
            return false;
        }

        _clipboard.Set(_layout, _selection.ItemIds);
        return true;
    }

    public bool PasteClipboard()
    {
        if (!_clipboard.HasContent || _clipboard.SourceLayout is null)
        {
            return false;
        }

        var offset = 24 * _clipboard.AdvancePasteSequence();
        var changed = _mutationService.PasteItemsFromLayout(ref _layout, _clipboard.SourceLayout, _clipboard.ItemIds, offset, offset, out var newItemIds);
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
        var result = _movementService.MoveSelectedWithSnap(
            ref _layout,
            _selection,
            _panelLayout,
            _compiledPanelItemMap,
            deltaX,
            deltaY,
            canvasWidth,
            canvasHeight,
            _snappingEnabled,
            bypassSnap,
            _shellMode);
        if (result.Moved)
        {
            var movedItemIds = _selectionService.GetActiveMoveItemIds(_layout, _selection, _panelLayout, _compiledPanelItemMap)
                .Where(id => !IsLocked(id))
                .ToList();
            _runtimeSyncService.SyncPlacementsToRuntime(_layout, movedItemIds);
        }

        return result;
    }

    public LayoutMoveResult ResizeSelectedWithSnap(double deltaWidth, double deltaHeight, double canvasWidth, double canvasHeight, bool bypassSnap)
    {
        var result = _movementService.ResizeSelectedWithSnap(
            ref _layout,
            _selection,
            deltaWidth,
            deltaHeight,
            canvasWidth,
            canvasHeight,
            _snappingEnabled,
            bypassSnap);
        if (result.Moved && _selection.PrimaryItemId is not null)
        {
            _runtimeSyncService.SyncPlacementToRuntime(_layout, _selection.PrimaryItemId.Value);
        }

        return result;
    }


    public string GetLayoutPath() => _layoutPath;

    public PanelPresetDocument? CreateSelectedPanelPreset(string name, string category = "Custom")
    {
        if (_selection.ItemIds.Count == 0)
        {
            return null;
        }

        var composer = new PanelPresetComposer();
        return composer.CreateFromLayoutSelection(_layout, _selection.ItemIds, name, category);
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
        _panelLayout = _panelLayoutService.CreateNew(name, _layout);
        _panelLayoutPath = null;
        return RecompilePanelLayout(selectLastPanel: false);
    }

    public bool OpenPanelLayout(string path)
    {
        _panelLayout = _panelLayoutService.Load(path);
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
        _panelLayoutService.Save(path, _panelLayout);
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
        _selection.Replace(_selection.ItemIds.Where(validIds.Contains), _selection.PrimaryItemId);

        RefreshRuntime();
    }

    private bool ReorderSelected(Func<IReadOnlyList<LayoutItemPlacement>, IEnumerable<LayoutItemPlacement>, Dictionary<Guid, int>> reorder)
    {
        var changed = _editCommandsService.ReorderSelected(ref _layout, ref _panelLayout, _selection, _compiledPanelItemMap, reorder);
        if (!changed)
        {
            return false;
        }

        if (_panelLayout is not null)
        {
            return RecompilePanelLayout(selectLastPanel: false, preserveSelection: true);
        }

        RefreshRuntime();
        SyncSelectionState();
        return true;
    }

    private bool RecompilePanelLayout(bool selectLastPanel, bool preserveSelection = false)
    {
        if (_panelLayout is null)
        {
            return false;
        }

        _layout = _panelLayoutService.Compile(_panelLayout, out var panelItemMap);

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

        private PanelLayoutInstance? ResolveSelectedPanel(Guid? itemId)
    {
        return _selectionPresentationService.ResolveSelectedPanel(itemId, _panelLayout, _compiledPanelItemMap);
    }

    private IReadOnlyList<Guid> GetSelectedPanelIds()
    {
        return _selectionPresentationService.GetSelectedPanelIds(_selection, _panelLayout, _compiledPanelItemMap);
    }

    private IReadOnlyList<PanelLayoutInstance> GetSelectedPanels()
    {
        return _selectionPresentationService.GetSelectedPanels(_selection, _panelLayout, _compiledPanelItemMap);
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
        _runtimeSyncService.RefreshRuntime(_layout, _selection);
    }

    private void SyncSelectionState()
    {
        _runtimeSyncService.SyncSelectionState(_layout, _selection);
    }

    private void SyncAllPlacementsToRuntime()
    {
        _runtimeSyncService.SyncAllPlacementsToRuntime(_layout);
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
