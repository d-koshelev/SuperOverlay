using WpfWindow = System.Windows.Window;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SuperOverlay.LayoutBuilder.Runtime;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Telemetry.Mock;

namespace SuperOverlay.iRacing.Runtime;

internal sealed class RuntimeWindowController
{
    private readonly WpfWindow _owner;
    private readonly Grid _rootGrid;
    private readonly Border _runtimeHintBorder;
    private readonly Border _editOverlayBar;
    private readonly MockTelemetryProvider _telemetry;
    private readonly IRacingMapper _mapper;
    private readonly OverlayRuntimeBootstrapper _bootstrapper;

    private OverlayRuntimeSession? _session;
    private DispatcherTimer? _timer;
    private bool _isMoveEditEnabled;

    public RuntimeWindowController(
        WpfWindow owner,
        Grid rootGrid,
        Border runtimeHintBorder,
        Border editOverlayBar,
        MockTelemetryProvider telemetry,
        IRacingMapper mapper,
        OverlayRuntimeBootstrapper bootstrapper)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _rootGrid = rootGrid ?? throw new ArgumentNullException(nameof(rootGrid));
        _runtimeHintBorder = runtimeHintBorder ?? throw new ArgumentNullException(nameof(runtimeHintBorder));
        _editOverlayBar = editOverlayBar ?? throw new ArgumentNullException(nameof(editOverlayBar));
        _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
    }

    public OverlayRuntimeSession? Session => _session;
    public bool IsMoveEditEnabled => _isMoveEditEnabled;

    public void Initialize()
    {
        ConfigureWindowBounds();
        BuildSession(OverlayShellMode.Runtime);
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

    public void ToggleMoveEdit()
    {
        _owner.Focus();

        if (_isMoveEditEnabled)
        {
            CancelMoveEdit();
            return;
        }

        BuildSession(OverlayShellMode.RuntimeMoveEdit);
    }

    public void ApplyMoveEdit()
    {
        if (_session is null || !_isMoveEditEnabled)
        {
            return;
        }

        _session.SaveLayout();
        BuildSession(OverlayShellMode.Runtime);
    }

    public void CancelMoveEdit()
    {
        if (_session is null || !_isMoveEditEnabled)
        {
            return;
        }

        _session.ReloadLayout();
        BuildSession(OverlayShellMode.Runtime);
    }

    public void Stop()
    {
        _timer?.Stop();
    }

    private void BuildSession(OverlayShellMode shellMode)
    {
        _rootGrid.Children.Clear();
        _session = _bootstrapper.Build(_rootGrid, shellMode);
        _session.SetSnappingEnabled(false);

        _runtimeHintBorder.Visibility = shellMode == OverlayShellMode.Runtime ? Visibility.Visible : Visibility.Collapsed;
        _editOverlayBar.Visibility = shellMode == OverlayShellMode.RuntimeMoveEdit ? Visibility.Visible : Visibility.Collapsed;
        _isMoveEditEnabled = shellMode == OverlayShellMode.RuntimeMoveEdit;
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
}
