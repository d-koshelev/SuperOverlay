using System.Text.Json.Serialization;

namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingTelemetryVariableInventoryEntry(
    string Name,
    string Description,
    string Unit,
    string VarType,
    int Offset,
    int Count,
    bool CountAsTime,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? SampleValue);
