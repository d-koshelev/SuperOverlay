namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingInputState(
    float Throttle,
    float Brake,
    float Clutch,
    float SteeringWheelAngleDeg = 0f);
