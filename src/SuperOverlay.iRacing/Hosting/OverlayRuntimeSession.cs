using SuperOverlay.Dashboards.Items.DecorativePanel;
using SuperOverlay.Dashboards.Items.ShiftLeds;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.Persistence;
using SuperOverlay.Core.Layouts.PanelLayouts;
using SuperOverlay.Core.Layouts.Runtime;
using SuperOverlay.Core.Layouts.Panels;
using SuperOverlay.Core.Layouts.Editing;
using SuperOverlay.Core.Layouts.Editor;
namespace SuperOverlay.iRacing.Hosting;

public sealed class OverlayRuntimeSession : ILayoutInteractionSession
{
    private readonly OverlaySessionState _state;
    private readonly LayoutSnapService _snapService;
    private readonly OverlaySelectionState _selection;
    private readonly OverlayDocumentSessionService _documentSession;
    private readonly OverlayEditingSessionService _editingSession;
    private readonly PanelLayoutEditingService _panelLayoutEditingService;

    public OverlayRuntimeSession(
        LayoutHost layoutHost,
        DashboardRegistry registry,
        LayoutRuntimeComposer composer,
        LayoutFileStore fileStore,
        LayoutMutationCore mutationService,
        IRacingLayoutItemCatalogService itemCatalogService,
        string layoutPath,
        LayoutDocument layout,
        OverlayShellMode shellMode)
    {
        ArgumentNullException.ThrowIfNull(layoutHost);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(composer);
        ArgumentNullException.ThrowIfNull(fileStore);
        ArgumentNullException.ThrowIfNull(mutationService);
        ArgumentNullException.ThrowIfNull(itemCatalogService);
        ArgumentException.ThrowIfNullOrWhiteSpace(layoutPath);
        ArgumentNullException.ThrowIfNull(layout);

        _state = new OverlaySessionState { Layout = layout };
        _snapService = new LayoutSnapService();
        _selection = new OverlaySelectionState();
        var selectionService = new OverlaySelectionService();
        var widgetSettingsService = new WidgetSettingsSessionService(registry, mutationService);
        var panelLayoutService = new PanelLayoutSessionService(layoutPath);
        var editCommandsService = new OverlayEditCommandsService(mutationService);
        var selectionPresentationService = new LayoutSelectionPresentationService(new DashboardLayoutItemMetadataResolver(registry), editCommandsService);
        var runtimeSyncService = new OverlayRuntimeSyncService(layoutHost, composer, selectionService, shellMode);
        var movementService = new OverlayMovementService(mutationService, _snapService, selectionService);
        var clipboard = new LayoutClipboardState();
        var panelPresetEditingService = new PanelPresetEditingService();
        _panelLayoutEditingService = new PanelLayoutEditingService();

        _documentSession = new OverlayDocumentSessionService(
            _state,
            fileStore,
            layoutPath,
            shellMode,
            panelLayoutService,
            runtimeSyncService,
            _selection,
            selectionPresentationService);

        _editingSession = new OverlayEditingSessionService(
            _state,
            registry,
            widgetSettingsService,
            itemCatalogService,
            _selection,
            selectionService,
            editCommandsService,
            selectionPresentationService,
            panelPresetEditingService,
            _panelLayoutEditingService,
            movementService,
            mutationService,
            clipboard,
            _documentSession,
            _snapService,
            shellMode);

        _documentSession.RefreshRuntime();
    }

