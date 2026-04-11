namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingSessionStateBlock(
    int SessionInfoUpdate,
    string? SessionName,
    string? SessionType,
    string? TrackDisplayName,
    string? CarScreenName,
    int SessionTickRate);
