using System.Windows;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorState
{
    public string? CurrentLayoutName { get; set; }
    public bool IsRaceMode { get; set; }
    public bool IsSnappingEnabled { get; set; } = true;
    public bool AreGuidesEnabled { get; set; } = true;

    public bool IsDraggingToolbar { get; set; }
    public bool IsDraggingProperties { get; set; }
    public bool IsDraggingWidgets { get; set; }
    public bool IsResizingWidgets { get; set; }
    public bool IsMarqueeSelecting { get; set; }

    public Point ToolbarDragOffset { get; set; }
    public Point PropertiesDragOffset { get; set; }
    public Point WidgetDragStartPointer { get; set; }
    public Point WidgetResizeStartPointer { get; set; }
    public Point MarqueeStart { get; set; }

    public LayoutEditorWidget? PrimarySelectedWidget { get; set; }

    public Dictionary<Guid, Point> DragStartPositions { get; } = [];
    public Dictionary<Guid, Size> ResizeStartSizes { get; } = [];

    public LayoutEditorPresetDocument? PendingPresetPlacement { get; set; }
    public LayoutEditorWidget? PendingWidgetPlacement { get; set; }
    public bool IsPlacingPreset { get; set; }
    public bool IsPlacingWidget { get; set; }
}
