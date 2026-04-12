using SuperOverlay.Dashboards.Items.DecorativePanel;
using SuperOverlay.Dashboards.Items.ShiftLeds;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.PanelLayouts;
using SuperOverlay.Core.Layouts.Panels;
using SuperOverlay.Core.Layouts.Editing;
using SuperOverlay.Core.Layouts.Editor;
using SuperOverlay.Core.Layouts.Runtime;

namespace SuperOverlay.iRacing.Hosting;

internal sealed class OverlayEditingSessionService
{
    private readonly OverlaySessionState _state;
    private readonly DashboardRegistry _registry;
    private readonly WidgetSettingsSessionService _widgetSettingsService;
    private readonly IRacingLayoutItemCatalogService _itemCatalogService;
    private readonly OverlaySelectionState _selection;
    private readonly OverlaySelectionService _selectionService;
    private readonly OverlayEditCommandsService _editCommandsService;
    private readonly LayoutSelectionPresentationService _selectionPresentationService;
    private readonly PanelPresetEditingService _panelPresetEditingService;
    private readonly PanelLayoutEditingService _panelLayoutEditingService;
    private readonly OverlayMovementService _movementService;
    private readonly LayoutMutationCore _mutationService;
    private readonly LayoutClipboardState _clipboard;
    private readonly OverlayDocumentSessionService _documentService;
    private readonly LayoutSnapService _snapService;
    private readonly OverlayShellMode _shellMode;

    public OverlayEditingSessionService(
        OverlaySessionState state,
        DashboardRegistry registry,
        WidgetSettingsSessionService widgetSettingsService,
        IRacingLayoutItemCatalogService itemCatalogService,
        OverlaySelectionState selection,
        OverlaySelectionService selectionService,
        OverlayEditCommandsService editCommandsService,
        LayoutSelectionPresentationService selectionPresentationService,
        PanelPresetEditingService panelPresetEditingService,
        PanelLayoutEditingService panelLayoutEditingService,
        OverlayMovementService movementService,
        LayoutMutationCore mutationService,
        LayoutClipboardState clipboard,
        OverlayDocumentSessionService documentService,
        LayoutSnapService snapService,
        OverlayShellMode shellMode)
    {
        _state = state;
        _registry = registry;
        _widgetSettingsService = widgetSettingsService;
        _itemCatalogService = itemCatalogService;
        _selection = selection;
        _selectionService = selectionService;
        _editCommandsService = editCommandsService;
        _selectionPresentationService = selectionPresentationService;
        _panelPresetEditingService = panelPresetEditingService;
        _panelLayoutEditingService = panelLayoutEditingService;
        _movementService = movementService;
        _mutationService = mutationService;
        _clipboard = clipboard;
        _documentService = documentService;
        _snapService = snapService;
        _shellMode = shellMode;
    }

    public DashboardRegistry Registry => _registry;
    public bool HasSelection => _selection.HasSelection;
    public bool CanPaste => _clipboard.HasContent;
    public bool HasLockedSelection => _selection.ItemIds.Any(id => _state.Layout.Items.Any(x => x.Id == id && x.IsLocked));
    public bool HasPanelSelection => _state.PanelLayout is not null && _selectionPresentationService.GetSelectedPanelIds(_selection, _state.PanelLayout, _state.CompiledPanelItemMap).Count > 0;
    public Guid? GetSelectedItemId() => _selection.PrimaryItemId;
    public IEnumerable<Guid> GetSelectedItemIds() => _selection.ItemIds.ToList();
    public IReadOnlyList<DashboardCatalogItem> GetCatalog() => _registry.GetCatalog().OrderBy(x => x.DisplayName).ToList();
    public LayoutSelectedItemProperties? GetSelectedItemProperties() => _selectionPresentationService.GetSelectedItemProperties(_state.Layout, _selection, _state.PanelLayout, _state.CompiledPanelItemMap);
    public IReadOnlyList<LayoutEditorItem> GetLayoutItems() => _selectionPresentationService.GetLayoutItems(_state.Layout);
    public bool IsLocked(Guid itemId) => _state.Layout.Items.Any(x => x.Id == itemId && x.IsLocked);
    public void EndDrag() => _snapService.EndDrag();

    public WidgetCornerSettings? GetSelectedWidgetCornerSettings()
        => _state.PanelLayout is not null ? null : _widgetSettingsService.GetSelectedCornerSettings(_state.Layout, _selection.PrimaryItemId);

