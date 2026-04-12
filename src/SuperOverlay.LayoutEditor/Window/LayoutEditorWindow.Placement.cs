using System.Windows;

using SuperOverlay.Core.Layouts.Editor;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    private void BeginWidgetPlacement()
    {
        _placement.BeginWidgetPlacement(LayoutEditorUiConstants.DefaultWidgetWidth, LayoutEditorUiConstants.DefaultWidgetHeight);
        Focus();
    }

    private void UpdateWidgetPreview(Point pointer)
    {
        _placement.UpdateWidgetPreview(pointer);
    }

    private void ConfirmWidgetPlacement()
    {
        _placement.ConfirmWidgetPlacement();
    }

    internal void CancelPlacement(bool clearSelection = true)
    {
        _placement.CancelPlacement(clearSelection);
    }

    private void BeginPresetPlacement(LayoutEditorPresetDocument preset)
    {
        _placement.BeginPresetPlacement(preset);
        Focus();
    }

    private void UpdatePresetPreview(Point pointer)
    {
        _placement.UpdatePresetPreview(pointer);
    }

    private void ConfirmPresetPlacement()
    {
        _placement.ConfirmPresetPlacement();
    }

    private void ApplyLayout(LayoutEditorLayoutDocument document)
    {
        _placement.ApplyLayout(document);
        Title = $"SuperOverlay LayoutEditor - {document.Name}";
    }
}
