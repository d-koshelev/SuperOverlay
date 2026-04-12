namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorDocumentMapper
{
    public static LayoutEditorPresetWidget ToPresetWidget(LayoutEditorWidget widget, double offsetX = 0, double offsetY = 0)
    {
        return new LayoutEditorPresetWidget
        {
            Id = widget.Id,
            GroupId = widget.GroupId,
            X = widget.X - offsetX,
            Y = widget.Y - offsetY,
            Width = widget.Width,
            Height = widget.Height,
            ShowInRace = widget.ShowInRace,
            IsLocked = widget.IsLocked,
            TopLeftContent = widget.TopLeftContent,
            TopRightContent = widget.TopRightContent,
            CenterContent = widget.CenterContent,
            BottomLeftContent = widget.BottomLeftContent,
            BottomRightContent = widget.BottomRightContent,
            TopLeftTextSizePreset = widget.TopLeftTextSizePreset,
            TopRightTextSizePreset = widget.TopRightTextSizePreset,
            CenterTextSizePreset = widget.CenterTextSizePreset,
            BottomLeftTextSizePreset = widget.BottomLeftTextSizePreset,
            BottomRightTextSizePreset = widget.BottomRightTextSizePreset,
            TopLeftTextRole = widget.TopLeftTextRole,
            TopRightTextRole = widget.TopRightTextRole,
            CenterTextRole = widget.CenterTextRole,
            BottomLeftTextRole = widget.BottomLeftTextRole,
            BottomRightTextRole = widget.BottomRightTextRole,
        };
    }

    public static LayoutEditorLayoutWidget ToLayoutWidget(LayoutEditorWidget widget)
    {
        return new LayoutEditorLayoutWidget
        {
            Id = widget.Id,
            GroupId = widget.GroupId,
            X = widget.X,
            Y = widget.Y,
            Width = widget.Width,
            Height = widget.Height,
            ShowInRace = widget.ShowInRace,
            IsLocked = widget.IsLocked,
            TopLeftContent = widget.TopLeftContent,
            TopRightContent = widget.TopRightContent,
            CenterContent = widget.CenterContent,
            BottomLeftContent = widget.BottomLeftContent,
            BottomRightContent = widget.BottomRightContent,
            TopLeftTextSizePreset = widget.TopLeftTextSizePreset,
            TopRightTextSizePreset = widget.TopRightTextSizePreset,
            CenterTextSizePreset = widget.CenterTextSizePreset,
            BottomLeftTextSizePreset = widget.BottomLeftTextSizePreset,
            BottomRightTextSizePreset = widget.BottomRightTextSizePreset,
            TopLeftTextRole = widget.TopLeftTextRole,
            TopRightTextRole = widget.TopRightTextRole,
            CenterTextRole = widget.CenterTextRole,
            BottomLeftTextRole = widget.BottomLeftTextRole,
            BottomRightTextRole = widget.BottomRightTextRole,
        };
    }
}
