using IRSDKSharper;

namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingVehicleState(
    int SpeedKph,
    int Rpm,
    int Gear,
    double ShiftLightPercent,
    bool PitLimiterActive,
    bool RevLimiterActive,
    IRacingSdkEnum.EngineWarnings EngineWarnings,
    IRacingSdkEnum.CarLeftRight CarLeftRight);
