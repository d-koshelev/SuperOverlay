using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorCompositionRoot
{
    public static LayoutEditorWindowRuntime Build(
        LayoutEditorWindow window,
        LayoutEditorState state,
        ObservableCollection<LayoutEditorWidget> widgets,
        ObservableCollection<LayoutEditorWidget> previewWidgets,
        ILayoutEditorInteractionEngine? engine,
        Func<IReadOnlyList<LayoutEditorWidget>> selectedWidgetsAccessor,
        Action positionPropertiesPanel,
        Action refreshSelectionDetails,
        Action hideGuides,
        Action<Point> updatePresetPreview,
        Action<Point> updateWidgetPreview,
        Action confirmPresetPlacement,
        Action confirmWidgetPlacement,
        Action cancelPlacement,
        Action<double, double> moveFloatingMenu,
        Action<double, double> movePropertiesPanel,
        Action<LayoutEditorWidget, MouseButtonEventArgs> handleWidgetResizeClick,
        Action<Point> updateResizedWidgets,
        Action toggleLockSelectionFromShortcut,
        Func<DependencyObject?, LayoutEditorWidget?> resolveWidgetFromSource,
        Func<LayoutEditorWindow, LayoutEditorPropertiesPanelView> propertiesPanelViewFactory)
    {
        var workspace = new LayoutEditorWorkspaceService(new LayoutEditorPresetStore(), new LayoutEditorLayoutStore());
        var dialogs = new LayoutEditorDialogService(window);
        var commands = new LayoutEditorCommandService(workspace, dialogs, engine);
        var slotEditing = new LayoutEditorSlotEditingService(window);
        var propertiesPresenter = new LayoutEditorPropertiesPanelPresenter(propertiesPanelViewFactory(window));
        var selection = new LayoutEditorSelectionService(
            widgets,
            state,
            propertiesPresenter,
            window.PropertiesPanel,
            selectedWidgetsAccessor,
            positionPropertiesPanel);
        var marquee = new LayoutEditorMarqueePresenter(state, window.SelectionRectangle, widgets, selection.SelectWidgets, engine);
        var placement = new LayoutEditorPlacementPresenter(
            state,
            widgets,
            previewWidgets,
            window.PlacementHintPanel,
            window.PlacementHintText,
            () => new Size(window.RootGrid.ActualWidth, window.RootGrid.ActualHeight),
            () => Mouse.GetPosition(window.OverlayChromeLayer),
            selection.SelectWidgets,
            positionPropertiesPanel);
        var interaction = new LayoutEditorInteractionCoordinator(
            state,
            window.RootGrid,
            window.OverlayChromeLayer,
            window.FloatingMenu,
            window.PropertiesPanel,
            selection,
            marquee,
            resolveWidgetFromSource,
            updatePresetPreview,
            updateWidgetPreview,
            confirmPresetPlacement,
            confirmWidgetPlacement,
            cancelPlacement,
            moveFloatingMenu,
            movePropertiesPanel,
            hideGuides,
            refreshSelectionDetails,
            positionPropertiesPanel,
            handleWidgetResizeClick,
            updateResizedWidgets);
        var shortcuts = new LayoutEditorShortcutService(
            state,
            () => window.DeleteMenuItem_OnClick(window, new RoutedEventArgs()),
            () => window.DuplicateMenuItem_OnClick(window, new RoutedEventArgs()),
            () => window.GroupMenuItem_OnClick(window, new RoutedEventArgs()),
            () => window.UngroupMenuItem_OnClick(window, new RoutedEventArgs()),
            toggleLockSelectionFromShortcut,
            () => window.BringToFrontMenuItem_OnClick(window, new RoutedEventArgs()),
            () => window.SendToBackMenuItem_OnClick(window, new RoutedEventArgs()),
            () => window.BringForwardMenuItem_OnClick(window, new RoutedEventArgs()),
            () => window.SendBackwardMenuItem_OnClick(window, new RoutedEventArgs()),
            isRaceMode => window.SetRaceMode(isRaceMode),
            () => window.CancelPlacement());

        return new LayoutEditorWindowRuntime
        {
            Workspace = workspace,
            Dialogs = dialogs,
            Commands = commands,
            SlotEditing = slotEditing,
            PropertiesPresenter = propertiesPresenter,
            Selection = selection,
            Marquee = marquee,
            Placement = placement,
            Interaction = interaction,
            Shortcuts = shortcuts
        };
    }
}
