namespace SuperOverlay.iRacing.Telemetry.Mock;

public sealed class MockTelemetryProvider
{
    private int _tick;

    public (int speed, int rpm, int gear, double shiftLightPercent) Get()
    {
        _tick++;

        var speed = 100 + (int)(Math.Sin(_tick / 10.0) * 50);
        var rpm = 3000 + (int)(Math.Abs(Math.Sin(_tick / 8.0)) * 5000);
        var gear = (_tick / 40) % 6 + 1;
        var shiftLightPercent = Math.Clamp((rpm - 3200.0) / 4800.0, 0.0, 1.0);

        return (speed, rpm, gear, shiftLightPercent);
    }
}
