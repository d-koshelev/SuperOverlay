using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorPlacementPresenter
{
    private readonly LayoutEditorState _state;
    private readonly ObservableCollection<LayoutEditorWidget> _widgets;
    private readonly ObservableCollection<LayoutEditorWidget> _previewWidgets;
    private readonly Border _placementHintPanel;
    private readonly TextBlock _placementHintText;
    private readonly Func<Size> _viewportProvider;
    private readonly Func<Point> _mouseOverlayPositionProvider;
    private readonly Action<IReadOnlyCollection<LayoutEditorWidget>, LayoutEditorWidget?> _selectWidgets;
    private readonly Action _positionPropertiesPanel;

    public LayoutEditorPlacementPresenter(
        LayoutEditorState state,
        ObservableCollection<LayoutEditorWidget> widgets,
        ObservableCollection<LayoutEditorWidget> previewWidgets,
        Border placementHintPanel,
        TextBlock placementHintText,
        Func<Size> viewportProvider,
        Func<Point> mouseOverlayPositionProvider,
        Action<IReadOnlyCollection<LayoutEditorWidget>, LayoutEditorWidget?> selectWidgets,
        Action positionPropertiesPanel)
    {
        _state = state;
        _widgets = widgets;
        _previewWidgets = previewWidgets;
        _placementHintPanel = placementHintPanel;
        _placementHintText = placementHintText;
        _viewportProvider = viewportProvider;
        _mouseOverlayPositionProvider = mouseOverlayPositionProvider;
        _selectWidgets = selectWidgets;
        _positionPropertiesPanel = positionPropertiesPanel;
    }

    public void BeginWidgetPlacement(double defaultWidth, double defaultHeight)
    {
        CancelPlacement(clearSelection: false);
        LayoutEditorPlacementService.BeginWidgetPlacement(_state, LayoutEditorWidgetFactory.CreateDefault(defaultWidth, defaultHeight));
        _placementHintText.Text = "Widget placement — left click to place, right click or Esc to cancel";
        _placementHintPanel.Visibility = Visibility.Visible;
        _previewWidgets.Clear();
        UpdateWidgetPreview(_mouseOverlayPositionProvider());
    }

    public void UpdateWidgetPreview(Point pointer)
    {
        if (!_state.IsPlacingWidget || _state.PendingWidgetPlacement is null)
        {
            return;
        }

        _previewWidgets.Clear();
        foreach (var preview in LayoutEditorPlacementService.BuildWidgetPreview(_state, pointer, _viewportProvider()))
        {
            _previewWidgets.Add(preview);
        }
    }

    public void ConfirmWidgetPlacement()
    {
        if (!_state.IsPlacingWidget || _state.PendingWidgetPlacement is null || _previewWidgets.Count == 0)
        {
            CancelWidgetPlacement();
            return;
        }

        var widget = LayoutEditorWidgetFactory.CreateCopy(_previewWidgets[0]);
        _widgets.Add(widget);
        CancelWidgetPlacement(clearSelection: false);
        _selectWidgets([widget], widget);
    }

    public void BeginPresetPlacement(LayoutEditorPresetDocument preset)
    {
        CancelPlacement(clearSelection: false);
        LayoutEditorPlacementService.BeginPresetPlacement(_state, preset);
        _placementHintText.Text = $"Preset placement: {preset.Name} — left click to place, right click or Esc to cancel";
        _placementHintPanel.Visibility = Visibility.Visible;
        _previewWidgets.Clear();
        UpdatePresetPreview(_mouseOverlayPositionProvider());
    }

    public void UpdatePresetPreview(Point pointer)
    {
        if (!_state.IsPlacingPreset || _state.PendingPresetPlacement is null || _state.PendingPresetPlacement.Widgets.Count == 0)
        {
            return;
        }

        _previewWidgets.Clear();
        foreach (var preview in LayoutEditorPlacementService.BuildPresetPreview(_state, pointer, _viewportProvider()))
        {
            _previewWidgets.Add(preview);
        }
    }

    public void ConfirmPresetPlacement()
    {
        if (!_state.IsPlacingPreset || _state.PendingPresetPlacement is null || _previewWidgets.Count == 0)
        {
            CancelPresetPlacement();
            return;
        }

        var created = LayoutEditorPlacementService.MaterializePresetPreview(_previewWidgets).ToList();
        foreach (var widget in created)
        {
            _widgets.Add(widget);
        }

        CancelPlacement(clearSelection: false);
        _selectWidgets(created, created.FirstOrDefault());
    }

    public void CancelPlacement(bool clearSelection = true)
    {
        if (_state.IsPlacingPreset)
        {
            CancelPresetPlacement(clearSelection);
            return;
        }

        if (_state.IsPlacingWidget)
        {
            CancelWidgetPlacement(clearSelection);
        }
    }

    public void ApplyLayout(LayoutEditorLayoutDocument document)
    {
        CancelPlacement(clearSelection: false);
        _widgets.Clear();

        var created = LayoutEditorPlacementService.CreateWidgetsFromLayout(document, _viewportProvider()).ToList();
        foreach (var widget in created)
        {
            _widgets.Add(widget);
        }

        _state.CurrentLayoutName = document.Name;
        _selectWidgets(created, created.FirstOrDefault());
    }

    private void CancelWidgetPlacement(bool clearSelection = true)
    {
        LayoutEditorPlacementService.Cancel(_state);
        _previewWidgets.Clear();
        _placementHintPanel.Visibility = Visibility.Collapsed;

        if (clearSelection)
        {
            _positionPropertiesPanel();
        }
    }

    private void CancelPresetPlacement(bool clearSelection = true)
    {
        LayoutEditorPlacementService.Cancel(_state);
        _previewWidgets.Clear();
        _placementHintPanel.Visibility = Visibility.Collapsed;

        if (clearSelection)
        {
            _positionPropertiesPanel();
        }
    }
}
