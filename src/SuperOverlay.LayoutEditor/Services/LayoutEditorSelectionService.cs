using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorSelectionService
{
    private readonly ObservableCollection<LayoutEditorWidget> _widgets;
    private readonly LayoutEditorState _state;
    private readonly LayoutEditorPropertiesPanelPresenter _propertiesPresenter;
    private readonly Border _propertiesPanel;
    private readonly Func<IReadOnlyList<LayoutEditorWidget>> _selectedWidgetsAccessor;
    private readonly Action _positionPropertiesPanel;

    public LayoutEditorSelectionService(
        ObservableCollection<LayoutEditorWidget> widgets,
        LayoutEditorState state,
        LayoutEditorPropertiesPanelPresenter propertiesPresenter,
        Border propertiesPanel,
        Func<IReadOnlyList<LayoutEditorWidget>> selectedWidgetsAccessor,
        Action positionPropertiesPanel)
    {
        _widgets = widgets;
        _state = state;
        _propertiesPresenter = propertiesPresenter;
        _propertiesPanel = propertiesPanel;
        _selectedWidgetsAccessor = selectedWidgetsAccessor;
        _positionPropertiesPanel = positionPropertiesPanel;
    }

    public IReadOnlyList<LayoutEditorWidget> PrepareWidgetSelection(LayoutEditorWidget widget)
    {
        var keepCurrentGroupSelection = Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && widget.IsSelected;
        var selected = _selectedWidgetsAccessor();

        if (!keepCurrentGroupSelection)
        {
            SelectWidgets([widget], widget);
            selected = [widget];
        }
        else
        {
            _state.PrimarySelectedWidget = widget;
            RefreshSelectionDetails();
            _positionPropertiesPanel();
        }

        return selected;
    }

    public void SelectWidgets(IReadOnlyCollection<LayoutEditorWidget> widgetsToSelect, LayoutEditorWidget? primaryWidget)
    {
        var selectedIds = widgetsToSelect.Select(x => x.Id).ToHashSet();
        foreach (var widget in _widgets)
        {
            widget.IsSelected = selectedIds.Contains(widget.Id);
        }

        _state.PrimarySelectedWidget = primaryWidget;
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
        LayoutEditorDragService.UpdateWidgetDrag(_state, _selectedWidgetsAccessor(), pointer, viewport);
        _positionPropertiesPanel();
    }
}
