using System.Windows;
using System.Windows.Controls;
using SuperOverlay.Core.Layouts.Editing;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    private void RefreshGridOverlay()
    {
        var visibility = _state.AreGuidesEnabled && !_state.IsRaceMode
            ? Visibility.Visible
            : Visibility.Collapsed;

        var width = OverlayChromeLayer.ActualWidth > 0 ? OverlayChromeLayer.ActualWidth : RootGrid.ActualWidth;
        var height = OverlayChromeLayer.ActualHeight > 0 ? OverlayChromeLayer.ActualHeight : RootGrid.ActualHeight;
        if (width <= 0)
        {
            width = ActualWidth;
        }

        if (height <= 0)
        {
            height = ActualHeight;
        }

        GridOverlayBackdrop.Width = width;
        GridOverlayBackdrop.Height = height;
        Canvas.SetLeft(GridOverlayBackdrop, 0);
        Canvas.SetTop(GridOverlayBackdrop, 0);

        CenterVerticalGridLine.Width = 1;
        CenterVerticalGridLine.Height = height;
        Canvas.SetLeft(CenterVerticalGridLine, System.Math.Max(0, (width / 2d) - 0.5d));
        Canvas.SetTop(CenterVerticalGridLine, 0);

        CenterHorizontalGridLine.Width = width;
        CenterHorizontalGridLine.Height = 1;
        Canvas.SetLeft(CenterHorizontalGridLine, 0);
        Canvas.SetTop(CenterHorizontalGridLine, System.Math.Max(0, (height / 2d) - 0.5d));

        GridOverlayBackdrop.Visibility = visibility;
        CenterVerticalGridLine.Visibility = visibility;
        CenterHorizontalGridLine.Visibility = visibility;
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
}
