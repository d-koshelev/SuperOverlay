namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed class IRacingSessionInfoStore
{
    private readonly object _gate = new();
    private IRacingSessionInfoRawState? _latest;

    public void Set(IRacingSessionInfoRawState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        lock (_gate)
        {
            _latest = state;
        }
    }

    public bool TryGetLatest(out IRacingSessionInfoRawState state)
    {
        lock (_gate)
        {
            if (_latest is not null)
            {
                state = _latest;
                return true;
            }
        }

        state = new IRacingSessionInfoRawState(DateTimeOffset.UtcNow, 0, string.Empty, new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase));
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
