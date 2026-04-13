using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Windows;

namespace SuperOverlay.iRacing.Runtime;

internal sealed class RuntimeWindowInteractionController
{
    public RuntimeWindowInteractionController(
        RuntimeWindowController runtimeController,
        RuntimeCanvasController canvasController,
        Action focusWindow)
    {
    }

    public void Initialize()
    {
    }

    public bool HandleKeyDown(WpfKeyEventArgs e) => false;

    public bool HandleMouseLeftButtonDown(WpfMouseButtonEventArgs e) => false;

    public void HandleMouseMove(WpfMouseEventArgs e)
    {
    }

    public bool HandleMouseLeftButtonUp(WpfMouseButtonEventArgs e) => false;
}
