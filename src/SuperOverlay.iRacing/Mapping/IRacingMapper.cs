using SuperOverlay.Dashboards.Runtime;
using SuperOverlay.iRacing.Telemetry.IRacing;

namespace SuperOverlay.iRacing.Mapping;

public sealed class IRacingMapper
{
    public DashboardRuntimeState Map(int speed, int rpm, int gear, double shiftLightPercent)
    {
        return new DashboardRuntimeState(
            new VehicleState(speed, rpm, gear, shiftLightPercent),
            new InputState(0, 0, 0));
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
                snapshot.Inputs.Clutch));
    }
}
