using WpfWindow = System.Windows.Window;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Telemetry.IRacing;

namespace SuperOverlay.iRacing.Runtime;

internal sealed class RuntimeWindowController
{
    private readonly WpfWindow _owner;
    private readonly Grid _rootGrid;
    private readonly IRacingTelemetryProvider _telemetry;
    private readonly IRacingMapper _mapper;
    private readonly OverlayRuntimeBootstrapper _bootstrapper;

    private OverlayRuntimeSession? _session;
    private DispatcherTimer? _timer;

    public RuntimeWindowController(
        WpfWindow owner,
        Grid rootGrid,
        IRacingTelemetryProvider telemetry,
        IRacingMapper mapper,
        OverlayRuntimeBootstrapper bootstrapper)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _rootGrid = rootGrid ?? throw new ArgumentNullException(nameof(rootGrid));
        _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
    }

    public OverlayRuntimeSession? Session => _session;
    public bool IsMoveEditEnabled => false;

    public void Initialize()
    {
        Debug.WriteLine("[SO] RuntimeWindowController.Initialize");
        ConfigureWindowBounds();
        BuildSession();
        _telemetry.Start();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void ConfigureWindowBounds()
    {
        _owner.Left = SystemParameters.VirtualScreenLeft;
        _owner.Top = SystemParameters.VirtualScreenTop;
        _owner.Width = SystemParameters.VirtualScreenWidth;
        _owner.Height = SystemParameters.VirtualScreenHeight;
    }

    public void Stop()
    {
        _timer?.Stop();
        _telemetry.Stop();
    }

    private void BuildSession()
    {
        _rootGrid.Children.Clear();
        _session = _bootstrapper.Build(_rootGrid, SuperOverlay.Core.Layouts.Runtime.OverlayShellMode.Runtime);
        _session.SetSnappingEnabled(false);
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_session is null)
        {
            Debug.WriteLine("[SO] Runtime tick skipped: session null");
            return;
        }

        var hasLiveSnapshot = _telemetry.TryGetLatestSnapshot(out var snapshot);
        Debug.WriteLine($"[SO] Runtime tick hasLiveSnapshot={hasLiveSnapshot} connected={snapshot.Connection.IsConnected} speed={snapshot.Vehicle.SpeedKph} gear={snapshot.Vehicle.Gear}");
        _session.Update(_mapper.Map(snapshot));
    }
}
