namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingConnectionState(
    bool IsStarted,
    bool IsConnected,
    int Status,
    int TickRate,
    int TickCount,
    int FramesDropped);
