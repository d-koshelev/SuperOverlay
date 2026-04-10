using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Windows;
using System.Windows.Input;

namespace SuperOverlay.iRacing.Runtime;

internal sealed class RuntimeWindowInteractionController
{
    private readonly RuntimeWindowController _runtimeController;
    private readonly RuntimeCanvasController _canvasController;
    private readonly Action _focusWindow;

    public RuntimeWindowInteractionController(
        RuntimeWindowController runtimeController,
        RuntimeCanvasController canvasController,
        Action focusWindow)
    {
        _runtimeController = runtimeController ?? throw new ArgumentNullException(nameof(runtimeController));
        _canvasController = canvasController ?? throw new ArgumentNullException(nameof(canvasController));
        _focusWindow = focusWindow ?? throw new ArgumentNullException(nameof(focusWindow));
    }

    public void Initialize()
    {
        _canvasController.UpdateShellMode(_runtimeController.IsMoveEditEnabled);
    }

    public bool HandleKeyDown(WpfKeyEventArgs e)
    {
        if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.E)
        {
            _runtimeController.ToggleMoveEdit();
            RefreshShellMode();
            e.Handled = true;
            return true;
        }

        if (!_runtimeController.IsMoveEditEnabled)
        {
            return false;
        }

        if (e.Key == Key.Enter)
        {
            _runtimeController.ApplyMoveEdit();
            RefreshShellMode();
            e.Handled = true;
            return true;
        }

        if (e.Key == Key.Escape)
        {
            _runtimeController.CancelMoveEdit();
            RefreshShellMode();
            e.Handled = true;
            return true;
        }

        return false;
    }

    public bool HandleMouseLeftButtonDown(WpfMouseButtonEventArgs e)
    {
        if (!_runtimeController.IsMoveEditEnabled)
        {
            return false;
        }

        _focusWindow();
        if (!_canvasController.HandleMouseLeftButtonDown(e.OriginalSource as DependencyObject))
        {
            return false;
        }

        e.Handled = true;
        return true;
    }

    public void HandleMouseMove(WpfMouseEventArgs e)
    {
        if (!_runtimeController.IsMoveEditEnabled)
        {
            return;
        }

        _canvasController.HandleMouseMove(e);
    }

    public bool HandleMouseLeftButtonUp(WpfMouseButtonEventArgs e)
    {
        if (!_runtimeController.IsMoveEditEnabled)
        {
            return false;
        }

        if (!_canvasController.HandleMouseLeftButtonUp())
        {
            return false;
        }

        e.Handled = true;
        return true;
    }

    public void ApplyMoveEdit()
    {
        _runtimeController.ApplyMoveEdit();
        RefreshShellMode();
    }

    public void CancelMoveEdit()
    {
        _runtimeController.CancelMoveEdit();
        RefreshShellMode();
    }

    private void RefreshShellMode()
    {
        _canvasController.UpdateShellMode(_runtimeController.IsMoveEditEnabled);
    }
}
