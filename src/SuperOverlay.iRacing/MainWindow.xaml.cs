using System.Windows;
using System.Windows.Threading;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Telemetry.Mock;

namespace SuperOverlay.iRacing;

public partial class MainWindow : Window
{
    private readonly MockTelemetryProvider _telemetry = new();
    private readonly IRacingMapper _mapper = new();
    private readonly OverlayRuntimeBootstrapper _bootstrapper = new();

    private OverlayRuntimeSession _session = null!;
    private DispatcherTimer _timer = null!;

    public MainWindow()
    {
        InitializeComponent();

        _session = _bootstrapper.Build(RootGrid);

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
}
