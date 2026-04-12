using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SuperOverlay.Core.Layouts.Runtime;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.Core.Layouts.Editing;

namespace SuperOverlay.iRacing.Runtime;

internal sealed class RuntimeCanvasController
{
    private readonly Func<OverlayRuntimeSession?> _getSession;
    private readonly Grid _rootGrid;
    private LayoutEditorInteractionController? _interactionController;

    public RuntimeCanvasController(Func<OverlayRuntimeSession?> getSession, Grid rootGrid)
    {
        _getSession = getSession ?? throw new ArgumentNullException(nameof(getSession));
        _rootGrid = rootGrid ?? throw new ArgumentNullException(nameof(rootGrid));
    }

    public void UpdateShellMode(bool isMoveEditEnabled)
    {
        _interactionController = isMoveEditEnabled && _getSession() is OverlayRuntimeSession session
            ? new LayoutEditorInteractionController(session, LayoutInteractionOptions.RuntimeMoveOnly)
            : null;
    }

    public bool HandleMouseLeftButtonDown(DependencyObject? originalSource)
    {
        var session = _getSession();
        if (session is null || _interactionController is null)
        {
            return false;
        }

        var startResult = _interactionController.BeginInteraction(originalSource, Mouse.GetPosition(_rootGrid), Keyboard.Modifiers);
        if (!startResult.CaptureMouse)
        {
            return false;
        }

        _rootGrid.CaptureMouse();
        return true;
    }

    public void HandleMouseMove(WpfMouseEventArgs e)
    {
        if (_interactionController is null)
        {
            return;
        }

        if (!_interactionController.IsInteracting || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        _interactionController.MoveInteraction(e.GetPosition(_rootGrid), _rootGrid.ActualWidth, _rootGrid.ActualHeight, true);
    }

    public bool HandleMouseLeftButtonUp()
    {
        if (_interactionController is null)
        {
            return false;
        }

        var endResult = _interactionController.EndInteraction(Keyboard.Modifiers);
        if (!endResult.Ended)
        {
            return false;
        }

        _rootGrid.ReleaseMouseCapture();
        return true;
    }
}
