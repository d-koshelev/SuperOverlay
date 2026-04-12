using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.PanelLayouts;

namespace SuperOverlay.iRacing.Hosting;

internal sealed class OverlaySessionState
{
    public required LayoutDocument Layout { get; set; }
    public PanelLayoutDocument? PanelLayout { get; set; }
    public string? PanelLayoutPath { get; set; }
    public IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> CompiledPanelItemMap { get; set; } = new Dictionary<Guid, IReadOnlyList<Guid>>();
    public bool SnappingEnabled { get; set; } = true;
}
