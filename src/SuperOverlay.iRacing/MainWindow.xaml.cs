using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfWindow = System.Windows.Window;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Windows.Threading;
using SuperOverlay.LayoutBuilder.Runtime;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Editor;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Telemetry.Mock;

namespace SuperOverlay.iRacing;

public partial class MainWindow : WpfWindow
{
    private readonly MockTelemetryProvider _telemetry = new();
    private readonly IRacingMapper _mapper = new();
    private readonly OverlayRuntimeBootstrapper _bootstrapper = new();

    private OverlayRuntimeSession? _session;
    private DispatcherTimer? _timer;
    private LayoutEditorInteractionController? _interactionController;
    private MainWindowInteractionController? _interactionWindowController;
    private LayoutSaveCoordinator? _saveCoordinator;
    private LayoutGuidePresenter? _guidePresenter;
    private EditorPropertiesPanelController? _propertiesController;
    private MainWindowCommandController? _commandController;
    private MainWindowCanvasController? _canvasController;

    public MainWindow()
    {
        InitializeComponent();

        _session = _bootstrapper.Build(RootGrid, OverlayShellMode.Editor);
        _interactionController = new LayoutEditorInteractionController(_session);
        _saveCoordinator = new LayoutSaveCoordinator(TimeSpan.FromMilliseconds(250), () => _session?.SaveLayout());
        _guidePresenter = new LayoutGuidePresenter(VerticalGuide, HorizontalGuide);
        var controllers = MainWindowControllerFactory.Create(
            this,
            CreatePropertiesPanelView(),
            CreateCanvasView(),
            () => _session,
            _interactionController,
            _saveCoordinator!,
            _guidePresenter!,
            () => RootGrid.ActualWidth,
            () => RootGrid.ActualHeight,
            RefreshCatalog,
            RefreshPropertiesPanel);

        _propertiesController = controllers.Properties;
        _commandController = controllers.Commands;
        _canvasController = controllers.Canvas;
        _interactionWindowController = controllers.Interaction;

        _session.SetSnappingEnabled(true);
        RefreshCatalog();
        RefreshPropertiesPanel();
        _guidePresenter.Hide();

        SnapToggleButton.IsChecked = true;
        SnapToggleButton.Content = "Snap: On";

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _timer.Tick += OnTick;
        _timer.Start();
        Loaded += (_, _) => Focus();
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

    private void EditorBar_MouseLeftButtonDown(object sender, WpfMouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void SnapToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        _interactionWindowController?.SetSnappingEnabled(true);
        SnapToggleButton.Content = "Snap: On";
    }

    private void SnapToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        _interactionWindowController?.SetSnappingEnabled(false);
        SnapToggleButton.Content = "Snap: Off";
    }

    private void SaveLayout_OnClick(object sender, RoutedEventArgs e) => _commandController?.SaveNow();

    private void ReloadLayout_OnClick(object sender, RoutedEventArgs e) => _commandController?.ReloadLayout();

