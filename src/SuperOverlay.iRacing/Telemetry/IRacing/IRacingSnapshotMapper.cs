using IRSDKSharper;

namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed class IRacingSnapshotMapper
{
    private static readonly string[] RawSampleKeys =
    {
        "Speed", "RPM", "Gear", "ShiftIndicatorPct", "Throttle", "Brake", "Clutch",
        "SessionFlags", "EngineWarnings", "CarLeftRight", "OnPitRoad", "LapDistPct",
        "PlayerTrackSurface", "PlayerTrackSurfaceMaterial", "PitSvFlags", "PitSvStatus",
        "FuelLevel", "FuelLevelPct", "FuelUsePerHour", "FuelPress", "TrackTemp", "AirTemp",
        "TrackWetness", "PaceFlags", "PaceMode", "SessionState", "SteeringWheelAngle"
    };

    public IRacingRawTelemetrySnapshot CreateRawTelemetrySnapshot(IRacingSdkData data)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in RawSampleKeys)
        {
            if (!data.TelemetryDataProperties.ContainsKey(key))
            {
                continue;
            }

            try
            {
                values[key] = data.GetValue(key);
            }
            catch
            {
                values[key] = null;
            }
        }

        return new IRacingRawTelemetrySnapshot(
            DateTimeOffset.UtcNow,
            data.TickCount,
            data.FramesDropped,
            values);
    }

    public IRacingRawSessionSnapshot? CreateRawSessionSnapshot(IRacingSdkData data)
    {
        if (string.IsNullOrWhiteSpace(data.SessionInfoYaml))
        {
            return null;
        }

        return new IRacingRawSessionSnapshot(
            DateTimeOffset.UtcNow,
            data.SessionInfoUpdate,
            data.SessionInfoYaml,
            null,
            null,
            null,
            null);
    }

    public IRacingNormalizedSnapshot CreateNormalizedSnapshot(IRacingSdk sdk)
    {
        var data = sdk.Data;
        var rawTelemetry = CreateRawTelemetrySnapshot(data);
        var rawSession = CreateRawSessionSnapshot(data);

        var engineWarnings = ReadEnumFlags(data, "EngineWarnings", default(IRacingSdkEnum.EngineWarnings));
        var sessionFlags = ReadEnumFlags(data, "SessionFlags", default(IRacingSdkEnum.Flags));
        var paceFlags = ReadEnumFlags(data, "PaceFlags", default(IRacingSdkEnum.PaceFlags));
        var pitSvFlags = ReadEnumFlags(data, "PitSvFlags", default(IRacingSdkEnum.PitSvFlags));

        var vehicle = new IRacingVehicleState(
            SpeedKph: (int)Math.Round(ReadFloat(data, "Speed") * 3.6f),
            Rpm: (int)Math.Round(ReadFloat(data, "RPM")),
            Gear: ReadInt(data, "Gear"),
            ShiftLightPercent: Math.Clamp(ReadFloat(data, "ShiftIndicatorPct"), 0f, 1f),
            PitLimiterActive: engineWarnings.HasFlag(IRacingSdkEnum.EngineWarnings.PitSpeedLimiter),
            RevLimiterActive: engineWarnings.HasFlag(IRacingSdkEnum.EngineWarnings.RevLimiterActive),
            EngineWarnings: engineWarnings,
            CarLeftRight: ReadEnum(data, "CarLeftRight", IRacingSdkEnum.CarLeftRight.Off));

        var inputs = new IRacingInputState(
            Throttle: Math.Clamp(ReadFloat(data, "Throttle"), 0f, 1f),
            Brake: Math.Clamp(ReadFloat(data, "Brake"), 0f, 1f),
            Clutch: Math.Clamp(ReadFloat(data, "Clutch"), 0f, 1f),
            SteeringWheelAngleDeg: ReadFloat(data, "SteeringWheelAngle") * (180f / MathF.PI));

        var flags = new IRacingFlagState(
            Flags: sessionFlags,
            PaceFlags: paceFlags,
            SessionState: ReadEnum(data, "SessionState", IRacingSdkEnum.SessionState.Invalid),
            PaceMode: ReadEnum(data, "PaceMode", IRacingSdkEnum.PaceMode.NotPacing));

        var trackLocation = ReadEnum(data, "PlayerTrackSurface", IRacingSdkEnum.TrkLoc.NotInWorld);
        var pit = new IRacingPitState(
            OnPitRoad: ReadBool(data, "OnPitRoad"),
            InPitStall: trackLocation == IRacingSdkEnum.TrkLoc.InPitStall,
            ServiceFlags: pitSvFlags,
            ServiceStatus: ReadEnum(data, "PitSvStatus", IRacingSdkEnum.PitSvStatus.None),
            FuelLevelLiters: ReadFloat(data, "FuelLevel"),
            FuelUsePerHour: ReadFloat(data, "FuelUsePerHour"),
            FuelPress: ReadFloat(data, "FuelPress"),
            FuelLevelPct: ReadFloat(data, "FuelLevelPct"));

        var track = new IRacingTrackState(
            TrackLocation: trackLocation,
            TrackSurface: ReadEnum(data, "PlayerTrackSurfaceMaterial", IRacingSdkEnum.TrkSurf.SurfaceNotInWorld),
            TrackWetness: ReadEnum(data, "TrackWetness", IRacingSdkEnum.TrackWetness.Unknown),
            LapDistancePct: ReadFloat(data, "LapDistPct"),
            AirTempC: ReadFloat(data, "AirTemp"),
            TrackTempC: ReadFloat(data, "TrackTemp"));

        var session = new IRacingSessionStateBlock(
            SessionInfoUpdate: data.SessionInfoUpdate,
            SessionName: null,
            SessionType: null,
            TrackDisplayName: null,
            CarScreenName: null,
            SessionTickRate: data.TickRate);

        return new IRacingNormalizedSnapshot(
            DateTimeOffset.UtcNow,
            new IRacingConnectionState(sdk.IsStarted, sdk.IsConnected, data.Status, data.TickRate, data.TickCount, data.FramesDropped),
            vehicle,
            inputs,
            flags,
            pit,
            track,
            session,
            rawTelemetry,
            rawSession);
    }

    private static bool ReadBool(IRacingSdkData data, string name)
        => data.TelemetryDataProperties.TryGetValue(name, out _) && data.GetBool(name);

    private static int ReadInt(IRacingSdkData data, string name)
        => data.TelemetryDataProperties.TryGetValue(name, out _) ? data.GetInt(name) : 0;

    private static float ReadFloat(IRacingSdkData data, string name)
        => data.TelemetryDataProperties.TryGetValue(name, out _) ? data.GetFloat(name) : 0f;

    private static TEnum ReadEnum<TEnum>(IRacingSdkData data, string name, TEnum fallback) where TEnum : struct, Enum
    {
        if (!data.TelemetryDataProperties.TryGetValue(name, out _))
        {
            return fallback;
        }

        try
        {
            var raw = data.GetValue(name);
            return raw switch
            {
                int intValue => (TEnum)Enum.ToObject(typeof(TEnum), intValue),
                uint uintValue => (TEnum)Enum.ToObject(typeof(TEnum), uintValue),
                _ => fallback
            };
        }
        catch
        {
            return fallback;
        }
    }

    private static TEnum ReadEnumFlags<TEnum>(IRacingSdkData data, string name, TEnum fallback) where TEnum : struct, Enum
    {
        if (!data.TelemetryDataProperties.TryGetValue(name, out _))
        {
            return fallback;
        }

        try
        {
            var raw = data.GetValue(name);
            return raw switch
            {
                int intValue => (TEnum)Enum.ToObject(typeof(TEnum), unchecked((uint)intValue)),
                uint uintValue => (TEnum)Enum.ToObject(typeof(TEnum), uintValue),
                _ => fallback
            };
        }
        catch
        {
            return fallback;
        }
    }
}
