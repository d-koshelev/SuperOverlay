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
    private readonly LayoutEditorMarqueePresenter _marquee;
    private readonly ILayoutEditorHitTestService _hitTest;
    private readonly Action<Point> _updatePresetPreview;
    private readonly Action<Point> _updateWidgetPreview;
    private readonly Action _confirmPresetPlacement;
    private readonly Action _confirmWidgetPlacement;
    private readonly Action _cancelPlacement;
    private readonly LayoutEditorFloatingPanelInteractionService _floatingPanels;
    private readonly Action _hideGuides;
    private readonly Action _refreshSelectionDetails;
    private readonly Action _positionPropertiesPanel;
    private readonly Action<IReadOnlyCollection<LayoutEditorWidget>, LayoutEditorWidget?> _selectWidgets;
    private readonly Action<LayoutEditorWidget, MouseButtonEventArgs> _handleWidgetLeftClick;
    private readonly Action<LayoutEditorWidget, MouseButtonEventArgs> _handleWidgetResizeClick;
    private readonly Action<LayoutEditorWidget, Point> _beginWidgetDrag;
    private readonly Action _endWidgetDrag;
    private readonly Action _endWidgetResize;
    private readonly Action<LayoutEditorWidget> _toggleWidgetSelection;
    private readonly Action<Point> _updateDraggedWidgets;
    private readonly Action<Point> _updateResizedWidgets;

    public LayoutEditorInteractionCoordinator(
        LayoutEditorState state,
        FrameworkElement rootGrid,
        FrameworkElement overlayChromeLayer,
        Border floatingMenu,
        Border propertiesPanel,
        LayoutEditorMarqueePresenter marquee,
        ILayoutEditorHitTestService hitTest,
        Action<Point> updatePresetPreview,
        Action<Point> updateWidgetPreview,
        Action confirmPresetPlacement,
        Action confirmWidgetPlacement,
        Action cancelPlacement,
        LayoutEditorFloatingPanelInteractionService floatingPanels,
        Action hideGuides,
        Action refreshSelectionDetails,
        Action positionPropertiesPanel,
        Action<IReadOnlyCollection<LayoutEditorWidget>, LayoutEditorWidget?> selectWidgets,
        Action<LayoutEditorWidget, MouseButtonEventArgs> handleWidgetLeftClick,
        Action<LayoutEditorWidget, MouseButtonEventArgs> handleWidgetResizeClick,
        Action<LayoutEditorWidget, Point> beginWidgetDrag,
        Action endWidgetDrag,
        Action endWidgetResize,
        Action<LayoutEditorWidget> toggleWidgetSelection,
        Action<Point> updateDraggedWidgets,
        Action<Point> updateResizedWidgets)
    {
        _state = state;
        _rootGrid = rootGrid;
        _overlayChromeLayer = overlayChromeLayer;
        _floatingMenu = floatingMenu;
        _propertiesPanel = propertiesPanel;
        _marquee = marquee;
        _hitTest = hitTest;
        _updatePresetPreview = updatePresetPreview;
        _updateWidgetPreview = updateWidgetPreview;
        _confirmPresetPlacement = confirmPresetPlacement;
        _confirmWidgetPlacement = confirmWidgetPlacement;
        _cancelPlacement = cancelPlacement;
        _floatingPanels = floatingPanels;
        _hideGuides = hideGuides;
        _refreshSelectionDetails = refreshSelectionDetails;
        _positionPropertiesPanel = positionPropertiesPanel;
        _selectWidgets = selectWidgets;
        _handleWidgetLeftClick = handleWidgetLeftClick;
        _handleWidgetResizeClick = handleWidgetResizeClick;
        _beginWidgetDrag = beginWidgetDrag;
        _endWidgetDrag = endWidgetDrag;
        _endWidgetResize = endWidgetResize;
        _toggleWidgetSelection = toggleWidgetSelection;
        _updateDraggedWidgets = updateDraggedWidgets;
        _updateResizedWidgets = updateResizedWidgets;
    }

    public bool HandleRootLeftButtonDown(MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return false;
        }

        var hit = _hitTest.Resolve(source);
        if (hit.Kind is LayoutEditorHitKind.FloatingMenu or LayoutEditorHitKind.PropertiesPanel)
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

        if (hit.Kind == LayoutEditorHitKind.ResizeHandle && hit.Widget is not null)
        {
            _handleWidgetResizeClick(hit.Widget, e);
            return true;
        }

        if (hit.Kind == LayoutEditorHitKind.WidgetBody && hit.Widget is not null)
        {
            _handleWidgetLeftClick(hit.Widget, e);
            return true;
        }

        _state.MarqueeStart = e.GetPosition(_overlayChromeLayer);
        _marquee.Begin(_state.MarqueeStart);
        _selectWidgets([], null);
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

        if (e.OriginalSource is DependencyObject source && _hitTest.IsInteractiveContent(source))
        {
            return;
        }

        _selectWidgets([], null);
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

        if (e.LeftButton == MouseButtonState.Pressed && _floatingPanels.Update(overlayPoint))
        {
            return true;
        }

        if (_state.IsPendingWidgetClick && e.LeftButton == MouseButtonState.Pressed)
        {
            var pointer = e.GetPosition(_rootGrid);
            var delta = pointer - _state.WidgetDragStartPointer;
            if ((System.Math.Abs(delta.X) >= SystemParameters.MinimumHorizontalDragDistance
                 || System.Math.Abs(delta.Y) >= SystemParameters.MinimumVerticalDragDistance)
                && _state.PendingWidgetClickTarget is { IsLocked: false } widget)
            {
                _state.IsPendingWidgetClick = false;
                _state.PendingWidgetClickTarget = null;
                _beginWidgetDrag(widget, _state.WidgetDragStartPointer);
                _updateDraggedWidgets(pointer);
                return true;
            }
        }

        if (_state.IsDraggingWidgets && e.LeftButton == MouseButtonState.Pressed)
        {
            _updateDraggedWidgets(e.GetPosition(_rootGrid));
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
            _endWidgetDrag();
            _rootGrid.ReleaseMouseCapture();
            _hideGuides();
            _refreshSelectionDetails();
            return true;
        }

        if (_state.IsPendingWidgetClick)
        {
            var widget = _state.PendingWidgetClickTarget;
            var shouldToggleOff = widget is not null
                && _state.PendingWidgetWasSelected
                && !_state.PendingWidgetPreserveSelection;

            _state.IsPendingWidgetClick = false;
            _state.PendingWidgetClickTarget = null;

            if (shouldToggleOff && widget is not null)
            {
                _toggleWidgetSelection(widget);
            }

            return widget is not null;
        }

        if (_state.IsResizingWidgets)
        {
            _endWidgetResize();
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

        if (e.OriginalSource is DependencyObject source && _hitTest.IsResizeHandle(source))
        {
            _handleWidgetResizeClick(widget, e);
            return true;
        }

        return false;
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

        _updateDraggedWidgets(e.GetPosition(_rootGrid));
        return true;
    }

    public bool HandleWidgetLeftButtonUp()
    {
        if (_state.IsResizingWidgets)
        {
            _endWidgetResize();
            _rootGrid.ReleaseMouseCapture();
            _hideGuides();
            _refreshSelectionDetails();
            return true;
        }

        if (!_state.IsDraggingWidgets)
        {
            return false;
        }

        _endWidgetDrag();
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
            _selectWidgets([widget], widget);
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
            _selectWidgets([widget], widget);
        }
    }

    public void BeginToolbarDrag(Point dragOffset)
    {
        _floatingPanels.BeginToolbarDrag(dragOffset);
        _rootGrid.CaptureMouse();
    }

    public bool EndToolbarDrag()
    {
        if (!_floatingPanels.EndToolbarDrag())
        {
            return false;
        }

        _rootGrid.ReleaseMouseCapture();
        return true;
    }

    public void BeginPropertiesDrag(Point dragOffset)
    {
        _floatingPanels.BeginPropertiesDrag(dragOffset);
        _rootGrid.CaptureMouse();
    }

    public bool EndPropertiesDrag()
    {
        if (!_floatingPanels.EndPropertiesDrag())
        {
            return false;
        }

        _rootGrid.ReleaseMouseCapture();
        return true;
    }

}
