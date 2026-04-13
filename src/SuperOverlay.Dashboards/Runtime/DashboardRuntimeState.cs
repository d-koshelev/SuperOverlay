namespace SuperOverlay.Dashboards.Runtime;

public sealed record DashboardRuntimeState
{
    public DashboardRuntimeState(
        VehicleState vehicle,
        InputState inputs,
        IReadOnlyDictionary<string, object?>? telemetryRaw = null,
        IReadOnlyDictionary<string, object?>? sessionInfo = null)
    {
        Vehicle = vehicle;
        Inputs = inputs;
        TelemetryRaw = telemetryRaw ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        SessionInfo = sessionInfo ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    public VehicleState Vehicle { get; init; }
    public InputState Inputs { get; init; }
    public IReadOnlyDictionary<string, object?> TelemetryRaw { get; init; }
    public IReadOnlyDictionary<string, object?> SessionInfo { get; init; }
}
