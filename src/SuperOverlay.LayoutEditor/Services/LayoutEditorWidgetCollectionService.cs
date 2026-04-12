using System.Collections.ObjectModel;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorWidgetCollectionService
{
    public static IReadOnlyList<LayoutEditorWidget> Duplicate(
        ObservableCollection<LayoutEditorWidget> widgets,
        IReadOnlyList<LayoutEditorWidget> selected,
        double canvasWidth,
        double canvasHeight,
        double offsetX,
        double offsetY)
    {
        var created = new List<LayoutEditorWidget>();
        var groupMap = new Dictionary<Guid, Guid>();

        foreach (var sourceWidget in selected)
        {
            Guid? duplicatedGroupId = null;
            if (sourceWidget.GroupId.HasValue)
            {
                if (!groupMap.TryGetValue(sourceWidget.GroupId.Value, out var newGroupId))
                {
                    newGroupId = Guid.NewGuid();
                    groupMap[sourceWidget.GroupId.Value] = newGroupId;
                }

                duplicatedGroupId = newGroupId;
            }

            var duplicated = LayoutEditorWidgetFactory.CreateCopy(
                sourceWidget,
                LayoutEditorMath.Clamp(sourceWidget.X + offsetX, 0, Math.Max(0, canvasWidth - sourceWidget.Width)),
                LayoutEditorMath.Clamp(sourceWidget.Y + offsetY, 0, Math.Max(0, canvasHeight - sourceWidget.Height)),
                duplicatedGroupId);

            widgets.Add(duplicated);
            created.Add(duplicated);
        }

        return created;
    }

    public static bool Group(IReadOnlyList<LayoutEditorWidget> selected)
    {
        if (selected.Count < 2)
        {
            return false;
        }

        var groupId = Guid.NewGuid();
        foreach (var widget in selected)
        {
            widget.GroupId = groupId;
        }

        return true;
    }

    public static void Ungroup(IReadOnlyList<LayoutEditorWidget> selected)
    {
        foreach (var widget in selected)
        {
            widget.GroupId = null;
        }
    }

    public static void Delete(ObservableCollection<LayoutEditorWidget> widgets, IReadOnlyList<LayoutEditorWidget> selected)
    {
        foreach (var widget in selected.ToList())
        {
            widgets.Remove(widget);
        }
    }

    public static bool ToggleShowInRace(IReadOnlyList<LayoutEditorWidget> selected)
    {
        if (selected.Count == 0)
        {
            return false;
        }

        var makeVisible = selected.Any(x => !x.ShowInRace);
        foreach (var widget in selected)
        {
            widget.ShowInRace = makeVisible;
        }

        return true;
    }

    public static void ApplyShowInRace(IReadOnlyList<LayoutEditorWidget> selected, bool showInRace)
    {
        foreach (var widget in selected)
        {
            widget.ShowInRace = showInRace;
        }
    }
}
