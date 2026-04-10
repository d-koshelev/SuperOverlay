namespace SuperOverlay.iRacing.Editor;

public sealed record MainWindowControllers(
    EditorPropertiesPanelController Properties,
    MainWindowCommandController Commands,
    MainWindowCanvasController Canvas,
    MainWindowInteractionController Interaction);