    public void Update(object runtimeState) => _documentSession.Update(runtimeState);
    public void SetSnappingEnabled(bool enabled) => _state.SnappingEnabled = enabled;
    public Guid? GetSelectedItemId() => _editingSession.GetSelectedItemId();
    public IEnumerable<Guid> GetSelectedItemIds() => _editingSession.GetSelectedItemIds();
    public bool HasSelection => _editingSession.HasSelection;
    public bool CanPaste => _editingSession.CanPaste;
    public bool HasLockedSelection => _editingSession.HasLockedSelection;
    public bool HasPanelLayout => _documentSession.HasPanelLayout;
    public string? GetPanelLayoutPath() => _documentSession.PanelLayoutPath;
    public LayoutCanvas GetCanvas() => _documentSession.GetCanvas();
    public bool UpdateCanvasSize(double width, double height) => _documentSession.UpdateCanvasSize(width, height);
    public bool HasPanelSelection => _editingSession.HasPanelSelection;
    public IReadOnlyList<DashboardCatalogItem> GetCatalog() => _editingSession.GetCatalog();
    public LayoutSelectedItemProperties? GetSelectedItemProperties() => _editingSession.GetSelectedItemProperties();
    public WidgetCornerSettings? GetSelectedWidgetCornerSettings() => _editingSession.GetSelectedWidgetCornerSettings();
    public bool UpdateSelectedWidgetCornerSettings(double topLeft, double topRight, double bottomRight, double bottomLeft) => _editingSession.UpdateSelectedWidgetCornerSettings(topLeft, topRight, bottomRight, bottomLeft);
    public ShiftLedDashboardSettings? GetSelectedShiftLedSettings() => _editingSession.GetSelectedShiftLedSettings();
    public bool UpdateSelectedShiftLedSettings(ShiftLedDashboardSettings settings) => _editingSession.UpdateSelectedShiftLedSettings(settings);
    public DecorativePanelDashboardSettings? GetSelectedDecorativePanelSettings() => _editingSession.GetSelectedDecorativePanelSettings();
    public bool UpdateSelectedDecorativePanelSettings(DecorativePanelDashboardSettings settings) => _editingSession.UpdateSelectedDecorativePanelSettings(settings);
    public bool UpdateSelectedItemProperties(double x, double y, double width, double height, int zIndex, bool isLocked) => _editingSession.UpdateSelectedItemProperties(x, y, width, height, zIndex, isLocked);
    public IReadOnlyList<LayoutEditorItem> GetLayoutItems() => _editingSession.GetLayoutItems();
    public void SelectItem(Guid? itemId) => _editingSession.SelectItem(itemId);
    public void SelectItems(IEnumerable<Guid> itemIds, Guid? primaryItemId = null) => _editingSession.SelectItems(itemIds, primaryItemId);
    public void ToggleItemSelection(Guid itemId) => _editingSession.ToggleItemSelection(itemId);
    public void SetPrimarySelection(Guid itemId) => _editingSession.SetPrimarySelection(itemId);
    public Guid? HitTestItemId(object? hitSource) => _documentSession.HitTestItemId(hitSource);
    public bool IsResizeHandleHit(object? hitSource, Guid itemId) => _documentSession.IsResizeHandleHit(hitSource, itemId);
    public bool IsLocked(Guid itemId) => _editingSession.IsLocked(itemId);
    public IReadOnlyList<Guid> GetItemsInSelectionRect(double x, double y, double width, double height, bool requireFullContainment = false) => _documentSession.GetItemsInSelectionRect(x, y, width, height, requireFullContainment);
    public bool AddItem(string typeId) => _editingSession.AddItem(typeId);
    public bool DeleteSelected() => _editingSession.DeleteSelected();
    public bool DuplicateSelected() => _editingSession.DuplicateSelected();
    public bool GroupSelectedItems() => _editingSession.GroupSelectedItems();
    public bool UngroupSelected() => _editingSession.UngroupSelected();
    public bool SetLockSelected(bool isLocked) => _editingSession.SetLockSelected(isLocked);
    public bool CopySelected() => _editingSession.CopySelected();
    public bool PasteClipboard() => _editingSession.PasteClipboard();
    public bool BringToFrontSelected() => _editingSession.BringToFrontSelected();
    public bool SendToBackSelected() => _editingSession.SendToBackSelected();
    public bool BringForwardSelected() => _editingSession.BringForwardSelected();
    public bool SendBackwardSelected() => _editingSession.SendBackwardSelected();
    public LayoutMoveResult MoveSelectedWithSnap(double deltaX, double deltaY, double canvasWidth, double canvasHeight, bool bypassSnap) => _editingSession.MoveSelectedWithSnap(deltaX, deltaY, canvasWidth, canvasHeight, bypassSnap);
    public LayoutMoveResult ResizeSelectedWithSnap(double deltaWidth, double deltaHeight, double canvasWidth, double canvasHeight, bool bypassSnap) => _editingSession.ResizeSelectedWithSnap(deltaWidth, deltaHeight, canvasWidth, canvasHeight, bypassSnap);
    public string GetLayoutPath() => _documentSession.LayoutPath;
    public PanelPresetDocument? CreateSelectedPanelPreset(string name, string category = "Custom") => _editingSession.CreateSelectedPanelPreset(name, category);
    public bool InsertPanelPreset(PanelPresetDocument preset, double x, double y) => _editingSession.InsertPanelPreset(preset, x, y);
    public bool OpenPanelPresetForEditing(PanelPresetDocument preset, double x, double y) => _editingSession.OpenPanelPresetForEditing(preset, x, y);
    public bool StartNewPanelLayout(string name = "Panel Layout") => _documentSession.StartNewPanelLayout(name);
    public bool OpenPanelLayout(string path) => _documentSession.OpenPanelLayout(path);
    public bool SavePanelLayout(string path) => _documentSession.SavePanelLayout(path, _panelLayoutEditingService);
    public bool InsertPanelPresetAsPanelInstance(PanelPresetDocument preset, double x, double y) => _editingSession.InsertPanelPresetAsPanelInstance(preset, x, y);
    public void EndDrag() => _editingSession.EndDrag();
    public void SaveLayout() => _documentSession.SaveLayout(_panelLayoutEditingService);
    public void ReloadLayout() => _documentSession.ReloadLayout();
}
