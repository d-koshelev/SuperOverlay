using System.Windows;
using System.Windows.Controls;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorPropertiesPanelChromePresenter
{
    private readonly LayoutEditorState _state;
    private readonly Canvas _viewport;
    private readonly Border _propertiesPanel;
    private readonly FrameworkElement _toolbar;
    private readonly LayoutEditorSnapPolicyService _snapPolicy;

    public LayoutEditorPropertiesPanelChromePresenter(
        LayoutEditorState state,
        Canvas viewport,
        Border propertiesPanel,
        FrameworkElement toolbar,
        LayoutEditorSnapPolicyService snapPolicy)
    {
        _state = state;
        _viewport = viewport;
        _propertiesPanel = propertiesPanel;
        _toolbar = toolbar;
        _snapPolicy = snapPolicy;
    }

    public void Move(double left, double top)
    {
        var viewport = new Size(_viewport.ActualWidth, _viewport.ActualHeight);
        var panelSize = new Size(_propertiesPanel.Width, _propertiesPanel.ActualHeight);
        var otherRect = new Rect(
            Canvas.GetLeft(_toolbar),
            Canvas.GetTop(_toolbar),
            _toolbar.ActualWidth,
            _toolbar.ActualHeight);

        var point = LayoutEditorFloatingPanelSnapService.ResolveSnappedPosition(
            new Point(left, top),
            viewport,
            panelSize,
            otherRect,
            LayoutEditorUiConstants.ChromePadding,
            _snapPolicy.IsPanelSnapEnabled());

        _state.HasManualPropertiesPosition = true;
        Canvas.SetLeft(_propertiesPanel, point.X);
        Canvas.SetTop(_propertiesPanel, point.Y);
    }

    public void Position()
    {
        if (_state.HasManualPropertiesPosition)
        {
            var clamped = LayoutEditorFloatingPanelLayoutService.ClampPanelPosition(
                Canvas.GetLeft(_propertiesPanel),
                Canvas.GetTop(_propertiesPanel),
                new Size(_viewport.ActualWidth, _viewport.ActualHeight),
                new Size(_propertiesPanel.Width, _propertiesPanel.ActualHeight),
                LayoutEditorUiConstants.ChromePadding);

            Canvas.SetLeft(_propertiesPanel, clamped.X);
            Canvas.SetTop(_propertiesPanel, clamped.Y);
            return;
        }

        var point = LayoutEditorFloatingPanelLayoutService.ResolvePropertiesPanelStartPosition(
            new Size(_viewport.ActualWidth, _viewport.ActualHeight),
            new Size(_propertiesPanel.Width, _propertiesPanel.ActualHeight),
            LayoutEditorUiConstants.ChromePadding,
            LayoutEditorUiConstants.PropertiesPanelDefaultTop);

        Canvas.SetLeft(_propertiesPanel, point.X);
        Canvas.SetTop(_propertiesPanel, point.Y);
    }
}
