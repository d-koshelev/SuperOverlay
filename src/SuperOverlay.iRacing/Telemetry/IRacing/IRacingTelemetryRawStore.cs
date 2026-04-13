namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed class IRacingTelemetryRawStore
{
    private readonly object _gate = new();
    private IRacingTelemetryRawState? _latest;

    public void Set(IRacingTelemetryRawState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        lock (_gate)
        {
            _latest = state;
        }
    }

    public bool TryGetLatest(out IRacingTelemetryRawState state)
    {
        lock (_gate)
        {
            if (_latest is not null)
            {
                state = _latest;
                return true;
            }
        }

        state = new IRacingTelemetryRawState(DateTimeOffset.UtcNow, 0, 0, new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase), Array.Empty<IRacingTelemetryVariableInventoryEntry>());
        return false;
    }

    public void Clear()
    {
        lock (_gate)
        {
            _latest = null;
        }
    }
}
