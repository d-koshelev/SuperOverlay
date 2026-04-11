using IRSDKSharper;

namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingPitState(
    bool OnPitRoad,
    bool InPitStall,
    IRacingSdkEnum.PitSvFlags ServiceFlags,
    IRacingSdkEnum.PitSvStatus ServiceStatus,
    float FuelLevelLiters,
    float FuelUsePerHour,
    float FuelPress,
    float FuelLevelPct);
