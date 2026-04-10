using System.Globalization;
using System.Windows.Controls;
using WpfTextBox = System.Windows.Controls.TextBox;
using System.Windows;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Windows.Input;
using MediaColor = System.Windows.Media.Color;
using MediaColorConverter = System.Windows.Media.ColorConverter;
using Forms = System.Windows.Forms;
using System.Windows.Threading;
using SuperOverlay.LayoutBuilder.Runtime;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.Dashboards.Items.ShiftLeds;
using SuperOverlay.iRacing.Telemetry.Mock;

namespace SuperOverlay.iRacing;

public partial class MainWindow : Window
{
    private const double MoveStep = 10;

    private readonly MockTelemetryProvider _telemetry = new();
    private readonly IRacingMapper _mapper = new();
    private readonly OverlayRuntimeBootstrapper _bootstrapper = new();

    private OverlayRuntimeSession? _session;
    private DispatcherTimer? _timer;
    private LayoutEditorInteractionController? _interactionController;
    private LayoutSaveCoordinator? _saveCoordinator;
    private LayoutGuidePresenter? _guidePresenter;

    public MainWindow()
    {
        InitializeComponent();

        _session = _bootstrapper.Build(RootGrid, OverlayShellMode.Editor);
        _interactionController = new LayoutEditorInteractionController(_session);
        _saveCoordinator = new LayoutSaveCoordinator(TimeSpan.FromMilliseconds(250), () => _session?.SaveLayout());
        _guidePresenter = new LayoutGuidePresenter(VerticalGuide, HorizontalGuide);

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

    private void EditorBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        _session.SetSnappingEnabled(true);
        SnapToggleButton.Content = "Snap: On";
    }

