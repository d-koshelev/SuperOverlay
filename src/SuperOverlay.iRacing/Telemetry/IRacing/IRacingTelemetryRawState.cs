namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingTelemetryRawState(
    DateTimeOffset CapturedAtUtc,
    int TickCount,
    int FramesDropped,
    IReadOnlyDictionary<string, object?> Fields,
    IReadOnlyList<IRacingTelemetryVariableInventoryEntry> Catalog);
