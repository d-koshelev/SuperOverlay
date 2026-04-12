using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorDragService
{
    public static void BeginWidgetDrag(LayoutEditorState state, IReadOnlyCollection<LayoutEditorWidget> selectedWidgets, Point startPointer)
    {
        state.IsDraggingWidgets = true;
        state.WidgetDragStartPointer = startPointer;
        state.DragStartPositions.Clear();

        foreach (var selectedWidget in selectedWidgets)
        {
            state.DragStartPositions[selectedWidget.Id] = new Point(selectedWidget.X, selectedWidget.Y);
        }
    }

    public static void UpdateWidgetDrag(LayoutEditorState state, IReadOnlyCollection<LayoutEditorWidget> selectedWidgets, Point pointer, Size viewport)
    {
        var deltaX = pointer.X - state.WidgetDragStartPointer.X;
        var deltaY = pointer.Y - state.WidgetDragStartPointer.Y;

        foreach (var selected in selectedWidgets)
        {
            if (!state.DragStartPositions.TryGetValue(selected.Id, out var start))
            {
                continue;
            }

            selected.X = LayoutEditorMath.Clamp(start.X + deltaX, 0, System.Math.Max(0, viewport.Width - selected.Width));
            selected.Y = LayoutEditorMath.Clamp(start.Y + deltaY, 0, System.Math.Max(0, viewport.Height - selected.Height));
        }
    }

    public static void EndWidgetDrag(LayoutEditorState state)
    {
        state.IsDraggingWidgets = false;
        state.DragStartPositions.Clear();
    }
}