    public bool UpdateSelectedWidgetCornerSettings(double topLeft, double topRight, double bottomRight, double bottomLeft)
    {
        if (_state.PanelLayout is not null) return false;
        var layout = _state.Layout;
        var changed = _widgetSettingsService.UpdateSelectedCornerSettings(ref layout, _selection.PrimaryItemId, topLeft, topRight, bottomRight, bottomLeft);
        if (!changed) return false;
        _state.Layout = layout;
        _documentService.RefreshRuntime();
        _documentService.SyncSelectionState();
        return true;
    }

    public ShiftLedDashboardSettings? GetSelectedShiftLedSettings()
        => _state.PanelLayout is not null ? null : _widgetSettingsService.GetSelectedSettings<ShiftLedDashboardSettings>(_state.Layout, _selection.PrimaryItemId, "dashboard.shift-leds");

    public bool UpdateSelectedShiftLedSettings(ShiftLedDashboardSettings settings)
    {
        if (_state.PanelLayout is not null) return false;
        var layout = _state.Layout;
        var changed = _widgetSettingsService.UpdateSelectedSettings(ref layout, _selection.PrimaryItemId, "dashboard.shift-leds", settings);
        if (!changed) return false;
        _state.Layout = layout;
        _documentService.RefreshRuntime();
        _documentService.SyncSelectionState();
        return true;
    }

    public DecorativePanelDashboardSettings? GetSelectedDecorativePanelSettings()
        => _state.PanelLayout is not null ? null : _widgetSettingsService.GetSelectedSettings<DecorativePanelDashboardSettings>(_state.Layout, _selection.PrimaryItemId, "dashboard.decorative-panel");

    public bool UpdateSelectedDecorativePanelSettings(DecorativePanelDashboardSettings settings)
    {
        if (_state.PanelLayout is not null) return false;
        var layout = _state.Layout;
        var changed = _widgetSettingsService.UpdateSelectedSettings(ref layout, _selection.PrimaryItemId, "dashboard.decorative-panel", settings);
        if (!changed) return false;
        _state.Layout = layout;
        _documentService.RefreshRuntime();
        _documentService.SyncSelectionState();
        return true;
    }

    public bool UpdateSelectedItemProperties(double x, double y, double width, double height, int zIndex, bool isLocked)
    {
        var layout = _state.Layout;
        var panelLayout = _state.PanelLayout;
        var changed = _movementService.UpdateSelectedItemProperties(ref layout, ref panelLayout, _selection, _state.CompiledPanelItemMap, x, y, width, height, zIndex, isLocked);
        if (!changed) return false;
        _state.Layout = layout;
        _state.PanelLayout = panelLayout;
        if (_state.PanelLayout is not null) return _documentService.RecompilePanelLayout(false, true);
        _documentService.RefreshRuntime();
        _documentService.SyncSelectionState();
        return true;
    }

    public void SelectItem(Guid? itemId)
    {
        if (itemId is null)
        {
            _selection.Clear();
            _documentService.SyncSelectionState();
            return;
        }

        SelectItems(_selectionService.GetSelectionUnitItemIds(_state.Layout, itemId.Value, _state.PanelLayout, _state.CompiledPanelItemMap), itemId.Value);
    }

    public void SelectItems(IEnumerable<Guid> itemIds, Guid? primaryItemId = null)
    {
        var normalized = _selectionService.NormalizeSelection(_state.Layout, itemIds, primaryItemId, _state.PanelLayout, _state.CompiledPanelItemMap);
        _selection.Replace(normalized.ItemIds, normalized.PrimaryItemId);
        _documentService.SyncSelectionState();
    }

    public void ToggleItemSelection(Guid itemId)
    {
        var targetUnit = _selectionService.GetSelectionUnitItemIds(_state.Layout, itemId, _state.PanelLayout, _state.CompiledPanelItemMap).ToHashSet();
        var currentGroupedIds = _selectionService.GetSelectedGroupedItemIds(_state.Layout, _selection);
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

        if (_selection.Contains(itemId)) _selection.Remove(itemId); else _selection.Replace(_selection.ItemIds.Concat(new[] { itemId }), itemId);
        _documentService.SyncSelectionState();
    }

