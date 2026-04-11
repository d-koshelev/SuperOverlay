using IRSDKSharper;
using SuperOverlay.iRacing.Telemetry.Mock;

namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed class IRacingTelemetryProvider
{
    private readonly object _gate = new();
    private readonly IRacingSdk _sdk;
    private readonly IRacingSnapshotMapper _snapshotMapper = new();
    private readonly MockTelemetryProvider _mockTelemetryProvider = new();

    private IRacingNormalizedSnapshot? _latestSnapshot;
    private Exception? _lastException;
    private bool _started;

    public IRacingTelemetryProvider(bool throwYamlExceptions = false)
    {
        _sdk = new IRacingSdk(throwYamlExceptions);
        _sdk.UpdateInterval = 1;
        _sdk.OnTelemetryData += HandleTelemetryData;
        _sdk.OnSessionInfo += HandleSessionInfo;
        _sdk.OnDisconnected += HandleDisconnected;
        _sdk.OnException += ex =>
        {
            lock (_gate)
            {
                _lastException = ex;
            }
        };
    }

    public bool IsConnected => _sdk.IsConnected;

    public Exception? LastException
    {
        get
        {
            lock (_gate)
            {
                return _lastException;
            }
        }
    }

    public void Start()
    {
        if (_started)
        {
            return;
        }

        _sdk.Start();
        _started = true;
    }

    public void Stop()
    {
        if (!_started)
        {
            return;
        }

        _sdk.Stop();
        _started = false;
    }

    public bool TryGetLatestSnapshot(out IRacingNormalizedSnapshot snapshot)
    {
        lock (_gate)
        {
            if (_latestSnapshot is not null)
            {
                snapshot = _latestSnapshot;
                return true;
            }
        }

        snapshot = CreateFallbackSnapshot();
        return false;
    }

    private void HandleTelemetryData() => UpdateLatestSnapshot();

    private void HandleSessionInfo() => UpdateLatestSnapshot();

    private void HandleDisconnected()
    {
        lock (_gate)
        {
            _latestSnapshot = null;
        }
    }

    private void UpdateLatestSnapshot()
    {
        try
        {
            var snapshot = _snapshotMapper.CreateNormalizedSnapshot(_sdk);
            lock (_gate)
            {
                _latestSnapshot = snapshot;
            }
        }
        catch (Exception ex)
        {
            lock (_gate)
            {
                _lastException = ex;
            }
        }
    }

    private IRacingNormalizedSnapshot CreateFallbackSnapshot()
    {
        var (speed, rpm, gear, shiftLightPercent) = _mockTelemetryProvider.Get();
        var rawTelemetry = new IRacingRawTelemetrySnapshot(
            DateTimeOffset.UtcNow,
            0,
            0,
            new Dictionary<string, object?>
            {
                ["SpeedKph"] = speed,
                ["RPM"] = rpm,
                ["Gear"] = gear,
                ["ShiftIndicatorPct"] = shiftLightPercent
            });

        return new IRacingNormalizedSnapshot(
            DateTimeOffset.UtcNow,
            new IRacingConnectionState(_started, false, 0, 0, 0, 0),
            new IRacingVehicleState(speed, rpm, gear, shiftLightPercent, false, false, default(IRacingSdkEnum.EngineWarnings), IRacingSdkEnum.CarLeftRight.Off),
            new IRacingInputState(0, 0, 0),
            new IRacingFlagState(default(IRacingSdkEnum.Flags), default(IRacingSdkEnum.PaceFlags), IRacingSdkEnum.SessionState.Invalid, IRacingSdkEnum.PaceMode.NotPacing),
            new IRacingPitState(false, false, default(IRacingSdkEnum.PitSvFlags), IRacingSdkEnum.PitSvStatus.None, 0, 0, 0, 0),
            new IRacingTrackState(IRacingSdkEnum.TrkLoc.NotInWorld, IRacingSdkEnum.TrkSurf.SurfaceNotInWorld, IRacingSdkEnum.TrackWetness.Unknown, 0, 0, 0),
            new IRacingSessionStateBlock(0, "Mock", "Mock", null, null, 0),
            rawTelemetry,
            null);
    }
}
