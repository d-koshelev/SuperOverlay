using System.Linq;
using System.Windows;
using System.Windows.Input;
using SuperOverlay.Core.Layouts.Editing;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    private LayoutEditorWidget? ResolveWidgetFromSource(DependencyObject? source)
    {
        return LayoutEditorVisualTreeService.ResolveWidgetFromSource(source);
    }

    private void HandleWidgetLeftClick(LayoutEditorWidget widget, MouseButtonEventArgs e)
    {
        var modifiers = Keyboard.Modifiers;
        var preserveSelection = modifiers.HasFlag(ModifierKeys.Alt) || modifiers.HasFlag(ModifierKeys.Shift);
        var wasSelected = widget.IsSelected;

        if (!wasSelected)
        {
            _selection.EnsureWidgetSelected(widget);
            SyncEngineSelection();
        }
        else if (preserveSelection)
        {
            _selection.EnsureWidgetSelected(widget);
            SyncEngineSelection();
        }

        _state.IsPendingWidgetClick = true;
        _state.PendingWidgetClickTarget = widget;
        _state.PendingWidgetWasSelected = wasSelected;
        _state.PendingWidgetPreserveSelection = preserveSelection;
        _state.WidgetDragStartPointer = e.GetPosition(RootGrid);
    }

    private void BeginWidgetDrag(LayoutEditorWidget widget, Point startPointer)
    {
        var selected = _selection.EnsureWidgetSelected(widget);
        SyncEngineSelection();
        _manipulation.BeginWidgetDrag(selected, startPointer);
        RootGrid.CaptureMouse();
    }


    private void ToggleWidgetSelection(LayoutEditorWidget widget)
    {
        _selection.ToggleWidgetSelection(widget);
        SyncEngineSelection();
    }

    private void HandleWidgetResizeClick(LayoutEditorWidget widget, MouseButtonEventArgs e)
    {
        _state.IsPendingWidgetClick = false;
        _state.PendingWidgetClickTarget = null;
        var selected = _selection.EnsureWidgetSelected(widget);
        SyncEngineSelection();
        _manipulation.BeginWidgetResize(selected, e.GetPosition(RootGrid));
        RootGrid.CaptureMouse();
    }

    private void SelectWidgets(IReadOnlyCollection<LayoutEditorWidget> widgetsToSelect, LayoutEditorWidget? primaryWidget)
    {
        _selection.SelectWidgets(widgetsToSelect, primaryWidget);
        SyncEngineSelection();
    }

    private void RefreshSelectionDetails()
    {
        _selection.RefreshSelectionDetails();
    }

    private void UpdateDraggedWidgets(Point pointer)
    {
        var selectedWidgets = SelectedWidgets;
        if (_engine is null || selectedWidgets.Count > 1)
        {
            _selection.UpdateDraggedWidgets(pointer, new Size(RootGrid.ActualWidth, RootGrid.ActualHeight));
            SyncEngineFromWidgets();
            return;
        }

        SyncEngineFromWidgets();

        var deltaX = pointer.X - _state.WidgetDragStartPointer.X;
        var deltaY = pointer.Y - _state.WidgetDragStartPointer.Y;
        if (System.Math.Abs(deltaX) < double.Epsilon && System.Math.Abs(deltaY) < double.Epsilon)
        {
            return;
        }

        var useSnap = _snapPolicy.IsInteractionSnapEnabled();
        _ = UpdateGuides(_engine.MoveSelection(deltaX, deltaY, new Size(RootGrid.ActualWidth, RootGrid.ActualHeight), !useSnap));
        ApplyEngineSnapshot(_engine.GetSnapshot());
        _state.WidgetDragStartPointer = pointer;
    }


    private void UpdateResizedWidgets(Point pointer)
    {
        if (_engine is null)
        {
            _manipulation.UpdateWidgetResize(SelectedWidgets, pointer, new Size(RootGrid.ActualWidth, RootGrid.ActualHeight));
            return;
        }

        SyncEngineFromWidgets();

        var deltaX = pointer.X - _state.WidgetResizeStartPointer.X;
        var deltaY = pointer.Y - _state.WidgetResizeStartPointer.Y;
        if (System.Math.Abs(deltaX) < double.Epsilon && System.Math.Abs(deltaY) < double.Epsilon)
        {
            return;
        }

        var useSnap = _snapPolicy.IsInteractionSnapEnabled();
        _ = UpdateGuides(_engine.ResizeSelection(deltaX, deltaY, new Size(RootGrid.ActualWidth, RootGrid.ActualHeight), !useSnap));
        ApplyEngineSnapshot(_engine.GetSnapshot());
        _state.WidgetResizeStartPointer = pointer;
    }


    private void EndWidgetDrag()
    {
        _manipulation.EndWidgetDrag();
    }

    private void EndWidgetResize()
    {
        _manipulation.EndWidgetResize();
    }

    private void SyncEngineFromWidgets()
    {
        _engine?.SyncWidgets(LayoutEditorSelectionVisualPresenter.BuildEngineInputs(Widgets), new Size(RootGrid.ActualWidth, RootGrid.ActualHeight));
        SyncEngineSelection();
    }

    private void SyncEngineSelection()
    {
        _engine?.SyncSelection(SelectedWidgets.Select(x => x.Id).ToList(), _state.PrimarySelectedWidget?.Id);
    }

    private void ApplyEngineSnapshot(LayoutEditorEngineSnapshot snapshot)
    {
        _selectionVisuals.ApplyEngineSnapshot(snapshot);
    }

}
