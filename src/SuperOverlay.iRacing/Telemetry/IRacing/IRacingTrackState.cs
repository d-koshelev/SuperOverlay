using IRSDKSharper;

namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingTrackState(
    IRacingSdkEnum.TrkLoc TrackLocation,
    IRacingSdkEnum.TrkSurf TrackSurface,
    IRacingSdkEnum.TrackWetness TrackWetness,
    float LapDistancePct,
    float AirTempC,
    float TrackTempC);
