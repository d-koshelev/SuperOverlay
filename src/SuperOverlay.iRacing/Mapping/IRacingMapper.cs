using SuperOverlay.Dashboards.Runtime;

namespace SuperOverlay.iRacing.Mapping;

public sealed class IRacingMapper
{
    public DashboardRuntimeState Map(int speed, int rpm, int gear, double shiftLightPercent)
    {
        return new DashboardRuntimeState(
            new VehicleState(speed, rpm, gear, shiftLightPercent),
            new InputState(0, 0, 0));
    }
}