    public void SetPrimarySelection(Guid itemId) => SelectItems(_selectionService.GetSelectionUnitItemIds(_state.Layout, itemId, _state.PanelLayout, _state.CompiledPanelItemMap), itemId);

    public bool AddItem(string typeId)
    {
        var layout = _state.Layout;
        var changed = _itemCatalogService.AddItem(ref layout, typeId);
        if (changed)
        {
            _state.Layout = layout;
            _documentService.RefreshRuntime();
        }
        return changed;
    }

    public bool DeleteSelected()
    {
        var layout = _state.Layout;
        var panelLayout = _state.PanelLayout;
        var changed = _editCommandsService.DeleteSelected(ref layout, ref panelLayout, _selection, _state.CompiledPanelItemMap);
        if (!changed) return false;
        _state.Layout = layout;
        _state.PanelLayout = panelLayout;
        if (_state.PanelLayout is not null) return _documentService.RecompilePanelLayout(false, false);
        _selection.Clear();
        _documentService.RefreshRuntime();
        return true;
    }

    public bool DuplicateSelected()
    {
        var layout = _state.Layout;
        var panelLayout = _state.PanelLayout;
        var changed = _editCommandsService.DuplicateSelected(ref layout, ref panelLayout, _selection, _state.CompiledPanelItemMap, out var newSelectionIds, out var primarySelectionId);
        if (!changed) return false;
        _state.Layout = layout;
        _state.PanelLayout = panelLayout;
        if (_state.PanelLayout is not null) return _documentService.RecompilePanelLayout(true, false);
        _documentService.RefreshRuntime();
        SelectItems(newSelectionIds, primarySelectionId);
        return true;
    }

    public bool GroupSelectedItems()
    {
        var layout = _state.Layout;
        var changed = _editCommandsService.GroupSelectedItems(ref layout, _selection);
        if (!changed) return false;
        _state.Layout = layout;
        var orderedIds = _selection.ItemIds.ToList();
        var anchorId = _selection.PrimaryItemId is not null && _selection.ItemIds.Contains(_selection.PrimaryItemId.Value) ? _selection.PrimaryItemId.Value : orderedIds[0];
        _documentService.RefreshRuntime();
        SelectItems(orderedIds, anchorId);
        return true;
    }

    public bool UngroupSelected()
    {
        var layout = _state.Layout;
        var changed = _editCommandsService.UngroupSelected(ref layout, _selection);
        if (!changed) return false;
        _state.Layout = layout;
        _documentService.RefreshRuntime();
        _documentService.SyncSelectionState();
        return true;
    }

    public bool SetLockSelected(bool isLocked)
    {
        var layout = _state.Layout;
        var panelLayout = _state.PanelLayout;
        var changed = _editCommandsService.SetLockSelected(ref layout, ref panelLayout, _selection, _state.CompiledPanelItemMap, isLocked);
        if (!changed) return false;
        _state.Layout = layout;
        _state.PanelLayout = panelLayout;
        if (_state.PanelLayout is not null) return _documentService.RecompilePanelLayout(false, true);
        _documentService.RefreshRuntime();
        _documentService.SyncSelectionState();
        return true;
    }

    public bool CopySelected()
    {
        if (_selection.ItemIds.Count == 0) return false;
        _clipboard.Set(_state.Layout, _selection.ItemIds);
        return true;
    }

