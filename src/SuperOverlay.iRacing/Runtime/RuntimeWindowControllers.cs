namespace SuperOverlay.iRacing.Runtime;

public sealed record RuntimeWindowControllers(
    RuntimeWindowController Window,
    RuntimeCanvasController Canvas,
    RuntimeWindowInteractionController Interaction);
