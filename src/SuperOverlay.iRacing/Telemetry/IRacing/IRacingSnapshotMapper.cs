using IRSDKSharper;

namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed class IRacingSnapshotMapper
{
    public IRacingTelemetryRawState CreateTelemetryRawState(IRacingSdkData data)
    {
        var catalog = data.TelemetryDataProperties
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => new IRacingTelemetryVariableInventoryEntry(
                x.Value.Name ?? x.Key,
                x.Value.Desc ?? string.Empty,
                x.Value.Unit ?? string.Empty,
                x.Value.VarType.ToString(),
                x.Value.Offset,
                x.Value.Count,
                x.Value.CountAsTime,
                TryFormatSampleValue(data, x.Key, x.Value)))
            .ToArray();

        return new IRacingTelemetryRawState(
            DateTimeOffset.UtcNow,
            data.TickCount,
            data.FramesDropped,
            CreateTelemetryValues(data),
            catalog);
    }

    public IRacingSessionInfoRawState? CreateSessionInfoRawState(IRacingSdkData data)
    {
        if (string.IsNullOrWhiteSpace(data.SessionInfoYaml))
        {
            return null;
        }

        return new IRacingSessionInfoRawState(
            DateTimeOffset.UtcNow,
            data.SessionInfoUpdate,
            data.SessionInfoYaml,
            CreateSessionInfoValues(data));
    }

    public IRacingRawTelemetrySnapshot CreateRawTelemetrySnapshot(IRacingTelemetryRawState telemetryState)
        => new(telemetryState.CapturedAtUtc, telemetryState.TickCount, telemetryState.FramesDropped, telemetryState.Fields);

    public IRacingRawSessionSnapshot? CreateRawSessionSnapshot(IRacingSessionInfoRawState? sessionState)
    {
        if (sessionState is null)
        {
            return null;
        }

        sessionState.Fields.TryGetValue("SessionInfo.Sessions[0].SessionType", out var sessionType);
        sessionState.Fields.TryGetValue("SessionInfo.Sessions[0].SessionName", out var sessionName);
        sessionState.Fields.TryGetValue("WeekendInfo.TrackDisplayName", out var trackDisplayName);
        sessionState.Fields.TryGetValue("DriverInfo.Drivers[0].CarScreenName", out var carScreenName);

        return new IRacingRawSessionSnapshot(
            sessionState.CapturedAtUtc,
            sessionState.SessionInfoUpdate,
            sessionState.SessionInfoYaml,
            sessionState.Fields,
            sessionType?.ToString(),
            sessionName?.ToString(),
            trackDisplayName?.ToString(),
            carScreenName?.ToString());
    }

    public IRacingNormalizedSnapshot CreateNormalizedSnapshot(IRacingSdk sdk)
    {
        var data = sdk.Data;
        var telemetryState = CreateTelemetryRawState(data);
        var sessionState = CreateSessionInfoRawState(data);
        var rawTelemetry = CreateRawTelemetrySnapshot(telemetryState);
        var rawSession = CreateRawSessionSnapshot(sessionState);

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
            SessionName: rawSession?.SessionName,
            SessionType: rawSession?.SessionType,
            TrackDisplayName: rawSession?.TrackDisplayName,
            CarScreenName: rawSession?.CarScreenName,
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

    private static Dictionary<string, object?> CreateTelemetryValues(IRacingSdkData data)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in data.TelemetryDataProperties)
        {
            try
            {
                values[pair.Key] = ReadTelemetryValue(data, pair.Value);
            }
            catch
            {
                values[pair.Key] = null;
            }
        }

        return values;
    }

    private static Dictionary<string, object?> CreateSessionInfoValues(IRacingSdkData data)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var list = data.sessionInfoAsList;
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] is IRacingSdkSessionInfoAsList.Datum datum && !string.IsNullOrWhiteSpace(datum.key))
            {
                values[datum.key] = datum.value;
            }
        }

        return values;
    }

    private static object? ReadTelemetryValue(IRacingSdkData data, IRacingSdkDatum datum)
    {
        if (datum.Count <= 1)
        {
            return data.GetValue(datum);
        }

        return datum.VarType switch
        {
            IRacingSdkEnum.VarType.Char => ReadCharArray(data, datum),
            IRacingSdkEnum.VarType.Bool => ReadBoolArray(data, datum),
            IRacingSdkEnum.VarType.Int => ReadIntArray(data, datum),
            IRacingSdkEnum.VarType.BitField => ReadBitFieldArray(data, datum),
            IRacingSdkEnum.VarType.Float => ReadFloatArray(data, datum),
            IRacingSdkEnum.VarType.Double => ReadDoubleArray(data, datum),
            _ => null
        };
    }

    private static string? TryFormatSampleValue(IRacingSdkData data, string key, IRacingSdkDatum datum)
    {
        try
        {
            var value = datum.Count <= 1 ? data.GetValue(key) : ReadTelemetryValue(data, datum);
            return value switch
            {
                null => null,
                System.Collections.IEnumerable sequence when value is not string => string.Join(", ", sequence.Cast<object?>().Take(4).Select(x => x?.ToString() ?? string.Empty)),
                _ => value.ToString()
            };
        }
        catch
        {
            return null;
        }
    }

    private static string ReadCharArray(IRacingSdkData data, IRacingSdkDatum datum)
    {
        var array = new char[datum.Count];
        data.GetCharArray(datum.Name, array, 0, array.Length);
        return new string(array).TrimEnd(' ');
    }

    private static bool[] ReadBoolArray(IRacingSdkData data, IRacingSdkDatum datum)
    {
        var array = new bool[datum.Count];
        data.GetBoolArray(datum, array, 0, array.Length);
        return array;
    }

    private static int[] ReadIntArray(IRacingSdkData data, IRacingSdkDatum datum)
    {
        var array = new int[datum.Count];
        data.GetIntArray(datum, array, 0, array.Length);
        return array;
    }

    private static uint[] ReadBitFieldArray(IRacingSdkData data, IRacingSdkDatum datum)
    {
        var array = new uint[datum.Count];
        data.GetBitFieldArray(datum, array, 0, array.Length);
        return array;
    }

    private static float[] ReadFloatArray(IRacingSdkData data, IRacingSdkDatum datum)
    {
        var array = new float[datum.Count];
        data.GetFloatArray(datum, array, 0, array.Length);
        return array;
    }

    private static double[] ReadDoubleArray(IRacingSdkData data, IRacingSdkDatum datum)
    {
        var array = new double[datum.Count];
        data.GetDoubleArray(datum, array, 0, array.Length);
        return array;
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