    private void AddSelectedCatalogItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_commandController?.AddSelectedCatalogItem(CatalogComboBox.SelectedItem as SuperOverlay.Dashboards.Registry.DashboardCatalogItem) == true)
        {
            Focus();
        }
    }

    private void GroupSelected_OnClick(object sender, RoutedEventArgs e) => _commandController?.GroupSelected();
    private void UngroupSelected_OnClick(object sender, RoutedEventArgs e) => _commandController?.UngroupSelected();
    private void DuplicateSelected_OnClick(object sender, RoutedEventArgs e) => _commandController?.DuplicateSelected();
    private void DeleteSelected_OnClick(object sender, RoutedEventArgs e) => _commandController?.DeleteSelected();
    private void CopySelected_OnClick(object sender, RoutedEventArgs e) => _commandController?.CopySelected();
    private void PasteClipboard_OnClick(object sender, RoutedEventArgs e) => _commandController?.PasteClipboard();
    private void LockSelected_OnClick(object sender, RoutedEventArgs e) => _commandController?.LockSelected();
    private void UnlockSelected_OnClick(object sender, RoutedEventArgs e) => _commandController?.UnlockSelected();
    private void BringForward_OnClick(object sender, RoutedEventArgs e) => _commandController?.BringForward();
    private void SendBackward_OnClick(object sender, RoutedEventArgs e) => _commandController?.SendBackward();
    private void BringToFront_OnClick(object sender, RoutedEventArgs e) => _commandController?.BringToFront();
    private void SendToBack_OnClick(object sender, RoutedEventArgs e) => _commandController?.SendToBack();

    private void RootGrid_OnPreviewMouseRightButtonDown(object sender, WpfMouseButtonEventArgs e)
    {
        Focus();

        if (_canvasController?.HandlePreviewRightMouseDown(e.OriginalSource as DependencyObject) == true)
        {
            CanvasContextMenu.IsOpen = false;
            e.Handled = true;
        }
    }

    private void RootGrid_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (_canvasController?.PrepareContextMenu() == false)
        {
            e.Handled = true;
        }
    }

    private void RootGrid_OnMouseLeftButtonDown(object sender, WpfMouseButtonEventArgs e)
    {
        Focus();

        if (_interactionWindowController?.BeginInteraction(e.OriginalSource as DependencyObject, e.GetPosition(RootGrid), Keyboard.Modifiers) == true)
        {
            RootGrid.CaptureMouse();
            e.Handled = true;
        }
    }

    private void RootGrid_OnMouseMove(object sender, WpfMouseEventArgs e)
        => _interactionWindowController?.MoveInteraction(e.GetPosition(RootGrid), Keyboard.Modifiers, e.LeftButton);

    private void RootGrid_OnMouseLeftButtonUp(object sender, WpfMouseButtonEventArgs e)
    {
        if (_interactionWindowController?.EndInteraction(Keyboard.Modifiers) == true)
        {
            RootGrid.ReleaseMouseCapture();
        }
    }

    private void Window_OnKeyDown(object sender, WpfKeyEventArgs e)
    {
        if (_commandController?.HandleKeyDown(e) == true)
        {
            e.Handled = true;
        }
    }

    private void ApplyProperties_OnClick(object sender, RoutedEventArgs e)
    {
        if (_propertiesController?.Apply() == true)
        {
            _commandController?.QueueSave();
        }
    }

    private void RefreshCatalog()
    {
        if (_session is null)
        {
            return;
        }

        var catalog = _session.GetCatalog();
        var currentTypeId = (CatalogComboBox.SelectedItem as SuperOverlay.Dashboards.Registry.DashboardCatalogItem)?.TypeId;
        var selected = catalog.FirstOrDefault(x => currentTypeId is not null && x.TypeId == currentTypeId) ?? catalog.FirstOrDefault();
        CatalogComboBox.ItemsSource = catalog;
        CatalogComboBox.SelectedItem = selected;
    }

    private void RefreshPropertiesPanel() => _propertiesController?.Refresh();

    private EditorPropertiesPanelView CreatePropertiesPanelView()
    {
        return new EditorPropertiesPanelView(
            PropertiesPanel,
            SelectedWidgetNameTextBlock,
            SelectedWidgetMetaTextBlock,
            PropertiesHintTextBlock,
            XTextBox,
            YTextBox,
            WidthTextBox,
            HeightTextBox,
            ZIndexTextBox,
            LockedCheckBox,
            CommonCornerSettingsPanel,
            CommonCornerTopLeftTextBox,
            CommonCornerTopRightTextBox,
            CommonCornerBottomRightTextBox,
            CommonCornerBottomLeftTextBox,
            ShiftLedSettingsPanel,
            ShiftLedCountTextBox,
            ShiftUsePerLedColorsCheckBox,
            ShiftShowBackgroundCheckBox,
            ShiftBackgroundColorTextBox,
            ShiftOffColorTextBox,
            ShiftOnColorTextBox,
            ShiftOnColorsTextBox,
            DecorativePanelSettingsPanel,
            DecorativeBackgroundColorTextBox,
            DecorativeOpacityTextBox);
    }

    private EditorCanvasView CreateCanvasView()
    {
        return new EditorCanvasView(
            RootGrid,
            CanvasContextMenu,
            CopyMenuItem,
            PasteMenuItem,
            DuplicateMenuItem,
            DeleteMenuItem,
            GroupSelectedMenuItem,
            UngroupSelectedMenuItem,
            LockSelectedMenuItem,
            UnlockSelectedMenuItem,
            BringForwardMenuItem,
            SendBackwardMenuItem,
            BringToFrontMenuItem,
            SendToBackMenuItem,
            SelectionMarquee);
    }
    private void PickShiftBackgroundColor_OnClick(object sender, RoutedEventArgs e) => _propertiesController?.PickShiftBackgroundColor();
    private void PickShiftOffColor_OnClick(object sender, RoutedEventArgs e) => _propertiesController?.PickShiftOffColor();
    private void PickShiftOnColor_OnClick(object sender, RoutedEventArgs e) => _propertiesController?.PickShiftOnColor();
    private void PickDecorativeBackgroundColor_OnClick(object sender, RoutedEventArgs e) => _propertiesController?.PickDecorativeBackgroundColor();


    protected override void OnClosed(EventArgs e)
    {
        _saveCoordinator?.SaveNow();
        _timer?.Stop();
        base.OnClosed(e);
    }
}
