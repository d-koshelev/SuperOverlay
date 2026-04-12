using System.Windows;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorFloatingPanelDragService
{
    public static void BeginToolbarDrag(LayoutEditorState state, Point dragOffset)
    {
        state.IsDraggingToolbar = true;
        state.ToolbarDragOffset = dragOffset;
    }

    public static void BeginPropertiesDrag(LayoutEditorState state, Point dragOffset)
    {
        state.IsDraggingProperties = true;
        state.PropertiesDragOffset = dragOffset;
    }

    public static Point ResolveToolbarPosition(LayoutEditorState state, Point pointer)
    {
        return new Point(pointer.X - state.ToolbarDragOffset.X, pointer.Y - state.ToolbarDragOffset.Y);
    }

    public static Point ResolvePropertiesPosition(LayoutEditorState state, Point pointer)
    {
        return new Point(pointer.X - state.PropertiesDragOffset.X, pointer.Y - state.PropertiesDragOffset.Y);
    }

    public static bool EndToolbarDrag(LayoutEditorState state)
    {
        if (!state.IsDraggingToolbar)
        {
            return false;
        }

        state.IsDraggingToolbar = false;
        return true;
    }

    public static bool EndPropertiesDrag(LayoutEditorState state)
    {
        if (!state.IsDraggingProperties)
        {
            return false;
        }

        state.IsDraggingProperties = false;
        return true;
    }
}