    public bool PasteClipboard()
    {
        if (!_clipboard.HasContent || _clipboard.SourceLayout is null) return false;
        var offset = 24 * _clipboard.AdvancePasteSequence();
        var layout = _state.Layout;
        var changed = _mutationService.PasteItemsFromLayout(ref layout, _clipboard.SourceLayout, _clipboard.ItemIds, offset, offset, out var newItemIds);
        if (!changed) return false;
        _state.Layout = layout;
        _documentService.RefreshRuntime();
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
        for (var i = list.Count - 2; i >= 0; i--) if (selectedIds.Contains(list[i].ItemId) && !selectedIds.Contains(list[i + 1].ItemId)) (list[i], list[i + 1]) = (list[i + 1], list[i]);
        return BuildZMap(list);
    });
    public bool SendBackwardSelected() => ReorderSelected((all, selected) =>
    {
        var list = all.ToList();
        var selectedIds = selected.Select(x => x.ItemId).ToHashSet();
        for (var i = 1; i < list.Count; i++) if (selectedIds.Contains(list[i].ItemId) && !selectedIds.Contains(list[i - 1].ItemId)) (list[i], list[i - 1]) = (list[i - 1], list[i]);
        return BuildZMap(list);
    });

    public LayoutMoveResult MoveSelectedWithSnap(double deltaX, double deltaY, double canvasWidth, double canvasHeight, bool bypassSnap)
    {
        var layout = _state.Layout;
        var result = _movementService.MoveSelectedWithSnap(ref layout, _selection, _state.PanelLayout, _state.CompiledPanelItemMap, deltaX, deltaY, canvasWidth, canvasHeight, _state.SnappingEnabled, bypassSnap, _shellMode);
        _state.Layout = layout;
        if (result.Moved)
        {
            var movedItemIds = _selectionService.GetActiveMoveItemIds(_state.Layout, _selection, _state.PanelLayout, _state.CompiledPanelItemMap).Where(id => !IsLocked(id)).ToList();
            _documentService.SyncAllPlacementsToRuntime();
        }
        return result;
    }

    public LayoutMoveResult ResizeSelectedWithSnap(double deltaWidth, double deltaHeight, double canvasWidth, double canvasHeight, bool bypassSnap)
    {
        var layout = _state.Layout;
        var result = _movementService.ResizeSelectedWithSnap(ref layout, _selection, deltaWidth, deltaHeight, canvasWidth, canvasHeight, _state.SnappingEnabled, bypassSnap);
        _state.Layout = layout;
        if (result.Moved) _documentService.SyncAllPlacementsToRuntime();
        return result;
    }

    public PanelPresetDocument? CreateSelectedPanelPreset(string name, string category = "Custom") => _panelPresetEditingService.CreateSelectedPanelPreset(_state.Layout, _selection.ItemIds.ToList(), name, category);

    public bool InsertPanelPreset(PanelPresetDocument preset, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(preset);
        if (!_panelPresetEditingService.InsertPanelPreset(_state.Layout, preset, x, y, out var updatedLayout, out var insertedItemIds)) return false;
        _state.Layout = updatedLayout;
        _documentService.RefreshRuntime();
        SelectItems(insertedItemIds, insertedItemIds.LastOrDefault());
        return true;
    }

    public bool OpenPanelPresetForEditing(PanelPresetDocument preset, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(preset);
        if (!_panelPresetEditingService.OpenPanelPresetForEditing(_state.Layout, preset, x, y, out var updatedLayout, out var insertedItemIds)) return false;
        _state.PanelLayout = null;
        _state.PanelLayoutPath = null;
        _state.CompiledPanelItemMap = new Dictionary<Guid, IReadOnlyList<Guid>>();
        _state.Layout = updatedLayout;
        _documentService.RefreshRuntime();
        SelectItems(insertedItemIds, insertedItemIds.LastOrDefault());
        return true;
    }

    public bool InsertPanelPresetAsPanelInstance(PanelPresetDocument preset, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(preset);
        if (!_panelLayoutEditingService.InsertPanelPresetAsPanelInstance(_state.PanelLayout, preset, x, y, out var updatedPanelLayout) || updatedPanelLayout is null) return false;
        _state.PanelLayout = updatedPanelLayout;
        return _documentService.RecompilePanelLayout(true, false);
    }

    private bool ReorderSelected(Func<IReadOnlyList<LayoutItemPlacement>, IEnumerable<LayoutItemPlacement>, Dictionary<Guid, int>> reorder)
    {
        var layout = _state.Layout;
        var panelLayout = _state.PanelLayout;
        var changed = _editCommandsService.ReorderSelected(ref layout, ref panelLayout, _selection, _state.CompiledPanelItemMap, reorder);
        if (!changed) return false;
        _state.Layout = layout;
        _state.PanelLayout = panelLayout;
        if (_state.PanelLayout is not null) return _documentService.RecompilePanelLayout(false, true);
        _documentService.RefreshRuntime();
        _documentService.SyncSelectionState();
        return true;
    }

    private static Dictionary<Guid, int> BuildZMap(IReadOnlyList<LayoutItemPlacement> orderedPlacements)
    {
        var map = new Dictionary<Guid, int>();
        for (var i = 0; i < orderedPlacements.Count; i++) map[orderedPlacements[i].ItemId] = i;
        return map;
    }
}
