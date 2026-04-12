using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SuperOverlay.Core.Layouts.Editing;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorSelectionVisualPresenter
{
    private readonly ObservableCollection<LayoutEditorWidget> _widgets;
    private readonly LayoutEditorState _state;
    private readonly Action _refreshSelectionDetails;

    public LayoutEditorSelectionVisualPresenter(
        ObservableCollection<LayoutEditorWidget> widgets,
        LayoutEditorState state,
        Action refreshSelectionDetails)
    {
        _widgets = widgets;
        _state = state;
        _refreshSelectionDetails = refreshSelectionDetails;
    }

    public void ApplyEngineSnapshot(LayoutEditorEngineSnapshot snapshot)
    {
        var snapshotIds = snapshot.Widgets.Keys.ToHashSet();
        foreach (var removed in _widgets.Where(x => !snapshotIds.Contains(x.Id)).ToList())
        {
            _widgets.Remove(removed);
        }

        var groupMap = new Dictionary<Guid, Guid>();
        foreach (var widget in _widgets)
        {
            if (!snapshot.Widgets.TryGetValue(widget.Id, out var engineWidget))
            {
                continue;
            }

            widget.X = engineWidget.X;
            widget.Y = engineWidget.Y;
            widget.Width = engineWidget.Width;
            widget.Height = engineWidget.Height;
            widget.ZIndex = engineWidget.ZIndex;
            widget.IsLocked = engineWidget.IsLocked;
            widget.IsSelected = snapshot.SelectedIds.Contains(widget.Id);
            widget.IsVisibleInCurrentMode = !_state.IsRaceMode || widget.ShowInRace;

            if (engineWidget.LinkedGroupKey.HasValue)
            {
                if (!groupMap.TryGetValue(engineWidget.LinkedGroupKey.Value, out var localGroupId))
                {
                    localGroupId = engineWidget.LinkedGroupKey.Value;
                    groupMap[engineWidget.LinkedGroupKey.Value] = localGroupId;
                }

                widget.GroupId = localGroupId;
            }
            else
            {
                widget.GroupId = null;
            }
        }

        _state.PrimarySelectedWidget = snapshot.PrimarySelectedId is null
            ? null
            : _widgets.FirstOrDefault(x => x.Id == snapshot.PrimarySelectedId.Value);

        ReorderWidgetsByZIndex();
        _refreshSelectionDetails();
    }

    public void ReorderWidgetsByZIndex()
    {
        var ordered = _widgets.OrderBy(x => x.ZIndex).ThenBy(x => x.Id).ToList();
        for (var targetIndex = 0; targetIndex < ordered.Count; targetIndex++)
        {
            var widget = ordered[targetIndex];
            var currentIndex = _widgets.IndexOf(widget);
            if (currentIndex >= 0 && currentIndex != targetIndex)
            {
                _widgets.Move(currentIndex, targetIndex);
            }
        }
    }

    public static IReadOnlyList<LayoutEditorEngineWidgetInput> BuildEngineInputs(IEnumerable<LayoutEditorWidget> widgets)
    {
        return widgets
            .Select(widget => new LayoutEditorEngineWidgetInput(
                widget.Id,
                widget.X,
                widget.Y,
                widget.Width,
                widget.Height,
                widget.ZIndex,
                widget.IsLocked,
                widget.GroupId))
            .ToList();
    }
}
