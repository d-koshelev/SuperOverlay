using System.Windows;
using System.Windows.Controls;
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

    private OverlayRuntimeSession _session = null!;
    private DispatcherTimer _timer = null!;

    public MainWindow()
    {
        InitializeComponent();

        _session = _bootstrapper.Build(RootGrid);
        RefreshItemList();

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

    private void ItemComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemComboBox.SelectedItem is LayoutEditorItem item)
        {
            _session.SelectItem(item.Id);
            return;
        }

        _session.SelectItem(null);
    }

    private void MoveLeft_OnClick(object sender, RoutedEventArgs e)
    {
        MoveSelected(-MoveStep, 0);
    }

    private void MoveRight_OnClick(object sender, RoutedEventArgs e)
    {
        MoveSelected(MoveStep, 0);
    }

    private void MoveUp_OnClick(object sender, RoutedEventArgs e)
    {
        MoveSelected(0, -MoveStep);
    }

    private void MoveDown_OnClick(object sender, RoutedEventArgs e)
    {
        MoveSelected(0, MoveStep);
    }

    private void SaveLayout_OnClick(object sender, RoutedEventArgs e)
    {
        _session.SaveLayout();
    }

    private void ReloadLayout_OnClick(object sender, RoutedEventArgs e)
    {
        _session.ReloadLayout();
        RefreshItemList();
    }

    private void MoveSelected(double deltaX, double deltaY)
    {
        if (_session.MoveSelected(deltaX, deltaY))
        {
            RefreshItemList();
        }
    }

    private void RefreshItemList()
    {
        var items = _session.GetLayoutItems();
        var selectedId = _session.GetSelectedItemId();

        ItemComboBox.ItemsSource = items;
        ItemComboBox.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items.FirstOrDefault();
    }
}
