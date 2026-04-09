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

    private bool _isUpdatingItemListSelection;

    private LayoutEditorInteractionController? _interactionController;
    private LayoutSaveCoordinator? _saveCoordinator;
    private LayoutGuidePresenter? _guidePresenter;

    public MainWindow()
    {
        InitializeComponent();

        _session = _bootstrapper.Build(RootGrid);
        _interactionController = new LayoutEditorInteractionController(_session);
        _saveCoordinator = new LayoutSaveCoordinator(
            TimeSpan.FromMilliseconds(250),
            () => _session?.SaveLayout());
        _guidePresenter = new LayoutGuidePresenter(VerticalGuide, HorizontalGuide);

        _session.SetSnappingEnabled(true);
        RefreshItemList();
        _guidePresenter.Hide();

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
        if (_session is null)
        {
            return;
        }

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

    private void ItemComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_session is null || _isUpdatingItemListSelection)
        {
            return;
        }

        if (ItemComboBox.SelectedItem is LayoutEditorItem item)
        {
            _session.SelectItem(item.Id);
            SyncItemListSelection();
            return;
        }

        _session.SelectItem(null);
        SyncItemListSelection();
    }

    private void MoveLeft_OnClick(object sender, RoutedEventArgs e) => MoveSelected(-MoveStep, 0);
    private void MoveRight_OnClick(object sender, RoutedEventArgs e) => MoveSelected(MoveStep, 0);
    private void MoveUp_OnClick(object sender, RoutedEventArgs e) => MoveSelected(0, -MoveStep);
    private void MoveDown_OnClick(object sender, RoutedEventArgs e) => MoveSelected(0, MoveStep);

    private void SaveLayout_OnClick(object sender, RoutedEventArgs e)
    {
        _saveCoordinator?.SaveNow();
    }

    private void ReloadLayout_OnClick(object sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        _saveCoordinator?.CancelPendingSave();
        _session.ReloadLayout();
        RefreshItemList();
        _guidePresenter?.Hide();
    }

    private void RootGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_session is null || _interactionController is null)
        {
            return;
        }

        var startResult = _interactionController.BeginInteraction(
            e.OriginalSource as DependencyObject,
            e.GetPosition(RootGrid));

        SyncItemListSelection();

        if (startResult.ClearedSelection)
        {
            _guidePresenter?.Hide();
            return;
        }

        if (startResult.CaptureMouse)
        {
            RootGrid.CaptureMouse();
            e.Handled = true;
        }
    }

    private void RootGrid_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_session is null || _interactionController is null)
        {
            return;
        }

        if (!_interactionController.IsInteracting || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var result = _interactionController.MoveInteraction(
            e.GetPosition(RootGrid),
            RootGrid.ActualWidth,
            RootGrid.ActualHeight,
            Keyboard.Modifiers.HasFlag(ModifierKeys.Alt));

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

        if (!_interactionController.EndInteraction())
        {
            return;
        }

        RootGrid.ReleaseMouseCapture();
        QueueSaveLayout();
        SyncItemListSelection();
        _guidePresenter?.Hide();
    }

    private void MoveSelected(double deltaX, double deltaY)
    {
        if (_session is null)
        {
            return;
        }

        var result = _session.MoveSelectedWithSnap(
            deltaX,
            deltaY,
            RootGrid.ActualWidth,
            RootGrid.ActualHeight,
            Keyboard.Modifiers.HasFlag(ModifierKeys.Alt));

        if (result.Moved)
        {
            QueueSaveLayout();
            _guidePresenter?.Show(result);
        }
    }

    private void RefreshItemList()
    {
        if (_session is null)
        {
            return;
        }

        var items = _session.GetLayoutItems();

        _isUpdatingItemListSelection = true;
        ItemComboBox.ItemsSource = items;
        _isUpdatingItemListSelection = false;

        SyncItemListSelection();
    }

    private void SyncItemListSelection()
    {
        if (_session is null)
        {
            return;
        }

        var selectedId = _session.GetSelectedItemId();
        var items = ItemComboBox.ItemsSource as IEnumerable<LayoutEditorItem>;
        var selectedItem = selectedId is null || items is null
            ? null
            : items.FirstOrDefault(x => x.Id == selectedId.Value);

        _isUpdatingItemListSelection = true;
        ItemComboBox.SelectedItem = selectedItem;
        _isUpdatingItemListSelection = false;
    }

    private void QueueSaveLayout()
    {
        _saveCoordinator?.QueueSave();
    }

    protected override void OnClosed(EventArgs e)
    {
        _saveCoordinator?.SaveNow();
        _timer?.Stop();
        base.OnClosed(e);
    }
}
