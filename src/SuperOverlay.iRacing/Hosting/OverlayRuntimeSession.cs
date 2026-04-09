using SuperOverlay.Dashboards.Runtime;
using SuperOverlay.LayoutBuilder.Runtime;

namespace SuperOverlay.iRacing.Hosting;

public sealed class OverlayRuntimeSession
{
    private readonly LayoutHost _layoutHost;

    public OverlayRuntimeSession(LayoutHost layoutHost)
    {
        ArgumentNullException.ThrowIfNull(layoutHost);
        _layoutHost = layoutHost;
    }

    public void Update(DashboardRuntimeState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _layoutHost.Update(state);
    }
}
