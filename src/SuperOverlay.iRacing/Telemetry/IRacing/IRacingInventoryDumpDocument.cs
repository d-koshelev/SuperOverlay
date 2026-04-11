namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingInventoryDumpDocument(
    DateTimeOffset CapturedAtUtc,
    bool Connected,
    int Version,
    int Status,
    int TickRate,
    int SessionInfoUpdate,
    int SessionInfoLength,
    int SessionInfoOffset,
    int VarCount,
    int VarHeaderOffset,
    int BufferCount,
    int BufferLength,
    int TickCount,
    int Offset,
    int FramesDropped,
    IReadOnlyList<IRacingTelemetryVariableInventoryEntry> TelemetryVariables,
    string SessionInfoYamlPath);