    private void SnapToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        _session.SetSnappingEnabled(false);
        SnapToggleButton.Content = "Snap: Off";
        _guidePresenter?.Hide();
    }

    private void SaveLayout_OnClick(object sender, RoutedEventArgs e) => _saveCoordinator?.SaveNow();

    private void ReloadLayout_OnClick(object sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        _saveCoordinator?.CancelPendingSave();
        _session.ReloadLayout();
        RefreshCatalog();
        RefreshPropertiesPanel();
        _guidePresenter?.Hide();
        HideSelectionMarquee();
    }

    private void AddSelectedCatalogItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_session is null || CatalogComboBox.SelectedItem is not SuperOverlay.Dashboards.Registry.DashboardCatalogItem item)
        {
            return;
        }

        if (_session.AddItem(item.TypeId))
        {
            _session.SelectItem(_session.GetLayoutItems().LastOrDefault()?.Id);
            RefreshPropertiesPanel();
            QueueSaveLayout();
            Focus();
        }
    }

    private void GroupSelected_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.GroupSelectedItems() == true);
    private void UngroupSelected_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.UngroupSelected() == true);
    private void DuplicateSelected_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.DuplicateSelected() == true);
    private void DeleteSelected_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.DeleteSelected() == true);
    private void CopySelected_OnClick(object sender, RoutedEventArgs e) => _session?.CopySelected();
    private void PasteClipboard_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.PasteClipboard() == true);
    private void LockSelected_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.SetLockSelected(true) == true);
    private void UnlockSelected_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.SetLockSelected(false) == true);
    private void BringForward_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.BringForwardSelected() == true);
    private void SendBackward_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.SendBackwardSelected() == true);
    private void BringToFront_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.BringToFrontSelected() == true);
    private void SendToBack_OnClick(object sender, RoutedEventArgs e) => ExecuteAndSave(() => _session?.SendToBackSelected() == true);

    private void RootGrid_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        Focus();
        HideSelectionMarquee();

        var hitItemId = _session.HitTestItemId(e.OriginalSource as DependencyObject);
        if (hitItemId is null)
        {
            _session.SelectItem(null);
            RefreshPropertiesPanel();
            if (!_session.CanPaste)
            {
                CanvasContextMenu.IsOpen = false;
                e.Handled = true;
            }
            return;
        }

        if (!_session.GetSelectedItemIds().Contains(hitItemId.Value))
        {
            _session.SelectItem(hitItemId.Value);
            RefreshPropertiesPanel();
        }
    }

    private void RootGrid_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        var selectedIds = _session.GetSelectedItemIds();
        var hasSelection = selectedIds.Count > 0;
        CopyMenuItem.IsEnabled = hasSelection;
        PasteMenuItem.IsEnabled = _session.CanPaste;
        DuplicateMenuItem.IsEnabled = hasSelection;
        DeleteMenuItem.IsEnabled = hasSelection;
        GroupSelectedMenuItem.IsEnabled = selectedIds.Count > 1;
        UngroupSelectedMenuItem.IsEnabled = hasSelection;
        LockSelectedMenuItem.IsEnabled = hasSelection && !_session.HasLockedSelection;
        UnlockSelectedMenuItem.IsEnabled = hasSelection && _session.HasLockedSelection;
        BringForwardMenuItem.IsEnabled = hasSelection;
        SendBackwardMenuItem.IsEnabled = hasSelection;
        BringToFrontMenuItem.IsEnabled = hasSelection;
        SendToBackMenuItem.IsEnabled = hasSelection;

        if (!hasSelection && !_session.CanPaste)
        {
            e.Handled = true;
        }
    }

    private void RootGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_session is null || _interactionController is null)
        {
            return;
        }

        Focus();

        var startResult = _interactionController.BeginInteraction(e.OriginalSource as DependencyObject, e.GetPosition(RootGrid), Keyboard.Modifiers);
        if (startResult.ShowMarquee && startResult.MarqueeRect is Rect startRect)
        {
            ShowSelectionMarquee(startRect);
            _guidePresenter?.Hide();
        }

        RefreshPropertiesPanel();

        if (startResult.ClearedSelection)
        {
            _guidePresenter?.Hide();
        }

        if (startResult.CaptureMouse)
        {
            RootGrid.CaptureMouse();
            e.Handled = true;
        }
    }

    private void RootGrid_OnMouseMove(object sender, WpfMouseEventArgs e)
    {
        if (_interactionController is null)
        {
            return;
        }

        if (!_interactionController.IsInteracting || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var result = _interactionController.MoveInteraction(e.GetPosition(RootGrid), RootGrid.ActualWidth, RootGrid.ActualHeight, Keyboard.Modifiers.HasFlag(ModifierKeys.Alt));

        if (_interactionController.IsMarqueeSelecting)
        {
            if (result.MarqueeRect is Rect rect)
            {
                ShowSelectionMarquee(rect);
            }
            return;
        }

        if (!result.Changed)
        {
            return;
        }

        _guidePresenter?.Show(new LayoutMoveResult(result.Changed, result.SnapX, result.SnapY));
    }

    private void RootGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_interactionController is null)
        {
            return;
        }

        var endResult = _interactionController.EndInteraction(Keyboard.Modifiers);
        if (!endResult.Ended)
        {
            return;
        }

        RootGrid.ReleaseMouseCapture();
        HideSelectionMarquee();
        _guidePresenter?.Hide();
        RefreshPropertiesPanel();
        QueueSaveLayout();
    }

    private void Window_OnKeyDown(object sender, WpfKeyEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        var modifiers = Keyboard.Modifiers;
        var handled = true;

        if (modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            _saveCoordinator?.SaveNow();
        }
        else if (modifiers == ModifierKeys.Control && e.Key == Key.C)
        {
            _session.CopySelected();
        }
        else if (modifiers == ModifierKeys.Control && e.Key == Key.V)
        {
            if (_session.PasteClipboard())
            {
                QueueSaveLayout();
            }
        }
        else if (modifiers == ModifierKeys.Control && e.Key == Key.D)
        {
            if (_session.DuplicateSelected())
            {
                QueueSaveLayout();
            }
        }
        else if (modifiers == ModifierKeys.Control && e.Key == Key.G)
        {
            if (_session.GroupSelectedItems())
            {
                QueueSaveLayout();
            }
        }
        else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.G)
        {
            if (_session.UngroupSelected())
            {
                QueueSaveLayout();
            }
        }
        else if (modifiers == ModifierKeys.Control && e.Key == Key.L)
        {
            if (_session.SetLockSelected(true))
            {
                QueueSaveLayout();
            }
        }
        else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.L)
        {
            if (_session.SetLockSelected(false))
            {
                QueueSaveLayout();
            }
        }
        else if (e.Key == Key.OemOpenBrackets)
        {
            if (modifiers.HasFlag(ModifierKeys.Shift) ? _session.SendToBackSelected() : _session.SendBackwardSelected())
            {
                QueueSaveLayout();
            }
        }
        else if (e.Key == Key.Oem6)
        {
            if (modifiers.HasFlag(ModifierKeys.Shift) ? _session.BringToFrontSelected() : _session.BringForwardSelected())
            {
                QueueSaveLayout();
            }
        }
        else if (e.Key == Key.Delete)
        {
            if (_session.DeleteSelected())
            {
                QueueSaveLayout();
            }
        }
        else if (e.Key == Key.Escape)
        {
            _session.SelectItem(null);
            RefreshPropertiesPanel();
            _guidePresenter?.Hide();
            HideSelectionMarquee();
        }
        else if (e.Key == Key.F5)
        {
            _saveCoordinator?.CancelPendingSave();
            _session.ReloadLayout();
            RefreshCatalog();
            RefreshPropertiesPanel();
            _guidePresenter?.Hide();
            HideSelectionMarquee();
        }
        else if (e.Key is Key.Left or Key.Right or Key.Up or Key.Down)
        {
            var step = modifiers.HasFlag(ModifierKeys.Shift) ? MoveStep * 5 : MoveStep;
            var dx = e.Key == Key.Left ? -step : e.Key == Key.Right ? step : 0;
            var dy = e.Key == Key.Up ? -step : e.Key == Key.Down ? step : 0;
            MoveSelected(dx, dy);
        }
        else
        {
            handled = false;
        }

        if (handled)
        {
            RefreshPropertiesPanel();
            e.Handled = true;
        }
    }

    private void MoveSelected(double deltaX, double deltaY)
    {
        if (_session is null)
        {
            return;
        }

        var result = _session.MoveSelectedWithSnap(deltaX, deltaY, RootGrid.ActualWidth, RootGrid.ActualHeight, Keyboard.Modifiers.HasFlag(ModifierKeys.Alt));
        if (result.Moved)
        {
            RefreshPropertiesPanel();
            QueueSaveLayout();
            _guidePresenter?.Show(result);
        }
    }

    private void ApplyProperties_OnClick(object sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        var current = _session.GetSelectedItemProperties();
        if (current is null)
        {
            RefreshPropertiesPanel();
            return;
        }

        if (!TryParseDouble(XTextBox.Text, out var x) ||
            !TryParseDouble(YTextBox.Text, out var y) ||
            !TryParseDouble(WidthTextBox.Text, out var width) ||
            !TryParseDouble(HeightTextBox.Text, out var height) ||
            !int.TryParse(ZIndexTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var zIndex))
        {
            System.Windows.MessageBox.Show(this, "Check X, Y, Width, Height and Z-Index values.", "Invalid properties", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var anyChanged = false;
        if (_session.UpdateSelectedItemProperties(x, y, width, height, zIndex, LockedCheckBox.IsChecked == true))
        {
            anyChanged = true;
        }

        if (CommonCornerSettingsPanel.Visibility == Visibility.Visible)
        {
            if (!TryParseNonNegativeDouble(CommonCornerTopLeftTextBox.Text, out var commonCornerTopLeft) ||
                !TryParseNonNegativeDouble(CommonCornerTopRightTextBox.Text, out var commonCornerTopRight) ||
                !TryParseNonNegativeDouble(CommonCornerBottomRightTextBox.Text, out var commonCornerBottomRight) ||
                !TryParseNonNegativeDouble(CommonCornerBottomLeftTextBox.Text, out var commonCornerBottomLeft))
            {
                System.Windows.MessageBox.Show(this, "Check corner radius values. Use non-negative numbers.", "Invalid corner properties", MessageBoxButton.OK, MessageBoxImage.Warning);
                RefreshPropertiesPanel();
                return;
            }

            if (_session.UpdateSelectedWidgetCornerSettings(commonCornerTopLeft, commonCornerTopRight, commonCornerBottomRight, commonCornerBottomLeft))
            {
                anyChanged = true;
            }
        }

        if (ShiftLedSettingsPanel.Visibility == Visibility.Visible)
        {
            if (!int.TryParse(ShiftLedCountTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var ledCount) ||
                ledCount < 4 || ledCount > 24 ||
                !IsValidColorValue(ShiftBackgroundColorTextBox.Text) ||
                !IsValidColorValue(ShiftOffColorTextBox.Text) ||
                !IsValidColorValue(ShiftOnColorTextBox.Text) ||
                !AreValidColorListValues(ShiftOnColorsTextBox.Text))
            {
                System.Windows.MessageBox.Show(this, "Check LED count and color values. Use 4-24 lamps and color formats like #20262E or #E6111827.", "Invalid LED properties", MessageBoxButton.OK, MessageBoxImage.Warning);
                RefreshPropertiesPanel();
                return;
            }

            var ledSettings = new ShiftLedDashboardSettings(
                LedCount: ledCount,
                ShowBackground: ShiftShowBackgroundCheckBox.IsChecked == true,
                BackgroundColor: NormalizeColorText(ShiftBackgroundColorTextBox.Text, "#E61F2937"),
                LedOffColor: NormalizeColorText(ShiftOffColorTextBox.Text, "#0F0F0F"),
                LedOnColor: NormalizeColorText(ShiftOnColorTextBox.Text, "#FF1E00"),
                UsePerLedColors: ShiftUsePerLedColorsCheckBox.IsChecked == true,
                LedOnColors: NormalizeColorListText(ShiftOnColorsTextBox.Text, ShiftLedDashboardSettingsDefaults.LegacyLikeLedOnColors),
                CornerTopLeft: TryParseNonNegativeDouble(CommonCornerTopLeftTextBox.Text, out var shiftCtl) ? shiftCtl : 0,
                CornerTopRight: TryParseNonNegativeDouble(CommonCornerTopRightTextBox.Text, out var shiftCtr) ? shiftCtr : 0,
                CornerBottomRight: TryParseNonNegativeDouble(CommonCornerBottomRightTextBox.Text, out var shiftCbr) ? shiftCbr : 0,
                CornerBottomLeft: TryParseNonNegativeDouble(CommonCornerBottomLeftTextBox.Text, out var shiftCbl) ? shiftCbl : 0);

            if (_session.UpdateSelectedShiftLedSettings(ledSettings))
            {
                anyChanged = true;
            }
        }

        if (anyChanged)
        {
            QueueSaveLayout();
        }

        RefreshPropertiesPanel();
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

    private void RefreshPropertiesPanel()
    {
        if (_session is null)
        {
            return;
        }

        var properties = _session.GetSelectedItemProperties();
        var hasSelection = properties is not null;
        PropertiesPanel.IsEnabled = hasSelection;

        if (!hasSelection)
        {
            SelectedWidgetNameTextBlock.Text = "No selection";
            SelectedWidgetMetaTextBlock.Text = string.Empty;
            PropertiesHintTextBlock.Text = "Select an item to edit its properties. Multi-select keeps one primary item here.";
            XTextBox.Text = string.Empty;
            YTextBox.Text = string.Empty;
            WidthTextBox.Text = string.Empty;
            HeightTextBox.Text = string.Empty;
            ZIndexTextBox.Text = string.Empty;
            LockedCheckBox.IsChecked = false;
            CommonCornerSettingsPanel.Visibility = Visibility.Collapsed;
            CommonCornerTopLeftTextBox.Text = string.Empty;
            CommonCornerTopRightTextBox.Text = string.Empty;
            CommonCornerBottomRightTextBox.Text = string.Empty;
            CommonCornerBottomLeftTextBox.Text = string.Empty;
            ShiftLedSettingsPanel.Visibility = Visibility.Collapsed;
            ShiftLedCountTextBox.Text = string.Empty;
            ShiftUsePerLedColorsCheckBox.IsChecked = false;
            ShiftShowBackgroundCheckBox.IsChecked = false;
            ShiftBackgroundColorTextBox.Text = string.Empty;
            ShiftOffColorTextBox.Text = string.Empty;
            ShiftOnColorTextBox.Text = string.Empty;
            ShiftOnColorsTextBox.Text = string.Empty;
            return;
        }

        SelectedWidgetNameTextBlock.Text = properties!.DisplayName;
        SelectedWidgetMetaTextBlock.Text = $"Type: {properties.TypeId} • Id: {properties.ItemId.ToString()[..8]}";
        PropertiesHintTextBlock.Text = properties.SelectedCount > 1
            ? $"Editing primary selected item. Current selection: {properties.SelectedCount} items{(properties.IsGrouped ? " in a group" : string.Empty)}."
            : properties.IsGrouped
                ? "This item belongs to a linked group. Position and size edits apply to the primary selected item only."
                : "Edit base layout properties for the selected item.";

        XTextBox.Text = FormatNumber(properties.X);
        YTextBox.Text = FormatNumber(properties.Y);
        WidthTextBox.Text = FormatNumber(properties.Width);
        HeightTextBox.Text = FormatNumber(properties.Height);
        ZIndexTextBox.Text = properties.ZIndex.ToString(CultureInfo.CurrentCulture);
        LockedCheckBox.IsChecked = properties.IsLocked;

        var commonCorners = _session.GetSelectedWidgetCornerSettings();
        if (commonCorners is null)
        {
            CommonCornerSettingsPanel.Visibility = Visibility.Collapsed;
            CommonCornerTopLeftTextBox.Text = string.Empty;
            CommonCornerTopRightTextBox.Text = string.Empty;
            CommonCornerBottomRightTextBox.Text = string.Empty;
            CommonCornerBottomLeftTextBox.Text = string.Empty;
        }
        else
        {
            CommonCornerSettingsPanel.Visibility = Visibility.Visible;
            CommonCornerTopLeftTextBox.Text = FormatNumber(commonCorners.TopLeft);
            CommonCornerTopRightTextBox.Text = FormatNumber(commonCorners.TopRight);
            CommonCornerBottomRightTextBox.Text = FormatNumber(commonCorners.BottomRight);
            CommonCornerBottomLeftTextBox.Text = FormatNumber(commonCorners.BottomLeft);
        }

        var shiftSettings = _session.GetSelectedShiftLedSettings();
        if (shiftSettings is null)
        {
            ShiftLedSettingsPanel.Visibility = Visibility.Collapsed;
            ShiftLedCountTextBox.Text = string.Empty;
            ShiftUsePerLedColorsCheckBox.IsChecked = false;
            ShiftShowBackgroundCheckBox.IsChecked = false;
            ShiftBackgroundColorTextBox.Text = string.Empty;
            ShiftOffColorTextBox.Text = string.Empty;
            ShiftOnColorTextBox.Text = string.Empty;
            ShiftOnColorsTextBox.Text = string.Empty;
        }
        else
        {
            ShiftLedSettingsPanel.Visibility = Visibility.Visible;
            ShiftLedCountTextBox.Text = shiftSettings.LedCount.ToString(CultureInfo.CurrentCulture);
            ShiftUsePerLedColorsCheckBox.IsChecked = shiftSettings.UsePerLedColors;
            ShiftShowBackgroundCheckBox.IsChecked = shiftSettings.ShowBackground;
            ShiftBackgroundColorTextBox.Text = shiftSettings.BackgroundColor;
            ShiftOffColorTextBox.Text = shiftSettings.LedOffColor;
            ShiftOnColorTextBox.Text = shiftSettings.LedOnColor;
            ShiftOnColorsTextBox.Text = shiftSettings.LedOnColors;
        }
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.##", CultureInfo.CurrentCulture);
    }

    private static bool TryParseDouble(string? text, out double value)
    {
        return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value);
    }

    private static bool IsValidColorValue(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        try
        {
            return MediaColorConverter.ConvertFromString(text.Trim()) is MediaColor;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    private static string NormalizeColorText(string? text, string fallback)
    {
        return string.IsNullOrWhiteSpace(text) ? fallback : text.Trim();
    }

    private static bool AreValidColorListValues(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        foreach (var part in text.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!IsValidColorValue(part))
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeColorListText(string? text, string fallback)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return fallback;
        }

        var values = text.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(IsValidColorValue)
            .Select(x => x.Trim())
            .ToArray();

        return values.Length == 0 ? fallback : string.Join(",", values);
    }

    private static bool TryParseNonNegativeDouble(string? text, out double value)
    {
        if (!double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value))
        {
            return false;
        }

        return value >= 0;
    }

    private void PickShiftBackgroundColor_OnClick(object sender, RoutedEventArgs e) => PickColorInto(ShiftBackgroundColorTextBox!, "#E61F2937");
    private void PickShiftOffColor_OnClick(object sender, RoutedEventArgs e) => PickColorInto(ShiftOffColorTextBox!, "#0F0F0F");
    private void PickShiftOnColor_OnClick(object sender, RoutedEventArgs e) => PickColorInto(ShiftOnColorTextBox!, "#FF1E00");

    private void PickColorInto(WpfTextBox targetTextBox, string fallback)
    {
        var initial = ParseMediaColor(targetTextBox.Text, fallback);
        using var dialog = new Forms.ColorDialog
        {
            FullOpen = true,
            AnyColor = true,
            Color = System.Drawing.Color.FromArgb(initial.A, initial.R, initial.G, initial.B)
        };

        if (dialog.ShowDialog() != Forms.DialogResult.OK)
        {
            return;
        }

        targetTextBox.Text = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
    }

    private static MediaColor ParseMediaColor(string? text, string fallback)
    {
        if (IsValidColorValue(text))
        {
            return (MediaColor)MediaColorConverter.ConvertFromString(text!.Trim())!;
        }

        return (MediaColor)MediaColorConverter.ConvertFromString(fallback)!;
    }

    private void QueueSaveLayout() => _saveCoordinator?.QueueSave();

    private void ExecuteAndSave(Func<bool> action)
    {
        if (action())
        {
            RefreshPropertiesPanel();
            QueueSaveLayout();
        }
        else
        {
            RefreshPropertiesPanel();
        }
    }

    private void ShowSelectionMarquee(Rect rect)
    {
        SelectionMarquee.Visibility = Visibility.Visible;
        SelectionMarquee.Margin = new Thickness(rect.X, rect.Y, 0, 0);
        SelectionMarquee.Width = Math.Max(0, rect.Width);
        SelectionMarquee.Height = Math.Max(0, rect.Height);
    }

    private void HideSelectionMarquee()
    {
        SelectionMarquee.Visibility = Visibility.Collapsed;
        SelectionMarquee.Width = 0;
        SelectionMarquee.Height = 0;
        SelectionMarquee.Margin = new Thickness(0);
    }

    protected override void OnClosed(EventArgs e)
    {
        _saveCoordinator?.SaveNow();
        _timer?.Stop();
        base.OnClosed(e);
    }
}
