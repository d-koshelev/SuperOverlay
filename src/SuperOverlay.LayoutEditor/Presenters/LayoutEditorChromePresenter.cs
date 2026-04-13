using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorChromePresenter
{
    private readonly LayoutEditorState _state;
    private readonly ObservableCollection<LayoutEditorWidget> _widgets;
    private readonly Border _floatingMenu;
    private readonly Border _propertiesPanel;
    private readonly Border _placementHintPanel;
    private readonly Rectangle _selectionRectangle;
    private readonly ItemsControl _widgetsItemsControl;
    private readonly ItemsControl _presetPreviewItemsControl;
    private readonly Button _snapToggleButton;
    private readonly Button _guidesToggleButton;
    private readonly Action _hideGuides;
    private readonly Action _refreshGridOverlay;
    private readonly Action _clearSelection;
    private readonly Action _refreshSelectionDetails;
    private readonly Action _positionPropertiesPanel;
    private readonly Func<int> _selectedWidgetCountAccessor;
    private readonly Action<string> _setWindowTitle;

    public LayoutEditorChromePresenter(
        LayoutEditorState state,
        ObservableCollection<LayoutEditorWidget> widgets,
        Border floatingMenu,
        Border propertiesPanel,
        Border placementHintPanel,
        Rectangle selectionRectangle,
        ItemsControl widgetsItemsControl,
        ItemsControl presetPreviewItemsControl,
        Button snapToggleButton,
        Button guidesToggleButton,
        Action hideGuides,
        Action refreshGridOverlay,
        Action clearSelection,
        Action refreshSelectionDetails,
        Action positionPropertiesPanel,
        Func<int> selectedWidgetCountAccessor,
        Action<string> setWindowTitle)
    {
        _state = state;
        _widgets = widgets;
        _floatingMenu = floatingMenu;
        _propertiesPanel = propertiesPanel;
        _placementHintPanel = placementHintPanel;
        _selectionRectangle = selectionRectangle;
        _widgetsItemsControl = widgetsItemsControl;
        _presetPreviewItemsControl = presetPreviewItemsControl;
        _snapToggleButton = snapToggleButton;
        _guidesToggleButton = guidesToggleButton;
        _hideGuides = hideGuides;
        _refreshGridOverlay = refreshGridOverlay;
        _clearSelection = clearSelection;
        _refreshSelectionDetails = refreshSelectionDetails;
        _positionPropertiesPanel = positionPropertiesPanel;
        _selectedWidgetCountAccessor = selectedWidgetCountAccessor;
        _setWindowTitle = setWindowTitle;
    }

    public void SetRaceMode(bool isRaceMode)
    {
        _state.IsRaceMode = isRaceMode;

        _floatingMenu.Visibility = isRaceMode ? Visibility.Collapsed : Visibility.Visible;
        _propertiesPanel.Visibility = isRaceMode ? Visibility.Collapsed : (_selectedWidgetCountAccessor() > 0 ? Visibility.Visible : Visibility.Collapsed);
        _placementHintPanel.Visibility = isRaceMode ? Visibility.Collapsed : _placementHintPanel.Visibility;
        _selectionRectangle.Visibility = isRaceMode ? Visibility.Collapsed : (_state.IsMarqueeSelecting ? Visibility.Visible : Visibility.Collapsed);
        if (isRaceMode)
        {
            _hideGuides();
        }

        _refreshGridOverlay();
        _widgetsItemsControl.IsHitTestVisible = !isRaceMode;
        _presetPreviewItemsControl.IsHitTestVisible = false;

        foreach (var widget in _widgets)
        {
            widget.IsVisibleInCurrentMode = !isRaceMode || widget.ShowInRace;
        }

        _setWindowTitle(isRaceMode
            ? "SuperOverlay LayoutEditor - RACE"
            : string.IsNullOrWhiteSpace(_state.CurrentLayoutName)
                ? "SuperOverlay LayoutEditor"
                : $"SuperOverlay LayoutEditor - {_state.CurrentLayoutName}");

        if (isRaceMode)
        {
            _clearSelection();
        }
        else
        {
            _refreshSelectionDetails();
            _positionPropertiesPanel();
        }
    }

    public void RefreshChromeToggleText()
    {
        _snapToggleButton.Content = _state.IsSnappingEnabled ? "Snap On" : "Snap Off";
        _guidesToggleButton.Content = _state.AreGuidesEnabled ? "Guides On" : "Guides Off";
    }
}
