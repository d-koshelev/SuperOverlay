using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorResizeService
{
    private const double MinWidth = 120;
    private const double MinHeight = 72;

    public static void BeginWidgetResize(LayoutEditorState state, IReadOnlyCollection<LayoutEditorWidget> selectedWidgets, Point startPointer)
    {
        state.IsResizingWidgets = true;
        state.WidgetResizeStartPointer = startPointer;
        state.ResizeStartSizes.Clear();

        foreach (var selectedWidget in selectedWidgets)
        {
            state.ResizeStartSizes[selectedWidget.Id] = new Size(selectedWidget.Width, selectedWidget.Height);
        }
    }

    public static void UpdateWidgetResize(LayoutEditorState state, IReadOnlyCollection<LayoutEditorWidget> selectedWidgets, Point pointer, Size viewport)
    {
        var deltaX = pointer.X - state.WidgetResizeStartPointer.X;
        var deltaY = pointer.Y - state.WidgetResizeStartPointer.Y;

        foreach (var selected in selectedWidgets.Take(1))
        {
            if (!state.ResizeStartSizes.TryGetValue(selected.Id, out var start))
            {
                continue;
            }

            selected.Width = LayoutEditorMath.Clamp(start.Width + deltaX, MinWidth, System.Math.Max(MinWidth, viewport.Width - selected.X));
            selected.Height = LayoutEditorMath.Clamp(start.Height + deltaY, MinHeight, System.Math.Max(MinHeight, viewport.Height - selected.Y));
        }
    }

    public static void EndWidgetResize(LayoutEditorState state)
    {
        state.IsResizingWidgets = false;
        state.ResizeStartSizes.Clear();
    }
}
