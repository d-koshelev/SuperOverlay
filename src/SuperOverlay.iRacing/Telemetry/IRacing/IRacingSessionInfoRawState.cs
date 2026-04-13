namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingSessionInfoRawState(
    DateTimeOffset CapturedAtUtc,
    int SessionInfoUpdate,
    string SessionInfoYaml,
    IReadOnlyDictionary<string, object?> Fields);
