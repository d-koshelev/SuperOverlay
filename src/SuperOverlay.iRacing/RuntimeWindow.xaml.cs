using WpfWindow = System.Windows.Window;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Windows;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Runtime;
using SuperOverlay.iRacing.Telemetry.IRacing;

namespace SuperOverlay.iRacing;

public partial class RuntimeWindow : WpfWindow
{
    private readonly IRacingTelemetryProvider _telemetry = new();
    private readonly IRacingMapper _mapper = new();
    private readonly Hosting.OverlayRuntimeBootstrapper _bootstrapper = new();

    private readonly RuntimeWindowController _runtimeController;
    private readonly RuntimeWindowInteractionController _interactionController;

    public RuntimeWindow()
    {
        InitializeComponent();

        var controllers = RuntimeWindowControllerFactory.Create(
            this,
            RootGrid,
            _telemetry,
            _mapper,
            _bootstrapper,
            () => Focus());

        _runtimeController = controllers.Window;
        _interactionController = controllers.Interaction;

        _runtimeController.Initialize();
        _interactionController.Initialize();
        Loaded += (_, _) => _runtimeController.ConfigureWindowBounds();
    }

    private void Window_OnKeyDown(object sender, WpfKeyEventArgs e) => _interactionController.HandleKeyDown(e);

    private void RootGrid_OnMouseLeftButtonDown(object sender, WpfMouseButtonEventArgs e) => _interactionController.HandleMouseLeftButtonDown(e);

    private void RootGrid_OnMouseMove(object sender, WpfMouseEventArgs e) => _interactionController.HandleMouseMove(e);

    private void RootGrid_OnMouseLeftButtonUp(object sender, WpfMouseButtonEventArgs e) => _interactionController.HandleMouseLeftButtonUp(e);

    protected override void OnClosed(EventArgs e)
    {
        _runtimeController.Stop();
        base.OnClosed(e);
    }
}
