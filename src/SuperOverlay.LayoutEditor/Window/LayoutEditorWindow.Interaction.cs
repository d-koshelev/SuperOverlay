using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    private void RootGrid_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_interaction.HandleRootLeftButtonDown(e))
        {
            e.Handled = true;
        }
    }

    private void RootGrid_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _interaction.HandleRootRightButtonDown(e);
    }

    private void RootGrid_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_interaction.HandleRootMouseMove(e))
        {
            e.Handled = true;
        }
    }

    private void RootGrid_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_interaction.HandleRootLeftButtonUp(e))
        {
            e.Handled = true;
        }
    }

    private void WidgetBorder_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_interaction.HandleWidgetLeftButtonDown(sender, e))
        {
            e.Handled = true;
        }
    }

    private void WidgetBorder_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_interaction.HandleWidgetMouseMove(e))
        {
            e.Handled = true;
        }
    }

    private void WidgetBorder_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_interaction.HandleWidgetLeftButtonUp())
        {
            e.Handled = true;
        }
    }

    private void WidgetBorder_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _interaction.HandleWidgetRightButtonDown(sender);
        e.Handled = false;
    }

    private void WidgetContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        _interaction.HandleWidgetContextMenuOpened(sender);
    }

    private void PropertiesMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        PositionPropertiesPanel();
    }

    private void FloatingMenu_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _interaction.BeginToolbarDrag(e.GetPosition(FloatingMenu));
        e.Handled = true;
    }

    private void FloatingMenu_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_interaction.HandleRootMouseMove(e))
        {
            e.Handled = true;
        }
    }

    private void FloatingMenu_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_interaction.EndToolbarDrag())
        {
            e.Handled = true;
        }
    }

    private void PropertiesPanel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _interaction.BeginPropertiesDrag(e.GetPosition(PropertiesPanel));
        e.Handled = true;
    }

    private void PropertiesPanel_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_interaction.HandleRootMouseMove(e))
        {
            e.Handled = true;
        }
    }

    private void PropertiesPanel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_interaction.EndPropertiesDrag())
        {
            e.Handled = true;
        }
    }

    private void MoveFloatingMenu(double left, double top)
    {
        var point = LayoutEditorFloatingPanelLayoutService.ClampPanelPosition(
            left,
            top,
            new Size(OverlayChromeLayer.ActualWidth, OverlayChromeLayer.ActualHeight),
            new Size(FloatingMenu.ActualWidth, FloatingMenu.ActualHeight));

        Canvas.SetLeft(FloatingMenu, point.X);
        Canvas.SetTop(FloatingMenu, point.Y);
    }

    private void MovePropertiesPanel(double left, double top)
    {
        var point = LayoutEditorFloatingPanelLayoutService.ClampPanelPosition(
            left,
            top,
            new Size(OverlayChromeLayer.ActualWidth, OverlayChromeLayer.ActualHeight),
            new Size(PropertiesPanel.Width, PropertiesPanel.ActualHeight));

        Canvas.SetLeft(PropertiesPanel, point.X);
        Canvas.SetTop(PropertiesPanel, point.Y);
    }

    private void ClampFloatingMenuToViewport()
    {
        MoveFloatingMenu(Canvas.GetLeft(FloatingMenu), Canvas.GetTop(FloatingMenu));
    }

    private void PositionPropertiesPanel()
    {
        var anchor = SelectedWidgets.Count == 0 ? null : _state.PrimarySelectedWidget ?? SelectedWidgets[0];
        var point = LayoutEditorFloatingPanelLayoutService.ResolvePropertiesPanelPosition(
            anchor,
            new Size(OverlayChromeLayer.ActualWidth, OverlayChromeLayer.ActualHeight),
            new Size(PropertiesPanel.Width, PropertiesPanel.ActualHeight),
            LayoutEditorUiConstants.PropertiesGap,
            LayoutEditorUiConstants.ChromePadding,
            LayoutEditorUiConstants.PropertiesPanelDefaultTop);

        Canvas.SetLeft(PropertiesPanel, point.X);
        Canvas.SetTop(PropertiesPanel, point.Y);
    }

    private void BeginMarqueeSelection(Point start)
    {
        _marquee.Begin(start);
    }

    private void UpdateMarqueeSelection(Point current)
    {
        _marquee.Update(current);
    }

    private void EndMarqueeSelection()
    {
        _marquee.End();
    }
}
