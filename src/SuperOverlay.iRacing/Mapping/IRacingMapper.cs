using SuperOverlay.Dashboards.Runtime;
using SuperOverlay.iRacing.Telemetry.IRacing;

namespace SuperOverlay.iRacing.Mapping;

public sealed class IRacingMapper
{
    public DashboardRuntimeState Map(int speed, int rpm, int gear, double shiftLightPercent)
    {
        return new DashboardRuntimeState(
            new VehicleState(speed, rpm, gear, shiftLightPercent),
            new InputState(0, 0, 0),
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Speed"] = speed,
                ["RPM"] = rpm,
                ["Gear"] = gear,
                ["ShiftIndicatorPct"] = shiftLightPercent
            },
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase));
    }

    public DashboardRuntimeState Map(IRacingNormalizedSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new DashboardRuntimeState(
            new VehicleState(
                snapshot.Vehicle.SpeedKph,
                snapshot.Vehicle.Rpm,
                snapshot.Vehicle.Gear,
                snapshot.Vehicle.ShiftLightPercent),
            new InputState(
                snapshot.Inputs.Throttle,
                snapshot.Inputs.Brake,
                snapshot.Inputs.Clutch),
            new Dictionary<string, object?>(snapshot.RawTelemetry.Values, StringComparer.OrdinalIgnoreCase),
            snapshot.RawSession is null
                ? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, object?>(snapshot.RawSession.Fields, StringComparer.OrdinalIgnoreCase));
    }
}
