namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingRawSessionSnapshot(
    DateTimeOffset CapturedAtUtc,
    int SessionInfoUpdate,
    string SessionInfoYaml,
    string? SessionType,
    string? SessionName,
    string? TrackDisplayName,
    string? CarScreenName);
