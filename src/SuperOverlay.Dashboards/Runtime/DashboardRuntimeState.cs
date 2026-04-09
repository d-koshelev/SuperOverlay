namespace SuperOverlay.Dashboards.Runtime;

public sealed record DashboardRuntimeState(
    VehicleState Vehicle,
    InputState Inputs);