using SuperOverlay.Dashboards.Runtime;

namespace SuperOverlay.iRacing.Mapping;

public sealed class IRacingMapper
{
    public DashboardRuntimeState Map(int speed, int rpm, int gear)
    {
        return new DashboardRuntimeState(
            new VehicleState(speed, rpm, gear),
            new InputState(0, 0, 0));
    }
}
