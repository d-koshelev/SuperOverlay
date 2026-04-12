using SuperOverlay.Core.Layouts.Layout;

namespace SuperOverlay.Core.Layouts.Editing;

public sealed class LayoutClipboardState
{
    public LayoutDocument? SourceLayout { get; private set; }
    public IReadOnlyList<Guid> ItemIds { get; private set; } = Array.Empty<Guid>();
    public int PasteSequence { get; private set; }
    public bool HasContent => SourceLayout is not null && ItemIds.Count > 0;

    public void Set(LayoutDocument sourceLayout, IEnumerable<Guid> itemIds)
    {
        ArgumentNullException.ThrowIfNull(sourceLayout);
        ArgumentNullException.ThrowIfNull(itemIds);

        SourceLayout = sourceLayout;
        ItemIds = itemIds.OrderBy(x => x).ToList();
        PasteSequence = 0;
    }

    public void Clear()
    {
        SourceLayout = null;
        ItemIds = Array.Empty<Guid>();
        PasteSequence = 0;
    }

    public int AdvancePasteSequence()
    {
        PasteSequence++;
        return PasteSequence;
    }
}
