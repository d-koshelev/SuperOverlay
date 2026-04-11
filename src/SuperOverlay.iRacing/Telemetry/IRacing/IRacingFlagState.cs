using IRSDKSharper;

namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingFlagState(
    IRacingSdkEnum.Flags Flags,
    IRacingSdkEnum.PaceFlags PaceFlags,
    IRacingSdkEnum.SessionState SessionState,
    IRacingSdkEnum.PaceMode PaceMode);
