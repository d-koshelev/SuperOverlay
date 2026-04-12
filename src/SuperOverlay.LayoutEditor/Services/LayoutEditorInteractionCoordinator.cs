using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorInteractionCoordinator
{
    private readonly LayoutEditorState _state;
    private readonly FrameworkElement _rootGrid;
    private readonly FrameworkElement _overlayChromeLayer;
    private readonly Border _floatingMenu;
    private readonly Border _propertiesPanel;
    private readonly LayoutEditorSelectionService _selection;
    private readonly LayoutEditorMarqueePresenter _marquee;
    private readonly Func<DependencyObject?, LayoutEditorWidget?> _resolveWidget;
    private readonly Action<Point> _updatePresetPreview;
    private readonly Action<Point> _updateWidgetPreview;
    private readonly Action _confirmPresetPlacement;
    private readonly Action _confirmWidgetPlacement;
    private readonly Action _cancelPlacement;
    private readonly Action<double, double> _moveFloatingMenu;
    private readonly Action<double, double> _movePropertiesPanel;
    private readonly Action _hideGuides;
    private readonly Action _refreshSelectionDetails;
    private readonly Action _positionPropertiesPanel;
    private readonly Action<LayoutEditorWidget, MouseButtonEventArgs> _handleWidgetResizeClick;
    private readonly Action<Point> _updateResizedWidgets;

    public LayoutEditorInteractionCoordinator(
        LayoutEditorState state,
        FrameworkElement rootGrid,
        FrameworkElement overlayChromeLayer,
        Border floatingMenu,
        Border propertiesPanel,
        LayoutEditorSelectionService selection,
        LayoutEditorMarqueePresenter marquee,
        Func<DependencyObject?, LayoutEditorWidget?> resolveWidget,
        Action<Point> updatePresetPreview,
        Action<Point> updateWidgetPreview,
        Action confirmPresetPlacement,
        Action confirmWidgetPlacement,
        Action cancelPlacement,
        Action<double, double> moveFloatingMenu,
        Action<double, double> movePropertiesPanel,
        Action hideGuides,
        Action refreshSelectionDetails,
        Action positionPropertiesPanel,
        Action<LayoutEditorWidget, MouseButtonEventArgs> handleWidgetResizeClick,
        Action<Point> updateResizedWidgets)
    {
        _state = state;
        _rootGrid = rootGrid;
        _overlayChromeLayer = overlayChromeLayer;
        _floatingMenu = floatingMenu;
        _propertiesPanel = propertiesPanel;
        _selection = selection;
        _marquee = marquee;
        _resolveWidget = resolveWidget;
        _updatePresetPreview = updatePresetPreview;
        _updateWidgetPreview = updateWidgetPreview;
        _confirmPresetPlacement = confirmPresetPlacement;
        _confirmWidgetPlacement = confirmWidgetPlacement;
        _cancelPlacement = cancelPlacement;
        _moveFloatingMenu = moveFloatingMenu;
        _movePropertiesPanel = movePropertiesPanel;
        _hideGuides = hideGuides;
        _refreshSelectionDetails = refreshSelectionDetails;
        _positionPropertiesPanel = positionPropertiesPanel;
        _handleWidgetResizeClick = handleWidgetResizeClick;
        _updateResizedWidgets = updateResizedWidgets;
    }

    public bool HandleRootLeftButtonDown(MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return false;
        }

        if (LayoutEditorVisualTreeService.IsDescendantOf(source, _propertiesPanel) || LayoutEditorVisualTreeService.IsDescendantOf(source, _floatingMenu))
        {
            return false;
        }

        if (_state.IsPlacingPreset)
        {
            _confirmPresetPlacement();
            return true;
        }

        if (_state.IsPlacingWidget)
        {
            _confirmWidgetPlacement();
            return true;
        }

        var clickedWidget = _resolveWidget(source);
        if (clickedWidget is not null)
        {
            BeginWidgetInteraction(clickedWidget, e.GetPosition(_rootGrid));
            return true;
        }

        _state.MarqueeStart = e.GetPosition(_overlayChromeLayer);
        _marquee.Begin(_state.MarqueeStart);
        _selection.SelectWidgets([], null);
        _rootGrid.CaptureMouse();
        return true;
    }

    public void HandleRootRightButtonDown(MouseButtonEventArgs e)
    {
        if (_state.IsPlacingPreset || _state.IsPlacingWidget)
        {
            _cancelPlacement();
            e.Handled = true;
            return;
        }

        if (e.OriginalSource is DependencyObject source && LayoutEditorVisualTreeService.FindAncestor<ContentPresenter>(source) is not null)
        {
            return;
        }

        _selection.SelectWidgets([], null);
        _refreshSelectionDetails();
    }

    public bool HandleRootMouseMove(MouseEventArgs e)
    {
        var overlayPoint = e.GetPosition(_overlayChromeLayer);

        if (_state.IsPlacingPreset)
        {
            _updatePresetPreview(overlayPoint);
        }
        else if (_state.IsPlacingWidget)
        {
            _updateWidgetPreview(overlayPoint);
        }

        if (_state.IsDraggingToolbar && e.LeftButton == MouseButtonState.Pressed)
        {
            var target = LayoutEditorFloatingPanelDragService.ResolveToolbarPosition(_state, overlayPoint);
            _moveFloatingMenu(target.X, target.Y);
            return true;
        }

        if (_state.IsDraggingProperties && e.LeftButton == MouseButtonState.Pressed)
        {
            var target = LayoutEditorFloatingPanelDragService.ResolvePropertiesPosition(_state, overlayPoint);
            _movePropertiesPanel(target.X, target.Y);
            return true;
        }

        if (_state.IsDraggingWidgets && e.LeftButton == MouseButtonState.Pressed)
        {
            _selection.UpdateDraggedWidgets(e.GetPosition(_rootGrid), new Size(_rootGrid.ActualWidth, _rootGrid.ActualHeight));
            return true;
        }

        if (_state.IsResizingWidgets && e.LeftButton == MouseButtonState.Pressed)
        {
            _updateResizedWidgets(e.GetPosition(_rootGrid));
            return true;
        }

        if (!_state.IsMarqueeSelecting)
        {
            return false;
        }

        _marquee.Update(overlayPoint);
        return false;
    }

    public bool HandleRootLeftButtonUp(MouseButtonEventArgs e)
    {
        if (LayoutEditorFloatingPanelDragService.EndToolbarDrag(_state))
        {
            _rootGrid.ReleaseMouseCapture();
            return true;
        }

        if (LayoutEditorFloatingPanelDragService.EndPropertiesDrag(_state))
        {
            _rootGrid.ReleaseMouseCapture();
            return true;
        }

        if (_state.IsDraggingWidgets)
        {
            LayoutEditorDragService.EndWidgetDrag(_state);
            _rootGrid.ReleaseMouseCapture();
            _hideGuides();
            _refreshSelectionDetails();
            return true;
        }

        if (_state.IsResizingWidgets)
        {
            LayoutEditorResizeService.EndWidgetResize(_state);
            _rootGrid.ReleaseMouseCapture();
            _hideGuides();
            _refreshSelectionDetails();
            return true;
        }

        if (!_state.IsMarqueeSelecting)
        {
            return false;
        }

        _marquee.Update(e.GetPosition(_overlayChromeLayer));
        _marquee.End();
        _hideGuides();
        _rootGrid.ReleaseMouseCapture();
        return true;
    }

    public bool HandleWidgetLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not LayoutEditorWidget widget)
        {
            return false;
        }

        if (e.OriginalSource is DependencyObject source && LayoutEditorVisualTreeService.IsResizeHandle(source))
        {
            _handleWidgetResizeClick(widget, e);
            return true;
        }

        BeginWidgetInteraction(widget, e.GetPosition(_rootGrid));
        return true;
    }

    public bool HandleWidgetMouseMove(MouseEventArgs e)
    {
        if (_state.IsResizingWidgets && e.LeftButton == MouseButtonState.Pressed)
        {
            _updateResizedWidgets(e.GetPosition(_rootGrid));
            return true;
        }

        if (!_state.IsDraggingWidgets || e.LeftButton != MouseButtonState.Pressed)
        {
            return false;
        }

        _selection.UpdateDraggedWidgets(e.GetPosition(_rootGrid), new Size(_rootGrid.ActualWidth, _rootGrid.ActualHeight));
        return true;
    }

    public bool HandleWidgetLeftButtonUp()
    {
        if (_state.IsResizingWidgets)
        {
            LayoutEditorResizeService.EndWidgetResize(_state);
            _rootGrid.ReleaseMouseCapture();
            _hideGuides();
            _refreshSelectionDetails();
            return true;
        }

        if (!_state.IsDraggingWidgets)
        {
            return false;
        }

        LayoutEditorDragService.EndWidgetDrag(_state);
        _rootGrid.ReleaseMouseCapture();
        _hideGuides();
        _refreshSelectionDetails();
        return true;
    }

    public void HandleWidgetRightButtonDown(object? sender)
    {
        if (sender is not FrameworkElement element || element.DataContext is not LayoutEditorWidget widget)
        {
            return;
        }

        if (!widget.IsSelected)
        {
            _selection.SelectWidgets([widget], widget);
            return;
        }

        _state.PrimarySelectedWidget = widget;
        _refreshSelectionDetails();
        _positionPropertiesPanel();
    }

    public void HandleWidgetContextMenuOpened(object? sender)
    {
        if (sender is ContextMenu contextMenu && contextMenu.PlacementTarget is FrameworkElement element && element.DataContext is LayoutEditorWidget widget && !widget.IsSelected)
        {
            _selection.SelectWidgets([widget], widget);
        }
    }

    public void BeginToolbarDrag(Point dragOffset)
    {
        LayoutEditorFloatingPanelDragService.BeginToolbarDrag(_state, dragOffset);
        _rootGrid.CaptureMouse();
    }

    public bool EndToolbarDrag()
    {
        if (!LayoutEditorFloatingPanelDragService.EndToolbarDrag(_state))
        {
            return false;
        }

        _rootGrid.ReleaseMouseCapture();
        return true;
    }

    public void BeginPropertiesDrag(Point dragOffset)
    {
        LayoutEditorFloatingPanelDragService.BeginPropertiesDrag(_state, dragOffset);
        _rootGrid.CaptureMouse();
    }

    public bool EndPropertiesDrag()
    {
        if (!LayoutEditorFloatingPanelDragService.EndPropertiesDrag(_state))
        {
            return false;
        }

        _rootGrid.ReleaseMouseCapture();
        return true;
    }

    private void BeginWidgetInteraction(LayoutEditorWidget widget, Point rootPoint)
    {
        var selected = _selection.PrepareWidgetSelection(widget);
        LayoutEditorDragService.BeginWidgetDrag(_state, selected, rootPoint);
        _rootGrid.CaptureMouse();
    }
}
