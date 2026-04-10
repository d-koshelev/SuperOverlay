using System.Windows;
using WpfWindow = System.Windows.Window;
using System.Windows.Controls;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Telemetry.Mock;

namespace SuperOverlay.iRacing.Runtime;

public static class RuntimeWindowControllerFactory
{
    public static RuntimeWindowControllers Create(
        WpfWindow owner,
        Grid rootGrid,
        Border runtimeHintBorder,
        Border editOverlayBar,
        MockTelemetryProvider telemetry,
        IRacingMapper mapper,
        OverlayRuntimeBootstrapper bootstrapper,
        Action focusWindow)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(rootGrid);
        ArgumentNullException.ThrowIfNull(runtimeHintBorder);
        ArgumentNullException.ThrowIfNull(editOverlayBar);
        ArgumentNullException.ThrowIfNull(telemetry);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(bootstrapper);
        ArgumentNullException.ThrowIfNull(focusWindow);

        var windowController = new RuntimeWindowController(
            owner,
            rootGrid,
            runtimeHintBorder,
            editOverlayBar,
            telemetry,
            mapper,
            bootstrapper);

        var canvasController = new RuntimeCanvasController(() => windowController.Session, rootGrid);
        var interactionController = new RuntimeWindowInteractionController(windowController, canvasController, focusWindow);

        return new RuntimeWindowControllers(windowController, canvasController, interactionController);
    }
}
