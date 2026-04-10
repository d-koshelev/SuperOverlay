namespace SuperOverlay.Dashboards.Runtime;

public sealed record VehicleState(
    int SpeedKph,
    int Rpm,
    int Gear,
    double ShiftLightPercent = 0.0);
