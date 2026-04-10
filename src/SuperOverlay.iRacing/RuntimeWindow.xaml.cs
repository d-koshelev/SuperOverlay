using System.Windows;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Windows.Input;
using System.Windows.Threading;
using SuperOverlay.LayoutBuilder.Runtime;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Telemetry.Mock;

namespace SuperOverlay.iRacing;

public partial class RuntimeWindow : Window
{
    private readonly MockTelemetryProvider _telemetry = new();
    private readonly IRacingMapper _mapper = new();
    private readonly OverlayRuntimeBootstrapper _bootstrapper = new();

    private OverlayRuntimeSession? _session;
    private LayoutEditorInteractionController? _interactionController;
    private DispatcherTimer? _timer;
    private bool _isMoveEditEnabled;

    public RuntimeWindow()
    {
        InitializeComponent();

        ConfigureWindowBounds();
        Loaded += (_, _) => ConfigureWindowBounds();

        BuildSession(OverlayShellMode.Runtime);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void ConfigureWindowBounds()
    {
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    private void BuildSession(OverlayShellMode shellMode)
    {
        RootGrid.Children.Clear();
        _session = _bootstrapper.Build(RootGrid, shellMode);
        _session.SetSnappingEnabled(false);
        _interactionController = shellMode == OverlayShellMode.RuntimeMoveEdit
            ? new LayoutEditorInteractionController(_session, LayoutInteractionOptions.RuntimeMoveOnly)
            : null;

        RuntimeHintBorder.Visibility = shellMode == OverlayShellMode.Runtime ? Visibility.Visible : Visibility.Collapsed;
        EditOverlayBar.Visibility = shellMode == OverlayShellMode.RuntimeMoveEdit ? Visibility.Visible : Visibility.Collapsed;
        _isMoveEditEnabled = shellMode == OverlayShellMode.RuntimeMoveEdit;
    }

    private void ToggleMoveEdit()
    {
        Focus();

        if (_isMoveEditEnabled)
        {
            CancelMoveEdit();
            return;
        }

        BuildSession(OverlayShellMode.RuntimeMoveEdit);
    }

    private void ApplyMoveEdit()
    {
        if (_session is null || !_isMoveEditEnabled)
        {
            return;
        }

        _session.SaveLayout();
        BuildSession(OverlayShellMode.Runtime);
    }

    private void CancelMoveEdit()
    {
        if (_session is null || !_isMoveEditEnabled)
        {
            return;
        }

        _session.ReloadLayout();
        BuildSession(OverlayShellMode.Runtime);
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        var (speed, rpm, gear, shiftLightPercent) = _telemetry.Get();
        _session.Update(_mapper.Map(speed, rpm, gear, shiftLightPercent));
    }

    private void Window_OnKeyDown(object sender, WpfKeyEventArgs e)
    {
        if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.E)
        {
            ToggleMoveEdit();
            e.Handled = true;
            return;
        }

        if (!_isMoveEditEnabled)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            ApplyMoveEdit();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            CancelMoveEdit();
            e.Handled = true;
        }
    }

    private void RootGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isMoveEditEnabled || _session is null || _interactionController is null)
        {
            return;
        }

        Focus();
        var startResult = _interactionController.BeginInteraction(e.OriginalSource as DependencyObject, e.GetPosition(RootGrid), Keyboard.Modifiers);
        if (startResult.CaptureMouse)
        {
            RootGrid.CaptureMouse();
            e.Handled = true;
        }
    }

    private void RootGrid_OnMouseMove(object sender, WpfMouseEventArgs e)
    {
        if (!_isMoveEditEnabled || _interactionController is null)
        {
            return;
        }

        if (!_interactionController.IsInteracting || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        _interactionController.MoveInteraction(e.GetPosition(RootGrid), RootGrid.ActualWidth, RootGrid.ActualHeight, true);
    }

    private void RootGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isMoveEditEnabled || _interactionController is null)
        {
            return;
        }

        var endResult = _interactionController.EndInteraction(Keyboard.Modifiers);
        if (!endResult.Ended)
        {
            return;
        }

        RootGrid.ReleaseMouseCapture();
        e.Handled = true;
    }

    private void ApplyEditButton_OnClick(object sender, RoutedEventArgs e) => ApplyMoveEdit();
    private void CancelEditButton_OnClick(object sender, RoutedEventArgs e) => CancelMoveEdit();

    protected override void OnClosed(EventArgs e)
    {
        _timer?.Stop();
        base.OnClosed(e);
    }
}
