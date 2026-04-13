using WpfWindow = System.Windows.Window;
using System.Windows.Controls;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Telemetry.IRacing;

namespace SuperOverlay.iRacing.Runtime;

internal static class RuntimeWindowControllerFactory
{
    public static RuntimeWindowControllers Create(
        WpfWindow owner,
        Grid rootGrid,
        IRacingTelemetryProvider telemetry,
        IRacingMapper mapper,
        OverlayRuntimeBootstrapper bootstrapper,
        Action focusWindow)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(rootGrid);
        ArgumentNullException.ThrowIfNull(telemetry);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(bootstrapper);
        ArgumentNullException.ThrowIfNull(focusWindow);

        var windowController = new RuntimeWindowController(
            owner,
            rootGrid,
            telemetry,
            mapper,
            bootstrapper);

        var canvasController = new RuntimeCanvasController(() => windowController.Session, rootGrid);
        var interactionController = new RuntimeWindowInteractionController(windowController, canvasController, focusWindow);

        return new RuntimeWindowControllers(windowController, canvasController, interactionController);
    }
}
