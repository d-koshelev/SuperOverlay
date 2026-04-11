namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingRawTelemetrySnapshot(
    DateTimeOffset CapturedAtUtc,
    int TickCount,
    int FramesDropped,
    IReadOnlyDictionary<string, object?> Values);
