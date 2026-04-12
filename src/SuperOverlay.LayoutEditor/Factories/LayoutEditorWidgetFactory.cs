namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorWidgetFactory
{
    public static LayoutEditorWidget CreateDefault(double width, double height, Guid? id = null)
    {
        return new LayoutEditorWidget
        {
            Id = id ?? Guid.NewGuid(),
            Width = width,
            Height = height,
            ShowInRace = true,
            IsLocked = false,
            IsVisibleInCurrentMode = true,
        };
    }

    public static LayoutEditorWidget CreateCopy(LayoutEditorWidget source, double? x = null, double? y = null, Guid? groupId = null, Guid? id = null)
    {
        return new LayoutEditorWidget
        {
            Id = id ?? Guid.NewGuid(),
            X = x ?? source.X,
            Y = y ?? source.Y,
            Width = source.Width,
            Height = source.Height,
            ShowInRace = source.ShowInRace,
            IsLocked = source.IsLocked,
            IsVisibleInCurrentMode = source.IsVisibleInCurrentMode,
            GroupId = groupId ?? source.GroupId,
            TopLeftContent = source.TopLeftContent,
            TopRightContent = source.TopRightContent,
            CenterContent = source.CenterContent,
            BottomLeftContent = source.BottomLeftContent,
            BottomRightContent = source.BottomRightContent,
            TopLeftTextSizePreset = source.TopLeftTextSizePreset,
            TopRightTextSizePreset = source.TopRightTextSizePreset,
            CenterTextSizePreset = source.CenterTextSizePreset,
            BottomLeftTextSizePreset = source.BottomLeftTextSizePreset,
            BottomRightTextSizePreset = source.BottomRightTextSizePreset,
            TopLeftTextRole = source.TopLeftTextRole,
            TopRightTextRole = source.TopRightTextRole,
            CenterTextRole = source.CenterTextRole,
            BottomLeftTextRole = source.BottomLeftTextRole,
            BottomRightTextRole = source.BottomRightTextRole,
        };
    }

    public static LayoutEditorWidget CreateFromPreset(LayoutEditorPresetWidget source, double x, double y, Guid? groupId = null)
    {
        return new LayoutEditorWidget
        {
            X = x,
            Y = y,
            Width = source.Width,
            Height = source.Height,
            ShowInRace = source.ShowInRace,
            IsLocked = source.IsLocked,
            IsVisibleInCurrentMode = true,
            GroupId = groupId ?? source.GroupId,
            TopLeftContent = source.TopLeftContent,
            TopRightContent = source.TopRightContent,
            CenterContent = source.CenterContent,
            BottomLeftContent = source.BottomLeftContent,
            BottomRightContent = source.BottomRightContent,
            TopLeftTextSizePreset = source.TopLeftTextSizePreset,
            TopRightTextSizePreset = source.TopRightTextSizePreset,
            CenterTextSizePreset = source.CenterTextSizePreset,
            BottomLeftTextSizePreset = source.BottomLeftTextSizePreset,
            BottomRightTextSizePreset = source.BottomRightTextSizePreset,
            TopLeftTextRole = source.TopLeftTextRole,
            TopRightTextRole = source.TopRightTextRole,
            CenterTextRole = source.CenterTextRole,
            BottomLeftTextRole = source.BottomLeftTextRole,
            BottomRightTextRole = source.BottomRightTextRole,
        };
    }

    public static LayoutEditorWidget CreateFromLayout(LayoutEditorLayoutWidget source, double x, double y)
    {
        return new LayoutEditorWidget
        {
            X = x,
            Y = y,
            Width = source.Width,
            Height = source.Height,
            ShowInRace = source.ShowInRace,
            IsLocked = source.IsLocked,
            IsVisibleInCurrentMode = true,
            GroupId = source.GroupId,
            TopLeftContent = source.TopLeftContent,
            TopRightContent = source.TopRightContent,
            CenterContent = source.CenterContent,
            BottomLeftContent = source.BottomLeftContent,
            BottomRightContent = source.BottomRightContent,
            TopLeftTextSizePreset = source.TopLeftTextSizePreset,
            TopRightTextSizePreset = source.TopRightTextSizePreset,
            CenterTextSizePreset = source.CenterTextSizePreset,
            BottomLeftTextSizePreset = source.BottomLeftTextSizePreset,
            BottomRightTextSizePreset = source.BottomRightTextSizePreset,
            TopLeftTextRole = source.TopLeftTextRole,
            TopRightTextRole = source.TopRightTextRole,
            CenterTextRole = source.CenterTextRole,
            BottomLeftTextRole = source.BottomLeftTextRole,
            BottomRightTextRole = source.BottomRightTextRole,
        };
    }
}
