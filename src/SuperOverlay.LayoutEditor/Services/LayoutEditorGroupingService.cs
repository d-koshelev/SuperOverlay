using System.Collections.ObjectModel;
using System.Linq;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorGroupingService
{
    private readonly ObservableCollection<LayoutEditorWidget> _widgets;

    public LayoutEditorGroupingService(ObservableCollection<LayoutEditorWidget> widgets)
    {
        _widgets = widgets;
    }

    public IReadOnlyList<LayoutEditorWidget> ExpandSelectionByGroups(IEnumerable<LayoutEditorWidget> widgets)
    {
        var expanded = new List<LayoutEditorWidget>();
        var seen = new HashSet<Guid>();

        foreach (var widget in widgets)
        {
            foreach (var member in GetGroupedWidgets(widget))
            {
                if (seen.Add(member.Id))
                {
                    expanded.Add(member);
                }
            }
        }

        return expanded;
    }

    public IReadOnlyList<LayoutEditorWidget> GetGroupedWidgets(LayoutEditorWidget widget)
    {
        if (!widget.GroupId.HasValue)
        {
            return [widget];
        }

        return _widgets
            .Where(x => x.GroupId == widget.GroupId)
            .OrderBy(x => x.ZIndex)
            .ThenBy(x => x.Id)
            .ToList();
    }
}
