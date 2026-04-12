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
        var viewport = new Size(OverlayChromeLayer.ActualWidth, OverlayChromeLayer.ActualHeight);
        var panelSize = new Size(FloatingMenu.ActualWidth, FloatingMenu.ActualHeight);
        var otherRect = new Rect(
            Canvas.GetLeft(PropertiesPanel),
            Canvas.GetTop(PropertiesPanel),
            PropertiesPanel.Width,
            PropertiesPanel.ActualHeight);

        var point = LayoutEditorFloatingPanelSnapService.ResolveSnappedPosition(
            new Point(left, top),
            viewport,
            panelSize,
            otherRect,
            LayoutEditorUiConstants.ChromePadding,
            _snapPolicy.IsPanelSnapEnabled());

        _state.HasManualToolbarPosition = true;
        Canvas.SetLeft(FloatingMenu, point.X);
        Canvas.SetTop(FloatingMenu, point.Y);
    }

    private void MovePropertiesPanel(double left, double top)
    {
        _propertiesChrome.Move(left, top);
    }

    private void ClampFloatingMenuToViewport()
    {
        var point = LayoutEditorFloatingPanelLayoutService.ClampPanelPosition(
            Canvas.GetLeft(FloatingMenu),
            Canvas.GetTop(FloatingMenu),
            new Size(OverlayChromeLayer.ActualWidth, OverlayChromeLayer.ActualHeight),
            new Size(FloatingMenu.ActualWidth, FloatingMenu.ActualHeight),
            LayoutEditorUiConstants.ChromePadding);

        Canvas.SetLeft(FloatingMenu, point.X);
        Canvas.SetTop(FloatingMenu, point.Y);
    }

    private void PositionFloatingMenu()
    {
        if (_state.HasManualToolbarPosition)
        {
            ClampFloatingMenuToViewport();
            return;
        }

        var point = LayoutEditorFloatingPanelLayoutService.ResolveToolbarStartPosition(
            new Size(OverlayChromeLayer.ActualWidth, OverlayChromeLayer.ActualHeight),
            new Size(FloatingMenu.ActualWidth, FloatingMenu.ActualHeight),
            LayoutEditorUiConstants.ChromePadding);

        Canvas.SetLeft(FloatingMenu, point.X);
        Canvas.SetTop(FloatingMenu, point.Y);
    }

    private void PositionPropertiesPanel()
    {
        _propertiesChrome.Position();
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
