using System.Windows;
using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.Persistence;
using SuperOverlay.Core.Layouts.PanelLayouts;
using SuperOverlay.Core.Layouts.Runtime;
using SuperOverlay.Core.Layouts.Editing;
using SuperOverlay.Core.Layouts.Editor;

namespace SuperOverlay.iRacing.Hosting;

internal sealed class OverlayDocumentSessionService
{
    private readonly OverlaySessionState _state;
    private readonly LayoutFileStore _fileStore;
    private readonly string _layoutPath;
    private readonly OverlayShellMode _shellMode;
    private readonly PanelLayoutSessionService _panelLayoutService;
    private readonly OverlayRuntimeSyncService _runtimeSyncService;
    private readonly OverlaySelectionState _selection;
    private readonly LayoutSelectionPresentationService _selectionPresentationService;

    public OverlayDocumentSessionService(
        OverlaySessionState state,
        LayoutFileStore fileStore,
        string layoutPath,
        OverlayShellMode shellMode,
        PanelLayoutSessionService panelLayoutService,
        OverlayRuntimeSyncService runtimeSyncService,
        OverlaySelectionState selection,
        LayoutSelectionPresentationService selectionPresentationService)
    {
        _state = state;
        _fileStore = fileStore;
        _layoutPath = layoutPath;
        _shellMode = shellMode;
        _panelLayoutService = panelLayoutService;
        _runtimeSyncService = runtimeSyncService;
        _selection = selection;
        _selectionPresentationService = selectionPresentationService;
    }

    public string LayoutPath => _layoutPath;
    public string? PanelLayoutPath => _state.PanelLayoutPath;
    public bool HasPanelLayout => _panelLayoutService.HasPanelLayout(_state.PanelLayout);
    public LayoutCanvas GetCanvas() => _state.Layout.Canvas;
    public void Update(object runtimeState) => _runtimeSyncService.LayoutHostUpdate(runtimeState);
    public Guid? HitTestItemId(object? hitSource) => _runtimeSyncService.HitTestItemId(hitSource);

    public bool IsResizeHandleHit(object? hitSource, Guid itemId)
    {
        if (_state.PanelLayout is not null)
        {
            return false;
        }

        return _runtimeSyncService.IsResizeHandleHit(hitSource, itemId);
    }

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

        if (_state.PanelLayout is not null)
        {
            if (_state.PanelLayout.Canvas.Width != canvas.Width || _state.PanelLayout.Canvas.Height != canvas.Height)
            {
                _state.PanelLayout = _state.PanelLayout with { Canvas = canvas };
                changed = true;
            }
        }

        if (_state.Layout.Canvas.Width != canvas.Width || _state.Layout.Canvas.Height != canvas.Height)
        {
            _state.Layout = _state.Layout with { Canvas = canvas };
            changed = true;
        }

        if (!changed)
        {
            return true;
        }

        RefreshRuntime();
        return true;
    }

    public IReadOnlyList<Guid> GetItemsInSelectionRect(double x, double y, double width, double height, bool requireFullContainment = false)
    {
        var left = Math.Min(x, x + width);
        var top = Math.Min(y, y + height);
        var right = Math.Max(x, x + width);
        var bottom = Math.Max(y, y + height);

        return _state.Layout.Placements
            .Select(x => LayoutPlacementResolver.ResolveForShell(x, _state.Layout.Canvas, _shellMode))
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

    public bool StartNewPanelLayout(string name = "Panel Layout")
    {
        _state.PanelLayout = _panelLayoutService.CreateNew(name, _state.Layout);
        _state.PanelLayoutPath = null;
        return RecompilePanelLayout(selectLastPanel: false, preserveSelection: false);
    }

    public bool OpenPanelLayout(string path)
    {
        _state.PanelLayout = _panelLayoutService.Load(path);
        _state.PanelLayoutPath = path;
        return RecompilePanelLayout(selectLastPanel: false, preserveSelection: false);
    }

    public bool SavePanelLayout(string path, PanelLayoutEditingService panelLayoutEditingService)
    {
        if (_state.PanelLayout is null)
        {
            return false;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _state.PanelLayout = panelLayoutEditingService.SyncPanelLayoutFromCompiledLayout(_state.PanelLayout, _state.Layout, _state.CompiledPanelItemMap);
        _panelLayoutService.Save(path, _state.PanelLayout);
        _state.PanelLayoutPath = path;
        return true;
    }

    public void SaveLayout(PanelLayoutEditingService panelLayoutEditingService)
    {
        if (_state.PanelLayout is not null && !string.IsNullOrWhiteSpace(_state.PanelLayoutPath))
        {
            SavePanelLayout(_state.PanelLayoutPath, panelLayoutEditingService);
        }

        _fileStore.Save(_layoutPath, _state.Layout);
    }

    public void ReloadLayout()
    {
        _state.PanelLayout = null;
        _state.PanelLayoutPath = null;
        _state.CompiledPanelItemMap = new Dictionary<Guid, IReadOnlyList<Guid>>();
        _state.Layout = _fileStore.Load(_layoutPath);
        var validIds = _state.Layout.Items.Select(x => x.Id).ToHashSet();
        _selection.Replace(_selection.ItemIds.Where(validIds.Contains), _selection.PrimaryItemId);
        RefreshRuntime();
    }

    public bool RecompilePanelLayout(bool selectLastPanel, bool preserveSelection = false)
    {
        if (_state.PanelLayout is null)
        {
            return false;
        }

        _state.Layout = _panelLayoutService.Compile(_state.PanelLayout, out var panelItemMap);

        var previousSelectedPanelIds = preserveSelection ? _selectionPresentationService.GetSelectedPanelIds(_selection, _state.PanelLayout, _state.CompiledPanelItemMap).ToHashSet() : new HashSet<Guid>();
        _state.CompiledPanelItemMap = panelItemMap;
        RefreshRuntime();

        if (selectLastPanel && _state.PanelLayout.Panels.Count > 0)
        {
            var panelId = _state.PanelLayout.Panels.Last().Id;
            if (_state.CompiledPanelItemMap.TryGetValue(panelId, out var itemIds) && itemIds.Count > 0)
            {
                _selection.Replace(itemIds, itemIds.Last());
                SyncSelectionState();
            }
        }
        else if (preserveSelection && previousSelectedPanelIds.Count > 0)
        {
            var itemIds = previousSelectedPanelIds
                .Where(x => _state.CompiledPanelItemMap.ContainsKey(x))
                .SelectMany(x => _state.CompiledPanelItemMap[x])
                .ToList();

            if (itemIds.Count > 0)
            {
                _selection.Replace(itemIds, itemIds.Last());
            }
            else
            {
                _selection.Clear();
            }

            SyncSelectionState();
        }
        else
        {
            _selection.Clear();
            SyncSelectionState();
        }

        return true;
    }

    public void RefreshRuntime() => _runtimeSyncService.RefreshRuntime(_state.Layout, _selection);
    public void SyncSelectionState() => _runtimeSyncService.SyncSelectionState(_state.Layout, _selection);
    public void SyncAllPlacementsToRuntime() => _runtimeSyncService.SyncAllPlacementsToRuntime(_state.Layout);
}
