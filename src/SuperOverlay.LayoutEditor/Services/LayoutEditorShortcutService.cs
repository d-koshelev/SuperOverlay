using System;
using System.Windows.Input;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorShortcutService
{
    private readonly LayoutEditorState _state;
    private readonly Action _deleteSelection;
    private readonly Action _duplicateSelection;
    private readonly Action _groupSelection;
    private readonly Action _ungroupSelection;
    private readonly Action _toggleLockSelection;
    private readonly Action _bringToFront;
    private readonly Action _sendToBack;
    private readonly Action _bringForward;
    private readonly Action _sendBackward;
    private readonly Action<bool> _setRaceMode;
    private readonly Action _cancelPlacement;

    public LayoutEditorShortcutService(
        LayoutEditorState state,
        Action deleteSelection,
        Action duplicateSelection,
        Action groupSelection,
        Action ungroupSelection,
        Action toggleLockSelection,
        Action bringToFront,
        Action sendToBack,
        Action bringForward,
        Action sendBackward,
        Action<bool> setRaceMode,
        Action cancelPlacement)
    {
        _state = state;
        _deleteSelection = deleteSelection;
        _duplicateSelection = duplicateSelection;
        _groupSelection = groupSelection;
        _ungroupSelection = ungroupSelection;
        _toggleLockSelection = toggleLockSelection;
        _bringToFront = bringToFront;
        _sendToBack = sendToBack;
        _bringForward = bringForward;
        _sendBackward = sendBackward;
        _setRaceMode = setRaceMode;
        _cancelPlacement = cancelPlacement;
    }

    public bool TryHandle(KeyEventArgs e)
    {
        if ((_state.IsPlacingPreset || _state.IsPlacingWidget) && e.Key == Key.Escape)
        {
            _cancelPlacement();
            return true;
        }

        if (_state.IsRaceMode && e.Key == Key.Escape)
        {
            _setRaceMode(false);
            return true;
        }

        var ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        var shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

        if (e.Key == Key.Delete)
        {
            _deleteSelection();
            return true;
        }

        if (ctrl && e.Key == Key.D)
        {
            _duplicateSelection();
            return true;
        }

        if (ctrl && !shift && e.Key == Key.G)
        {
            _groupSelection();
            return true;
        }

        if (ctrl && shift && e.Key == Key.G)
        {
            _ungroupSelection();
            return true;
        }

        if (ctrl && e.Key == Key.L)
        {
            _toggleLockSelection();
            return true;
        }

        if (ctrl && shift && e.Key == Key.PageUp)
        {
            _bringToFront();
            return true;
        }

        if (ctrl && shift && e.Key == Key.PageDown)
        {
            _sendToBack();
            return true;
        }

        if (ctrl && e.Key == Key.PageUp)
        {
            _bringForward();
            return true;
        }

        if (ctrl && e.Key == Key.PageDown)
        {
            _sendBackward();
            return true;
        }

        return false;
    }
}
