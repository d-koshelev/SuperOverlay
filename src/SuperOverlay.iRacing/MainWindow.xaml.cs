using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Mapping;
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

    private bool _isDraggingCanvas;
    private Point _lastCanvasPoint;

    public MainWindow()
    {
        InitializeComponent();

        _session = _bootstrapper.Build(RootGrid);
        _session.SetSnappingEnabled(true);
        RefreshItemList();
        HideGuides();

        if (SnapToggleButton is not null)
        {
            SnapToggleButton.IsChecked = true;
            SnapToggleButton.Content = "Snap: On";
        }

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_session is null) return;

        var (speed, rpm, gear) = _telemetry.Get();
        var state = _mapper.Map(speed, rpm, gear);
        _session.Update(state);
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
        if (_session is null) return;

        _session.SetSnappingEnabled(true);
        SnapToggleButton.Content = "Snap: On";
    }

    private void SnapToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (_session is null) return;

        _session.SetSnappingEnabled(false);
        SnapToggleButton.Content = "Snap: Off";
        HideGuides();
    }

    private void ItemComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_session is null) return;

        if (ItemComboBox.SelectedItem is LayoutEditorItem item)
        {
            _session.SelectItem(item.Id);
            return;
        }

        _session.SelectItem(null);
    }

    private void MoveLeft_OnClick(object sender, RoutedEventArgs e) => MoveSelected(-MoveStep, 0);
    private void MoveRight_OnClick(object sender, RoutedEventArgs e) => MoveSelected(MoveStep, 0);
    private void MoveUp_OnClick(object sender, RoutedEventArgs e) => MoveSelected(0, -MoveStep);
    private void MoveDown_OnClick(object sender, RoutedEventArgs e) => MoveSelected(0, MoveStep);

    private void SaveLayout_OnClick(object sender, RoutedEventArgs e)
    {
        _session?.SaveLayout();
    }

    private void ReloadLayout_OnClick(object sender, RoutedEventArgs e)
    {
        if (_session is null) return;

        _session.ReloadLayout();
        RefreshItemList();
        HideGuides();
    }

    private void RootGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_session?.GetSelectedItemId() is null)
        {
            return;
        }

        _isDraggingCanvas = true;
        _lastCanvasPoint = e.GetPosition(RootGrid);
        RootGrid.CaptureMouse();
    }

    private void RootGrid_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_session is null) return;

        if (!_isDraggingCanvas || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var current = e.GetPosition(RootGrid);
        var dx = current.X - _lastCanvasPoint.X;
        var dy = current.Y - _lastCanvasPoint.Y;

        if (Math.Abs(dx) < double.Epsilon && Math.Abs(dy) < double.Epsilon)
        {
            return;
        }

        var result = _session.MoveSelectedWithSnap(
            dx,
            dy,
            RootGrid.ActualWidth,
            RootGrid.ActualHeight,
            Keyboard.Modifiers.HasFlag(ModifierKeys.Alt));

        if (result.Moved)
        {
            _lastCanvasPoint = current;
            RefreshItemList();
            ShowGuides(result);
        }
    }

    private void RootGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_session is null) return;

        if (!_isDraggingCanvas)
        {
            return;
        }

        _isDraggingCanvas = false;
        RootGrid.ReleaseMouseCapture();
        _session.EndDrag();
        _session.SaveLayout();
        RefreshItemList();
        HideGuides();
    }

    private void MoveSelected(double deltaX, double deltaY)
    {
        if (_session is null) return;

        var result = _session.MoveSelectedWithSnap(
            deltaX,
            deltaY,
            RootGrid.ActualWidth,
            RootGrid.ActualHeight,
            Keyboard.Modifiers.HasFlag(ModifierKeys.Alt));

        if (result.Moved)
        {
            RefreshItemList();
            _session.SaveLayout();
            ShowGuides(result);
        }
    }

    private void RefreshItemList()
    {
        if (_session is null) return;

        var items = _session.GetLayoutItems();
        var selectedId = _session.GetSelectedItemId();

        ItemComboBox.ItemsSource = items;
        ItemComboBox.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items.FirstOrDefault();
    }

    private void HideGuides()
    {
        if (VerticalGuide is not null)
        {
            VerticalGuide.Visibility = Visibility.Collapsed;
        }

        if (HorizontalGuide is not null)
        {
            HorizontalGuide.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowGuides(LayoutMoveResult result)
    {
        if (result.SnapX is not null)
        {
            VerticalGuide.Margin = new Thickness(result.SnapX.Value, 0, 0, 0);
            VerticalGuide.Visibility = Visibility.Visible;
        }
        else
        {
            VerticalGuide.Visibility = Visibility.Collapsed;
        }

        if (result.SnapY is not null)
        {
            HorizontalGuide.Margin = new Thickness(0, result.SnapY.Value, 0, 0);
            HorizontalGuide.Visibility = Visibility.Visible;
        }
        else
        {
            HorizontalGuide.Visibility = Visibility.Collapsed;
        }
    }
}