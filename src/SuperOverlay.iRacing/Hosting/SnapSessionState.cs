namespace SuperOverlay.iRacing.Hosting;

public sealed class SnapSessionState
{
    public AxisSnapState X { get; } = new();
    public AxisSnapState Y { get; } = new();

    public void Reset()
    {
        X.Reset();
        Y.Reset();
    }
}

public sealed class AxisSnapState
{
    public bool IsActive { get; private set; }
    public double Target { get; private set; }

    public void Activate(double target)
    {
        IsActive = true;
        Target = target;
    }

    public void Reset()
    {
        IsActive = false;
        Target = 0;
    }
}
