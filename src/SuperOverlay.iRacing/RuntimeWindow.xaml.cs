using WpfWindow = System.Windows.Window;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Windows;
using System.Windows.Input;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Runtime;
using SuperOverlay.iRacing.Telemetry.Mock;

namespace SuperOverlay.iRacing;

public partial class RuntimeWindow : WpfWindow
{
    private readonly MockTelemetryProvider _telemetry = new();
    private readonly IRacingMapper _mapper = new();
    private readonly Hosting.OverlayRuntimeBootstrapper _bootstrapper = new();

    private readonly RuntimeWindowController _runtimeController;
    private readonly RuntimeCanvasController _canvasController;

    public RuntimeWindow()
    {
        InitializeComponent();

        _runtimeController = new RuntimeWindowController(this, RootGrid, RuntimeHintBorder, EditOverlayBar, _telemetry, _mapper, _bootstrapper);
        _canvasController = new RuntimeCanvasController(() => _runtimeController.Session, RootGrid);

        _runtimeController.Initialize();
        _canvasController.UpdateShellMode(_runtimeController.IsMoveEditEnabled);
        Loaded += (_, _) => _runtimeController.ConfigureWindowBounds();
    }

    private void Window_OnKeyDown(object sender, WpfKeyEventArgs e)
    {
        if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.E)
        {
            _runtimeController.ToggleMoveEdit();
            _canvasController.UpdateShellMode(_runtimeController.IsMoveEditEnabled);
            e.Handled = true;
            return;
        }

        if (!_runtimeController.IsMoveEditEnabled)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            _runtimeController.ApplyMoveEdit();
            _canvasController.UpdateShellMode(_runtimeController.IsMoveEditEnabled);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            _runtimeController.CancelMoveEdit();
            _canvasController.UpdateShellMode(_runtimeController.IsMoveEditEnabled);
            e.Handled = true;
        }
    }

    private void RootGrid_OnMouseLeftButtonDown(object sender, WpfMouseButtonEventArgs e)
    {
        if (!_runtimeController.IsMoveEditEnabled)
        {
            return;
        }

        Focus();
        if (_canvasController.HandleMouseLeftButtonDown(e.OriginalSource as DependencyObject))
        {
            e.Handled = true;
        }
    }

    private void RootGrid_OnMouseMove(object sender, WpfMouseEventArgs e)
    {
        if (!_runtimeController.IsMoveEditEnabled)
        {
            return;
        }

        _canvasController.HandleMouseMove(e);
    }

    private void RootGrid_OnMouseLeftButtonUp(object sender, WpfMouseButtonEventArgs e)
    {
        if (!_runtimeController.IsMoveEditEnabled)
        {
            return;
        }

        if (_canvasController.HandleMouseLeftButtonUp())
        {
            e.Handled = true;
        }
    }

    private void ApplyEditButton_OnClick(object sender, RoutedEventArgs e)
    {
        _runtimeController.ApplyMoveEdit();
        _canvasController.UpdateShellMode(_runtimeController.IsMoveEditEnabled);
    }

    private void CancelEditButton_OnClick(object sender, RoutedEventArgs e)
    {
        _runtimeController.CancelMoveEdit();
        _canvasController.UpdateShellMode(_runtimeController.IsMoveEditEnabled);
    }

    protected override void OnClosed(EventArgs e)
    {
        _runtimeController.Stop();
        base.OnClosed(e);
    }
}
