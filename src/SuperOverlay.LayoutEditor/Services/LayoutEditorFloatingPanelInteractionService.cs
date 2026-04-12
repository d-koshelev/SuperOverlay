using System;
using System.Windows;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorFloatingPanelInteractionService
{
    private readonly LayoutEditorState _state;
    private readonly Action<double, double> _moveFloatingMenu;
    private readonly Action<double, double> _movePropertiesPanel;

    public LayoutEditorFloatingPanelInteractionService(
        LayoutEditorState state,
        Action<double, double> moveFloatingMenu,
        Action<double, double> movePropertiesPanel)
    {
        _state = state;
        _moveFloatingMenu = moveFloatingMenu;
        _movePropertiesPanel = movePropertiesPanel;
    }

    public bool IsDraggingAnyPanel => _state.IsDraggingToolbar || _state.IsDraggingProperties;

    public void BeginToolbarDrag(Point dragOffset)
    {
        LayoutEditorFloatingPanelDragService.BeginToolbarDrag(_state, dragOffset);
    }

    public void BeginPropertiesDrag(Point dragOffset)
    {
        LayoutEditorFloatingPanelDragService.BeginPropertiesDrag(_state, dragOffset);
    }

    public bool Update(Point pointer)
    {
        if (_state.IsDraggingToolbar)
        {
            var target = LayoutEditorFloatingPanelDragService.ResolveToolbarPosition(_state, pointer);
            _moveFloatingMenu(target.X, target.Y);
            return true;
        }

        if (_state.IsDraggingProperties)
        {
            var target = LayoutEditorFloatingPanelDragService.ResolvePropertiesPosition(_state, pointer);
            _movePropertiesPanel(target.X, target.Y);
            return true;
        }

        return false;
    }

    public bool EndToolbarDrag()
    {
        return LayoutEditorFloatingPanelDragService.EndToolbarDrag(_state);
    }

    public bool EndPropertiesDrag()
    {
        return LayoutEditorFloatingPanelDragService.EndPropertiesDrag(_state);
    }
}
