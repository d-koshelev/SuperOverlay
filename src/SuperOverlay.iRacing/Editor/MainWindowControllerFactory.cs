using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor;

public static class MainWindowControllerFactory
{
    public static MainWindowControllers Create(
        System.Windows.Window owner,
        EditorPropertiesPanelView propertiesView,
        EditorCanvasView canvasView,
        Func<OverlayRuntimeSession?> getSession,
        LayoutEditorInteractionController interactionController,
        LayoutSaveCoordinator saveCoordinator,
        LayoutGuidePresenter guidePresenter,
        Func<double> getCanvasWidth,
        Func<double> getCanvasHeight,
        Action refreshCatalog,
        Action refreshProperties)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(propertiesView);
        ArgumentNullException.ThrowIfNull(canvasView);
        ArgumentNullException.ThrowIfNull(getSession);
        ArgumentNullException.ThrowIfNull(interactionController);
        ArgumentNullException.ThrowIfNull(saveCoordinator);
        ArgumentNullException.ThrowIfNull(guidePresenter);
        ArgumentNullException.ThrowIfNull(getCanvasWidth);
        ArgumentNullException.ThrowIfNull(getCanvasHeight);
        ArgumentNullException.ThrowIfNull(refreshCatalog);
        ArgumentNullException.ThrowIfNull(refreshProperties);

        var canvasController = new MainWindowCanvasController(
            canvasView,
            getSession,
            refreshProperties);

        var propertiesController = new EditorPropertiesPanelController(
            owner,
            propertiesView,
            getSession,
            WidgetSettings.EditorWidgetSettingsBinderRegistry.CreateDefault());

        var commandController = new MainWindowCommandController(
            getSession,
            saveCoordinator,
            guidePresenter,
            getCanvasWidth,
            getCanvasHeight,
            refreshCatalog,
            refreshProperties,
            canvasController.HideSelectionMarquee);

        var interactionWindowController = new MainWindowInteractionController(
            getSession,
            interactionController,
            canvasController,
            guidePresenter,
            refreshProperties,
            commandController.QueueSave,
            getCanvasWidth,
            getCanvasHeight);

        return new MainWindowControllers(
            propertiesController,
            commandController,
            canvasController,
            interactionWindowController);
    }
}
