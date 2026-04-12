using System.Windows;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    private static double Clamp(double value, double min, double max) => LayoutEditorMath.Clamp(value, min, max);

    internal void SetRaceMode(bool isRaceMode)
    {
        _state.IsRaceMode = isRaceMode;

        FloatingMenu.Visibility = isRaceMode ? Visibility.Collapsed : Visibility.Visible;
        PropertiesPanel.Visibility = isRaceMode ? Visibility.Collapsed : (SelectedWidgets.Count > 0 ? Visibility.Visible : Visibility.Collapsed);
        PlacementHintPanel.Visibility = isRaceMode ? Visibility.Collapsed : PlacementHintPanel.Visibility;
        SelectionRectangle.Visibility = isRaceMode ? Visibility.Collapsed : (_state.IsMarqueeSelecting ? Visibility.Visible : Visibility.Collapsed);
        if (isRaceMode)
        {
            HideGuides();
        }
        WidgetsItemsControl.IsHitTestVisible = !isRaceMode;
        PresetPreviewItemsControl.IsHitTestVisible = false;

        foreach (var widget in Widgets)
        {
            widget.IsVisibleInCurrentMode = !isRaceMode || widget.ShowInRace;
        }

        Title = isRaceMode
            ? "SuperOverlay LayoutEditor - RACE"
            : string.IsNullOrWhiteSpace(_state.CurrentLayoutName)
                ? "SuperOverlay LayoutEditor"
                : $"SuperOverlay LayoutEditor - {_state.CurrentLayoutName}";

        if (isRaceMode)
        {
            SelectWidgets([], null);
        }
        else
        {
            RefreshSelectionDetails();
            PositionPropertiesPanel();
        }
    }


    private LayoutEditorEngineMoveResult UpdateGuides(LayoutEditorEngineMoveResult result)
    {
        if (!_state.AreGuidesEnabled || _state.IsRaceMode)
        {
            HideGuides();
            return result;
        }

        if (result.SnapX is double snapX)
        {
            VerticalGuideLine.Margin = new Thickness(snapX, 0, 0, 0);
            VerticalGuideLine.Visibility = Visibility.Visible;
        }
        else
        {
            VerticalGuideLine.Visibility = Visibility.Collapsed;
        }

        if (result.SnapY is double snapY)
        {
            HorizontalGuideLine.Margin = new Thickness(0, snapY, 0, 0);
            HorizontalGuideLine.Visibility = Visibility.Visible;
        }
        else
        {
            HorizontalGuideLine.Visibility = Visibility.Collapsed;
        }

        return result;
    }

    private void HideGuides()
    {
        VerticalGuideLine.Visibility = Visibility.Collapsed;
        HorizontalGuideLine.Visibility = Visibility.Collapsed;
    }

    private void RefreshChromeToggleText()
    {
        SnapToggleButton.Content = _state.IsSnappingEnabled ? "Snap On" : "Snap Off";
        GuidesToggleButton.Content = _state.AreGuidesEnabled ? "Guides On" : "Guides Off";
    }
}