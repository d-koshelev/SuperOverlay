using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow : Window
{
    private readonly LayoutEditorWorkspaceService _workspace;
    private readonly LayoutEditorDialogService _dialogs;
    private readonly LayoutEditorCommandService _commands;
    private readonly LayoutEditorSlotEditingService _slotEditing;
    private readonly LayoutEditorPropertiesPanelPresenter _propertiesPresenter;
    private readonly LayoutEditorSelectionService _selection;
    private readonly LayoutEditorMarqueePresenter _marquee;
    private readonly LayoutEditorPlacementPresenter _placement;
    private readonly LayoutEditorInteractionCoordinator _interaction;
    private readonly LayoutEditorShortcutService _shortcuts;
    private readonly LayoutEditorState _state = new();
    private readonly ILayoutEditorInteractionEngine? _engine;

    public LayoutEditorWindow(ILayoutEditorInteractionEngine? engine = null)
    {
        _engine = engine;

        InitializeComponent();

        var runtime = LayoutEditorCompositionRoot.Build(
            this,
            _state,
            Widgets,
            PreviewWidgets,
            _engine,
            () => SelectedWidgets,
            PositionPropertiesPanel,
            RefreshSelectionDetails,
            HideGuides,
            UpdatePresetPreview,
            UpdateWidgetPreview,
            ConfirmPresetPlacement,
            ConfirmWidgetPlacement,
            () => CancelPlacement(),
            MoveFloatingMenu,
            MovePropertiesPanel,
            HandleWidgetResizeClick,
            UpdateResizedWidgets,
            ToggleLockSelectionFromShortcut,
            ResolveWidgetFromSource,
            LayoutEditorPropertiesPanelViewFactory.Create);

        _workspace = runtime.Workspace;
        _dialogs = runtime.Dialogs;
        _commands = runtime.Commands;
        _slotEditing = runtime.SlotEditing;
        _propertiesPresenter = runtime.PropertiesPresenter;
        _selection = runtime.Selection;
        _marquee = runtime.Marquee;
        _placement = runtime.Placement;
        _interaction = runtime.Interaction;
        _shortcuts = runtime.Shortcuts;

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    public ObservableCollection<LayoutEditorWidget> Widgets { get; } = [];
    public ObservableCollection<LayoutEditorWidget> PreviewWidgets { get; } = [];

    private IReadOnlyList<LayoutEditorWidget> SelectedWidgets => Widgets.Where(x => x.IsSelected).ToList();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ClampFloatingMenuToViewport();
        PositionPropertiesPanel();
        RefreshChromeToggleText();
        HideGuides();
        RefreshSelectionDetails();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ClampFloatingMenuToViewport();
        PositionPropertiesPanel();

        if (_state.IsPlacingPreset)
        {
            UpdatePresetPreview(Mouse.GetPosition(OverlayChromeLayer));
        }
        else if (_state.IsPlacingWidget)
        {
            UpdateWidgetPreview(Mouse.GetPosition(OverlayChromeLayer));
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_shortcuts.TryHandle(e))
        {
            e.Handled = true;
        }
    }

    private void ToggleLockSelectionFromShortcut()
    {
        SyncEngineFromWidgets();
        if (_commands.ToggleLockSelection(SelectedWidgets))
        {
            if (_engine is not null)
            {
                ApplyEngineSnapshot(_engine.GetSnapshot());
            }

            RefreshSelectionDetails();
        }
    }
}
