using SuperOverlay.Core.Layouts.Editor;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorWidgetFactory
{
    public static LayoutEditorWidget CreateDefault(double width, double height, Guid? id = null)
    {
        return ApplyRawPreview(new LayoutEditorWidget
        {
            Id = id ?? Guid.NewGuid(),
            Width = width,
            Height = height,
            ShowInRace = true,
            IsLocked = false,
            IsVisibleInCurrentMode = true,
            RawBindingSource = "TelemetryRaw",
            RawBindingFieldPath = "Speed",
        });
    }

    public static LayoutEditorWidget CreateCopy(LayoutEditorWidget source, double? x = null, double? y = null, Guid? groupId = null, Guid? id = null)
    {
        return ApplyRawPreview(new LayoutEditorWidget
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
            RawBindingSource = source.RawBindingSource,
            RawBindingFieldPath = source.RawBindingFieldPath,
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
        });
    }

    public static LayoutEditorWidget CreateFromPreset(LayoutEditorPresetWidget source, double x, double y, Guid? groupId = null)
    {
        return ApplyRawPreview(new LayoutEditorWidget
        {
            X = x,
            Y = y,
            Width = source.Width,
            Height = source.Height,
            ShowInRace = source.ShowInRace,
            IsLocked = source.IsLocked,
            IsVisibleInCurrentMode = true,
            GroupId = groupId ?? source.GroupId,
            RawBindingSource = source.RawBindingSource,
            RawBindingFieldPath = source.RawBindingFieldPath,
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
        });
    }

    public static LayoutEditorWidget CreateFromLayout(LayoutEditorLayoutWidget source, double x, double y)
    {
        return ApplyRawPreview(new LayoutEditorWidget
        {
            X = x,
            Y = y,
            Width = source.Width,
            Height = source.Height,
            ShowInRace = source.ShowInRace,
            IsLocked = source.IsLocked,
            IsVisibleInCurrentMode = true,
            GroupId = source.GroupId,
            RawBindingSource = source.RawBindingSource,
            RawBindingFieldPath = source.RawBindingFieldPath,
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
        });
    }

    private static LayoutEditorWidget ApplyRawPreview(LayoutEditorWidget widget)
    {
        if (string.IsNullOrWhiteSpace(widget.CenterContent)
            || string.Equals(widget.CenterContent, widget.RawBindingFieldPath, StringComparison.OrdinalIgnoreCase))
        {
            widget.CenterContent = LayoutEditorRawFieldCatalog.GetPreviewValue(widget.RawBindingSource, widget.RawBindingFieldPath);
        }

        return widget;
    }
}
