using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfMessageBox = System.Windows.MessageBox;
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
using SuperOverlay.iRacing.Telemetry.IRacing;

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
        RefreshEditorCanvasBounds();
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

    private void RefreshEditorCanvasBounds()
    {
        if (_session is null)
        {
            return;
        }

        var canvas = _session.GetCanvas();

        RootGrid.Width = canvas.Width;
        RootGrid.Height = canvas.Height;

        CanvasFrameBorder.Width = canvas.Width;
        CanvasFrameBorder.Height = canvas.Height;

        CanvasWidthTextBox.Text = ((int)Math.Round(canvas.Width)).ToString();
        CanvasHeightTextBox.Text = ((int)Math.Round(canvas.Height)).ToString();

        SyncCanvasPresetSelection(canvas.Width, canvas.Height);
    }

    private static readonly (string Key, double Width, double Height)[] CanvasPresets =
    {
        ("1920x1080", 1920, 1080),
        ("2560x1440", 2560, 1440),
        ("3840x2160", 3840, 2160),
        ("2560x1080", 2560, 1080),
        ("3440x1440", 3440, 1440),
        ("5120x2160", 5120, 2160),
        ("3840x1080", 3840, 1080),
        ("5120x1440", 5120, 1440),
        ("7680x2160", 7680, 2160),
        ("5760x1080", 5760, 1080),
        ("7680x1440", 7680, 1440),
        ("10320x1440", 10320, 1440)
    };

    private bool TryApplyCanvasSize(double width, double height)
    {
        if (_session is null)
        {
            return false;
        }

        if (_session.UpdateCanvasSize(width, height))
        {
            RefreshEditorCanvasBounds();
            RefreshPropertiesPanel();
            Focus();
            return true;
        }

        return false;
    }

    private bool TryGetSelectedCanvasPreset(out double width, out double height)
    {
        width = 0;
        height = 0;

        if (CanvasPresetComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not string key)
        {
            WpfMessageBox.Show(this, "Select a canvas preset first.", "Canvas Preset", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        var preset = CanvasPresets.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(preset.Key))
        {
            WpfMessageBox.Show(this, "The selected canvas preset is invalid.", "Canvas Preset", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        width = preset.Width;
        height = preset.Height;
        return true;
    }

    private void SyncCanvasPresetSelection(double width, double height)
    {
        var preset = CanvasPresets.FirstOrDefault(x => Math.Abs(x.Width - width) < 0.5 && Math.Abs(x.Height - height) < 0.5);
        if (string.IsNullOrWhiteSpace(preset.Key))
        {
            CanvasPresetComboBox.SelectedIndex = -1;
            return;
        }

        foreach (var item in CanvasPresetComboBox.Items.OfType<ComboBoxItem>())
        {
            if (item.Tag is string key && string.Equals(key, preset.Key, StringComparison.OrdinalIgnoreCase))
            {
                CanvasPresetComboBox.SelectedItem = item;
                return;
            }
        }

        CanvasPresetComboBox.SelectedIndex = -1;
    }

    private bool TryGetCanvasSizeInput(out double width, out double height)
    {
        width = 0;
        height = 0;

        if (!double.TryParse(CanvasWidthTextBox.Text, out width) || width < 320)
        {
            WpfMessageBox.Show(this, "Canvas width must be a number greater than or equal to 320.", "Apply Canvas", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!double.TryParse(CanvasHeightTextBox.Text, out height) || height < 180)
        {
            WpfMessageBox.Show(this, "Canvas height must be a number greater than or equal to 180.", "Apply Canvas", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        width = Math.Round(width);
        height = Math.Round(height);
        return true;
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

    private void ApplyCanvasSize_OnClick(object sender, RoutedEventArgs e)
    {
        if (_session is null || !TryGetCanvasSizeInput(out var width, out var height))
        {
            return;
        }

        TryApplyCanvasSize(width, height);
    }

    private void ApplyCanvasPreset_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedCanvasPreset(out var width, out var height))
        {
            return;
        }

        CanvasWidthTextBox.Text = ((int)Math.Round(width)).ToString();
        CanvasHeightTextBox.Text = ((int)Math.Round(height)).ToString();
        TryApplyCanvasSize(width, height);
    }

    private void CanvasPresetComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CanvasPresetComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not string key)
        {
            return;
        }

        var preset = CanvasPresets.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(preset.Key))
        {
            CanvasWidthTextBox.Text = ((int)Math.Round(preset.Width)).ToString();
            CanvasHeightTextBox.Text = ((int)Math.Round(preset.Height)).ToString();
        }
    }

    private void SaveLayout_OnClick(object sender, RoutedEventArgs e) => _commandController?.SaveNow();

    private void ReloadLayout_OnClick(object sender, RoutedEventArgs e)
    {
        _commandController?.ReloadLayout();
        RefreshEditorCanvasBounds();
    }

    private void NewPanelLayout_OnClick(object sender, RoutedEventArgs e)
    {
        if (_commandController?.NewPanelLayout(this) == true)
        {
            Focus();
            RefreshEditorCanvasBounds();
            RefreshCatalog();
            RefreshPropertiesPanel();
        }
    }

    private void OpenPanelLayout_OnClick(object sender, RoutedEventArgs e)
    {
        if (_commandController?.OpenPanelLayout(this) == true)
        {
            Focus();
            RefreshEditorCanvasBounds();
            RefreshCatalog();
            RefreshPropertiesPanel();
        }
    }

    private void SavePanelLayout_OnClick(object sender, RoutedEventArgs e) => _commandController?.SavePanelLayout(this);

    private void SavePanelPreset_OnClick(object sender, RoutedEventArgs e) => _commandController?.SaveSelectionAsPanelPreset(this);

    private void OpenPanelPreset_OnClick(object sender, RoutedEventArgs e)
    {
        if (_commandController?.OpenPanelPresetForEditing(this) == true)
        {
            Focus();
            RefreshEditorCanvasBounds();
            RefreshCatalog();
            RefreshPropertiesPanel();
        }
    }

    private void InsertPanelPreset_OnClick(object sender, RoutedEventArgs e)
    {
        if (_commandController?.InsertPanelPreset(this) == true)
        {
            Focus();
            RefreshEditorCanvasBounds();
        }
    }

    private async void DumpIRacingInventory_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var diagnosticsDirectory = Path.Combine(AppContext.BaseDirectory, "Diagnostics");
            var service = new IRacingInventoryDumpService();
            var result = await service.DumpAsync(diagnosticsDirectory);

            WpfMessageBox.Show(
                this,
                $"Dumped {result.VariableCount} telemetry variables.\n\nInventory: {result.InventoryJsonPath}\nSession YAML: {result.SessionYamlPath}",
                "Dump iRacing",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(
                this,
                ex.Message,
                "Dump iRacing",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddSelectedCatalogItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_commandController?.AddSelectedCatalogItem(CatalogComboBox.SelectedItem as SuperOverlay.Dashboards.Registry.DashboardCatalogItem) == true)
        {
            Focus();
            RefreshEditorCanvasBounds();
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