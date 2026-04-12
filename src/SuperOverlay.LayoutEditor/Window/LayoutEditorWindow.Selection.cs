using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    private LayoutEditorWidget? ResolveWidgetFromSource(DependencyObject? source)
    {
        return LayoutEditorVisualTreeService.ResolveWidgetFromSource(source);
    }

    private void HandleWidgetLeftClick(LayoutEditorWidget widget, MouseButtonEventArgs e)
    {
        var selected = _selection.PrepareWidgetSelection(widget);
        SyncEngineSelection();
        LayoutEditorDragService.BeginWidgetDrag(_state, selected, e.GetPosition(RootGrid));
        RootGrid.CaptureMouse();
    }

    private void HandleWidgetResizeClick(LayoutEditorWidget widget, MouseButtonEventArgs e)
    {
        var selected = _selection.PrepareWidgetSelection(widget);
        SyncEngineSelection();
        LayoutEditorResizeService.BeginWidgetResize(_state, selected, e.GetPosition(RootGrid));
        RootGrid.CaptureMouse();
    }

    private void SelectWidgets(IReadOnlyCollection<LayoutEditorWidget> widgetsToSelect, LayoutEditorWidget? primaryWidget)
    {
        if (_engine is not null)
        {
            _engine.SyncWidgets(Widgets, new Size(RootGrid.ActualWidth, RootGrid.ActualHeight));
            var normalizedIds = _engine.NormalizeSelection(widgetsToSelect.Select(x => x.Id).ToList(), primaryWidget?.Id);
            widgetsToSelect = Widgets.Where(x => normalizedIds.Contains(x.Id)).ToList();
            primaryWidget = primaryWidget is not null && normalizedIds.Contains(primaryWidget.Id)
                ? primaryWidget
                : widgetsToSelect.FirstOrDefault();
        }

        _selection.SelectWidgets(widgetsToSelect, primaryWidget);
        SyncEngineSelection();
    }

    private void RefreshSelectionDetails()
    {
        _selection.RefreshSelectionDetails();
    }

    private void UpdateDraggedWidgets(Point pointer)
    {
        if (_engine is null)
        {
            _selection.UpdateDraggedWidgets(pointer, new Size(RootGrid.ActualWidth, RootGrid.ActualHeight));
            return;
        }

        SyncEngineFromWidgets();

        var deltaX = pointer.X - _state.WidgetDragStartPointer.X;
        var deltaY = pointer.Y - _state.WidgetDragStartPointer.Y;
        if (System.Math.Abs(deltaX) < double.Epsilon && System.Math.Abs(deltaY) < double.Epsilon)
        {
            return;
        }

        _ = UpdateGuides(_engine.MoveSelection(deltaX, deltaY, new Size(RootGrid.ActualWidth, RootGrid.ActualHeight), !_state.IsSnappingEnabled));
        ApplyEngineSnapshot(_engine.GetSnapshot());
        _state.WidgetDragStartPointer = pointer;
    }


    private void UpdateResizedWidgets(Point pointer)
    {
        if (_engine is null)
        {
            LayoutEditorResizeService.UpdateWidgetResize(_state, SelectedWidgets, pointer, new Size(RootGrid.ActualWidth, RootGrid.ActualHeight));
            return;
        }

        SyncEngineFromWidgets();

        var deltaX = pointer.X - _state.WidgetResizeStartPointer.X;
        var deltaY = pointer.Y - _state.WidgetResizeStartPointer.Y;
        if (System.Math.Abs(deltaX) < double.Epsilon && System.Math.Abs(deltaY) < double.Epsilon)
        {
            return;
        }

        _ = UpdateGuides(_engine.ResizeSelection(deltaX, deltaY, new Size(RootGrid.ActualWidth, RootGrid.ActualHeight), !_state.IsSnappingEnabled));
        ApplyEngineSnapshot(_engine.GetSnapshot());
        _state.WidgetResizeStartPointer = pointer;
    }

    private void SyncEngineFromWidgets()
    {
        _engine?.SyncWidgets(Widgets, new Size(RootGrid.ActualWidth, RootGrid.ActualHeight));
        SyncEngineSelection();
    }

    private void SyncEngineSelection()
    {
        _engine?.SyncSelection(SelectedWidgets.Select(x => x.Id).ToList(), _state.PrimarySelectedWidget?.Id);
    }

    private void ApplyEngineSnapshot(LayoutEditorEngineSnapshot snapshot)
    {
        var snapshotIds = snapshot.Widgets.Keys.ToHashSet();
        foreach (var removed in Widgets.Where(x => !snapshotIds.Contains(x.Id)).ToList())
        {
            Widgets.Remove(removed);
        }

        var groupMap = new Dictionary<Guid, Guid>();
        foreach (var widget in Widgets)
        {
            if (!snapshot.Widgets.TryGetValue(widget.Id, out var engineWidget))
            {
                continue;
            }

            widget.X = engineWidget.X;
            widget.Y = engineWidget.Y;
            widget.Width = engineWidget.Width;
            widget.Height = engineWidget.Height;
            widget.ZIndex = engineWidget.ZIndex;
            widget.IsLocked = engineWidget.IsLocked;
            widget.IsSelected = snapshot.SelectedIds.Contains(widget.Id);
            widget.IsVisibleInCurrentMode = !_state.IsRaceMode || widget.ShowInRace;

            if (engineWidget.LinkedGroupKey.HasValue)
            {
                if (!groupMap.TryGetValue(engineWidget.LinkedGroupKey.Value, out var localGroupId))
                {
                    localGroupId = engineWidget.LinkedGroupKey.Value;
                    groupMap[engineWidget.LinkedGroupKey.Value] = localGroupId;
                }

                widget.GroupId = localGroupId;
            }
            else
            {
                widget.GroupId = null;
            }
        }

        _state.PrimarySelectedWidget = snapshot.PrimarySelectedId is null
            ? null
            : Widgets.FirstOrDefault(x => x.Id == snapshot.PrimarySelectedId.Value);

        ReorderWidgetsByZIndex();
        RefreshSelectionDetails();
    }

    private void ReorderWidgetsByZIndex()
    {
        var ordered = Widgets.OrderBy(x => x.ZIndex).ThenBy(x => x.Id).ToList();
        for (var targetIndex = 0; targetIndex < ordered.Count; targetIndex++)
        {
            var widget = ordered[targetIndex];
            var currentIndex = Widgets.IndexOf(widget);
            if (currentIndex >= 0 && currentIndex != targetIndex)
            {
                Widgets.Move(currentIndex, targetIndex);
            }
        }
    }
}
