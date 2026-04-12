namespace SuperOverlay.Core.Layouts.Editor;

public sealed class LayoutEditorPresetDocument
{
    public string Name { get; set; } = string.Empty;
    public DateTime SavedAtUtc { get; set; }
    public List<LayoutEditorPresetWidget> Widgets { get; set; } = [];
}

public sealed class LayoutEditorPresetWidget
{
    public Guid Id { get; set; }
    public Guid? GroupId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool ShowInRace { get; set; }
    public bool IsLocked { get; set; }
    public string? TopLeftContent { get; set; }
    public string? TopRightContent { get; set; }
    public string? CenterContent { get; set; }
    public string? BottomLeftContent { get; set; }
    public string? BottomRightContent { get; set; }
    public LayoutEditorTextSizePreset TopLeftTextSizePreset { get; set; } = LayoutEditorTextSizePreset.S;
    public LayoutEditorTextSizePreset TopRightTextSizePreset { get; set; } = LayoutEditorTextSizePreset.S;
    public LayoutEditorTextSizePreset CenterTextSizePreset { get; set; } = LayoutEditorTextSizePreset.M;
    public LayoutEditorTextSizePreset BottomLeftTextSizePreset { get; set; } = LayoutEditorTextSizePreset.S;
    public LayoutEditorTextSizePreset BottomRightTextSizePreset { get; set; } = LayoutEditorTextSizePreset.S;
    public LayoutEditorTextRole TopLeftTextRole { get; set; } = LayoutEditorTextRole.Label;
    public LayoutEditorTextRole TopRightTextRole { get; set; } = LayoutEditorTextRole.Label;
    public LayoutEditorTextRole CenterTextRole { get; set; } = LayoutEditorTextRole.Primary;
    public LayoutEditorTextRole BottomLeftTextRole { get; set; } = LayoutEditorTextRole.Label;
    public LayoutEditorTextRole BottomRightTextRole { get; set; } = LayoutEditorTextRole.Label;
}
