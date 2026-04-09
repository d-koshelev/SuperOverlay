using SuperOverlay.Dashboards.Registry;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Telemetry.Mock;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace SuperOverlay.iRacing;

public partial class MainWindow : Window
{
    private const double MoveStep = 10;
    private const double ResizeStep = 10;

    private readonly MockTelemetryProvider _telemetry = new();
    private readonly IRacingMapper _mapper = new();
    private readonly OverlayRuntimeBootstrapper _bootstrapper = new();

    private OverlayRuntimeSession _session = null!;
    private DispatcherTimer _timer = null!;

    private bool _isDraggingCanvas;
    private Point _lastCanvasPoint;
    private bool _snapEnabled = true;

    public MainWindow()
    {
        InitializeComponent();

        _session = _bootstrapper.Build(RootGrid);
        RefreshCatalogList();
        RefreshItemList();
        HideGuides();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
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
        _snapEnabled = true;

        if (SnapToggleButton is not null)
        {
            SnapToggleButton.Content = "Snap: On";
        }
    }

    private void SnapToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        _snapEnabled = false;
        HideGuides();

        if (SnapToggleButton is not null)
        {
            SnapToggleButton.Content = "Snap: Off";
        }
    }

    private void AddItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (CatalogComboBox.SelectedItem is not DashboardCatalogItem item)
        {
            return;
        }

        if (_session.AddItem(item.TypeId))
        {
            RefreshItemList();
            _session.SaveLayout();
        }
    }

    private void ItemComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
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

    private void WidthDown_OnClick(object sender, RoutedEventArgs e) => ResizeSelected(-ResizeStep, 0);
    private void WidthUp_OnClick(object sender, RoutedEventArgs e) => ResizeSelected(ResizeStep, 0);
    private void HeightDown_OnClick(object sender, RoutedEventArgs e) => ResizeSelected(0, -ResizeStep);
    private void HeightUp_OnClick(object sender, RoutedEventArgs e) => ResizeSelected(0, ResizeStep);

    private void DeleteSelected_OnClick(object sender, RoutedEventArgs e)
    {
        if (_session.DeleteSelected())
        {
            RefreshItemList();
            _session.SaveLayout();
            HideGuides();
        }
    }

    private void SaveLayout_OnClick(object sender, RoutedEventArgs e)
    {
        _session.SaveLayout();
    }

    private void ReloadLayout_OnClick(object sender, RoutedEventArgs e)
    {
        _session.ReloadLayout();
        RefreshItemList();
        HideGuides();
    }

    private void RootGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_session.GetSelectedItemId() is null)
        {
            return;
        }

        _isDraggingCanvas = true;
        _lastCanvasPoint = e.GetPosition(RootGrid);
        RootGrid.CaptureMouse();
    }

    private void RootGrid_OnMouseMove(object sender, MouseEventArgs e)
    {
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

        if (_session.MoveSelected(dx, dy))
        {
            _lastCanvasPoint = current;
        }

        if (!_snapEnabled || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            HideGuides();
            return;
        }

        UpdateGuides(current);
    }

    private void RootGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDraggingCanvas)
        {
            return;
        }

        _isDraggingCanvas = false;
        RootGrid.ReleaseMouseCapture();
        _session.SaveLayout();
        RefreshItemList();
        HideGuides();
    }

    private void MoveSelected(double deltaX, double deltaY)
    {
        if (_session.MoveSelected(deltaX, deltaY))
        {
            RefreshItemList();
            _session.SaveLayout();
        }
    }

    private void ResizeSelected(double deltaWidth, double deltaHeight)
    {
        if (_session.ResizeSelected(deltaWidth, deltaHeight))
        {
            RefreshItemList();
            _session.SaveLayout();
        }
    }

    private void RefreshCatalogList()
    {
        var items = _session.GetCatalog();
        CatalogComboBox.ItemsSource = items;
        CatalogComboBox.SelectedItem = items.FirstOrDefault();
    }

    private void RefreshItemList()
    {
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

    private void UpdateGuides(Point current)
    {
        if (VerticalGuide is not null)
        {
            VerticalGuide.Margin = new Thickness(current.X, 0, 0, 0);
            VerticalGuide.Visibility = Visibility.Visible;
        }

        if (HorizontalGuide is not null)
        {
            HorizontalGuide.Margin = new Thickness(0, current.Y, 0, 0);
            HorizontalGuide.Visibility = Visibility.Visible;
        }
    }
}