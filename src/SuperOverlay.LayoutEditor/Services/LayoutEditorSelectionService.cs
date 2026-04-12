using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorSelectionService
{
    private readonly ObservableCollection<LayoutEditorWidget> _widgets;
    private readonly LayoutEditorState _state;
    private readonly LayoutEditorPropertiesPanelPresenter _propertiesPresenter;
    private readonly Border _propertiesPanel;
    private readonly Func<IReadOnlyList<LayoutEditorWidget>> _selectedWidgetsAccessor;
    private readonly Action _positionPropertiesPanel;
    private readonly LayoutEditorManipulationService _manipulation;
    private readonly LayoutEditorGroupingService _grouping;

    public LayoutEditorSelectionService(
        ObservableCollection<LayoutEditorWidget> widgets,
        LayoutEditorState state,
        LayoutEditorPropertiesPanelPresenter propertiesPresenter,
        Border propertiesPanel,
        Func<IReadOnlyList<LayoutEditorWidget>> selectedWidgetsAccessor,
        Action positionPropertiesPanel,
        LayoutEditorManipulationService manipulation,
        LayoutEditorGroupingService grouping)
    {
        _widgets = widgets;
        _state = state;
        _propertiesPresenter = propertiesPresenter;
        _propertiesPanel = propertiesPanel;
        _selectedWidgetsAccessor = selectedWidgetsAccessor;
        _positionPropertiesPanel = positionPropertiesPanel;
        _manipulation = manipulation;
        _grouping = grouping;
    }

    public IReadOnlyList<LayoutEditorWidget> ToggleWidgetSelection(LayoutEditorWidget widget)
    {
        var selected = _selectedWidgetsAccessor().ToList();
        var groupMembers = _grouping.GetGroupedWidgets(widget).ToList();

        if (widget.IsSelected)
        {
            var idsToRemove = groupMembers.Select(x => x.Id).ToHashSet();
            selected.RemoveAll(x => idsToRemove.Contains(x.Id));
            var primaryWidget = _state.PrimarySelectedWidget is not null && idsToRemove.Contains(_state.PrimarySelectedWidget.Id)
                ? selected.LastOrDefault()
                : _state.PrimarySelectedWidget;
            SelectWidgets(selected, primaryWidget);
            return selected;
        }

        foreach (var member in groupMembers)
        {
            if (selected.All(x => x.Id != member.Id))
            {
                selected.Add(member);
            }
        }

        SelectWidgets(selected, widget);
        return selected;
    }

    public IReadOnlyList<LayoutEditorWidget> EnsureWidgetSelected(LayoutEditorWidget widget)
    {
        var selected = _selectedWidgetsAccessor().ToList();
        var groupMembers = _grouping.GetGroupedWidgets(widget).ToList();
        var changed = false;

        foreach (var member in groupMembers)
        {
            if (selected.All(x => x.Id != member.Id))
            {
                selected.Add(member);
                changed = true;
            }
        }

        if (changed || _state.PrimarySelectedWidget?.Id != widget.Id)
        {
            SelectWidgets(selected, widget);
        }

        return selected;
    }

    public void SelectWidgets(IReadOnlyCollection<LayoutEditorWidget> widgetsToSelect, LayoutEditorWidget? primaryWidget)
    {
        var expandedSelection = _grouping.ExpandSelectionByGroups(widgetsToSelect);
        var selectedIds = expandedSelection.Select(x => x.Id).ToHashSet();

        foreach (var widget in _widgets)
        {
            widget.IsSelected = selectedIds.Contains(widget.Id);
        }

        _state.PrimarySelectedWidget = primaryWidget is not null && selectedIds.Contains(primaryWidget.Id)
            ? primaryWidget
            : expandedSelection.LastOrDefault();

        RefreshSelectionDetails();
        _positionPropertiesPanel();
    }

    public void RefreshSelectionDetails()
    {
        _propertiesPresenter.Refresh(_selectedWidgetsAccessor(), _state.PrimarySelectedWidget);
        _propertiesPanel.Visibility = Visibility.Visible;
    }

    public void UpdateDraggedWidgets(Point pointer, Size viewport)
    {
        _manipulation.UpdateWidgetDrag(_selectedWidgetsAccessor(), pointer, viewport);
        _positionPropertiesPanel();
    }
}
