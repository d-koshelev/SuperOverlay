using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorManipulationService
{
    private readonly LayoutEditorState _state;
    private const double MinWidth = 120;
    private const double MinHeight = 72;

    public LayoutEditorManipulationService(LayoutEditorState state)
    {
        _state = state;
    }

    public void BeginWidgetDrag(IReadOnlyCollection<LayoutEditorWidget> selectedWidgets, Point startPointer)
    {
        _state.IsDraggingWidgets = true;
        _state.WidgetDragStartPointer = startPointer;
        _state.DragStartPositions.Clear();

        foreach (var selectedWidget in selectedWidgets)
        {
            _state.DragStartPositions[selectedWidget.Id] = new Point(selectedWidget.X, selectedWidget.Y);
        }
    }

    public void UpdateWidgetDrag(IReadOnlyCollection<LayoutEditorWidget> selectedWidgets, Point pointer, Size viewport)
    {
        var deltaX = pointer.X - _state.WidgetDragStartPointer.X;
        var deltaY = pointer.Y - _state.WidgetDragStartPointer.Y;

        foreach (var selected in selectedWidgets)
        {
            if (!_state.DragStartPositions.TryGetValue(selected.Id, out var start))
            {
                continue;
            }

            selected.X = LayoutEditorMath.Clamp(start.X + deltaX, 0, System.Math.Max(0, viewport.Width - selected.Width));
            selected.Y = LayoutEditorMath.Clamp(start.Y + deltaY, 0, System.Math.Max(0, viewport.Height - selected.Height));
        }
    }

    public void EndWidgetDrag()
    {
        _state.IsDraggingWidgets = false;
        _state.DragStartPositions.Clear();
    }

    public void BeginWidgetResize(IReadOnlyCollection<LayoutEditorWidget> selectedWidgets, Point startPointer)
    {
        _state.IsResizingWidgets = true;
        _state.WidgetResizeStartPointer = startPointer;
        _state.ResizeStartSizes.Clear();

        foreach (var selectedWidget in selectedWidgets)
        {
            _state.ResizeStartSizes[selectedWidget.Id] = new Size(selectedWidget.Width, selectedWidget.Height);
        }
    }

    public void UpdateWidgetResize(IReadOnlyCollection<LayoutEditorWidget> selectedWidgets, Point pointer, Size viewport)
    {
        var deltaX = pointer.X - _state.WidgetResizeStartPointer.X;
        var deltaY = pointer.Y - _state.WidgetResizeStartPointer.Y;

        foreach (var selected in selectedWidgets.Take(1))
        {
            if (!_state.ResizeStartSizes.TryGetValue(selected.Id, out var start))
            {
                continue;
            }

            selected.Width = LayoutEditorMath.Clamp(start.Width + deltaX, MinWidth, System.Math.Max(MinWidth, viewport.Width - selected.X));
            selected.Height = LayoutEditorMath.Clamp(start.Height + deltaY, MinHeight, System.Math.Max(MinHeight, viewport.Height - selected.Y));
        }
    }

    public void EndWidgetResize()
    {
        _state.IsResizingWidgets = false;
        _state.ResizeStartSizes.Clear();
    }
}
