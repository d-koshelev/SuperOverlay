using System.Diagnostics;
using IRSDKSharper;
using SuperOverlay.iRacing.Telemetry.Mock;

namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed class IRacingTelemetryProvider
{
    private readonly object _gate = new();
    private readonly IRacingSdk _sdk;
    private readonly IRacingSnapshotMapper _snapshotMapper = new();
    private readonly MockTelemetryProvider _mockTelemetryProvider = new();
    private readonly IRacingTelemetryRawStore _telemetryRawStore = new();
    private readonly IRacingSessionInfoStore _sessionInfoStore = new();

    private IRacingNormalizedSnapshot? _latestSnapshot;
    private Exception? _lastException;
    private bool _started;

    public IRacingTelemetryProvider(bool throwYamlExceptions = false)
    {
        Debug.WriteLine("[SO] Provider constructed");
        _sdk = new IRacingSdk(throwYamlExceptions);
        _sdk.UpdateInterval = 1;
        _sdk.OnTelemetryData += HandleTelemetryData;
        Debug.WriteLine("[SO] Telemetry event subscribed");
        _sdk.OnSessionInfo += HandleSessionInfo;
        Debug.WriteLine("[SO] SessionInfo event subscribed");
        _sdk.OnDisconnected += HandleDisconnected;
        _sdk.OnException += ex =>
        {
            Debug.WriteLine($"[SO] SDK exception: {ex}");
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
        Debug.WriteLine($"[SO] Provider start requested started={_started}");
        if (_started)
        {
            return;
        }

        Debug.WriteLine("[SO] SDK start requested");
        _sdk.Start();
        _started = true;
        Debug.WriteLine("[SO] Provider marked started");
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

        Debug.WriteLine("[SO] TryGetLatestSnapshot -> fallback");
        snapshot = CreateFallbackSnapshot();
        return false;
    }

    public bool TryGetLatestTelemetryRaw(out IRacingTelemetryRawState telemetryRaw)
        => _telemetryRawStore.TryGetLatest(out telemetryRaw);

    public bool TryGetLatestSessionInfo(out IRacingSessionInfoRawState sessionInfo)
        => _sessionInfoStore.TryGetLatest(out sessionInfo);

    private void HandleTelemetryData()
    {
        Debug.WriteLine($"[SO] HandleTelemetryData connected={_sdk.IsConnected} tick={_sdk.Data.TickCount}");
        try
        {
            var telemetryState = _snapshotMapper.CreateTelemetryRawState(_sdk.Data);
            _telemetryRawStore.Set(telemetryState);
            telemetryState.Fields.TryGetValue("Speed", out var speedObj);
            telemetryState.Fields.TryGetValue("Gear", out var gearObj);
            Debug.WriteLine($"[SO] TelemetryRaw updated count={telemetryState.Fields.Count} speed={speedObj ?? "<null>"} gear={gearObj ?? "<null>"}");
        }
        catch (Exception ex)
        {
            lock (_gate)
            {
                _lastException = ex;
            }
        }

        UpdateLatestSnapshot();
    }

    private void HandleSessionInfo()
    {
        Debug.WriteLine($"[SO] HandleSessionInfo update={_sdk.Data.SessionInfoUpdate}");
        try
        {
            var sessionState = _snapshotMapper.CreateSessionInfoRawState(_sdk.Data);
            if (sessionState is not null)
            {
                _sessionInfoStore.Set(sessionState);
                sessionState.Fields.TryGetValue("WeekendInfo.TrackDisplayName", out var trackObj);
                Debug.WriteLine($"[SO] SessionInfoRaw updated count={sessionState.Fields.Count} track={trackObj ?? "<null>"}");
            }
        }
        catch (Exception ex)
        {
            lock (_gate)
            {
                _lastException = ex;
            }
        }

        UpdateLatestSnapshot();
    }

    private void HandleDisconnected()
    {
        Debug.WriteLine("[SO] HandleDisconnected");
        lock (_gate)
        {
            _latestSnapshot = null;
        }

        _telemetryRawStore.Clear();
        _sessionInfoStore.Clear();
    }

    private void UpdateLatestSnapshot()
    {
        try
        {
            var snapshot = _snapshotMapper.CreateNormalizedSnapshot(_sdk);
            Debug.WriteLine($"[SO] Snapshot updated connected={snapshot.Connection.IsConnected} speed={snapshot.Vehicle.SpeedKph} gear={snapshot.Vehicle.Gear} rawTelemetryCount={snapshot.RawTelemetry.Values.Count} rawSessionCount={snapshot.RawSession.Fields.Count}");
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
        var rawTelemetryValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Speed"] = speed,
            ["RPM"] = rpm,
            ["Gear"] = gear,
            ["ShiftIndicatorPct"] = shiftLightPercent,
            ["SpeedKph"] = speed
        };
        var rawTelemetry = new IRacingRawTelemetrySnapshot(
            DateTimeOffset.UtcNow,
            0,
            0,
            rawTelemetryValues);
        var rawSessionFields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["WeekendInfo.TrackDisplayName"] = "Mock Track",
            ["WeekendInfo.TrackConfigName"] = "Grand Prix",
            ["WeekendInfo.TrackType"] = "Road",
            ["WeekendInfo.TrackWeatherType"] = "Constant",
            ["WeekendInfo.TrackSkies"] = "Partly Cloudy",
            ["DriverInfo.DriverCarIdx"] = 0,
            ["DriverInfo.DriverUserID"] = 1,
            ["DriverInfo.DriverSetupName"] = "Baseline",
            ["DriverInfo.DriverCarRedLine"] = rpm > 0 ? rpm + 1500 : 9000,
            ["DriverInfo.DriverCarFuelMaxLtr"] = 100,
            ["SessionInfo.CurrentSessionNum"] = 0
        };
        var rawSession = new IRacingRawSessionSnapshot(
            DateTimeOffset.UtcNow,
            0,
            string.Empty,
            rawSessionFields,
            "Practice",
            "Open Practice",
            "Mock Track",
            "Mock Car");

        return new IRacingNormalizedSnapshot(
            DateTimeOffset.UtcNow,
            new IRacingConnectionState(_started, false, 0, 0, 0, 0),
            new IRacingVehicleState(speed, rpm, gear, shiftLightPercent, false, false, default(IRacingSdkEnum.EngineWarnings), IRacingSdkEnum.CarLeftRight.Off),
            new IRacingInputState(0, 0, 0),
            new IRacingFlagState(default(IRacingSdkEnum.Flags), default(IRacingSdkEnum.PaceFlags), IRacingSdkEnum.SessionState.Invalid, IRacingSdkEnum.PaceMode.NotPacing),
            new IRacingPitState(false, false, default(IRacingSdkEnum.PitSvFlags), IRacingSdkEnum.PitSvStatus.None, 0, 0, 0, 0),
            new IRacingTrackState(IRacingSdkEnum.TrkLoc.NotInWorld, IRacingSdkEnum.TrkSurf.SurfaceNotInWorld, IRacingSdkEnum.TrackWetness.Unknown, 0, 0, 0),
            new IRacingSessionStateBlock(0, rawSession.SessionName, rawSession.SessionType, rawSession.TrackDisplayName, rawSession.CarScreenName, 0),
            rawTelemetry,
            rawSession);
    }
}
