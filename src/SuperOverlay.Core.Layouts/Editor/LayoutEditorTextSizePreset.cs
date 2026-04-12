namespace SuperOverlay.Core.Layouts.Editor;

public enum LayoutEditorTextSizePreset
{
    XS,
    S,
    M,
    L,
    XL,
}

public static class LayoutEditorTextSizePresetExtensions
{
    public static double ToFontSize(this LayoutEditorTextSizePreset preset, bool isCenter = false)
    {
        return (preset, isCenter) switch
        {
            (LayoutEditorTextSizePreset.XS, false) => 11,
            (LayoutEditorTextSizePreset.S, false) => 13,
            (LayoutEditorTextSizePreset.M, false) => 15,
            (LayoutEditorTextSizePreset.L, false) => 18,
            (LayoutEditorTextSizePreset.XL, false) => 22,
            (LayoutEditorTextSizePreset.XS, true) => 18,
            (LayoutEditorTextSizePreset.S, true) => 22,
            (LayoutEditorTextSizePreset.M, true) => 28,
            (LayoutEditorTextSizePreset.L, true) => 36,
            (LayoutEditorTextSizePreset.XL, true) => 48,
            _ => 13,
        };
    }
}
