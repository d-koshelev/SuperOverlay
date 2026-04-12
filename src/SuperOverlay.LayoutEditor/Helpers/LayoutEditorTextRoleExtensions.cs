using System.Windows;
using System.Windows.Media;

using SuperOverlay.Core.Layouts.Editor;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorTextRoleExtensions
{
    public static FontWeight ToFontWeight(this LayoutEditorTextRole role) => role switch
    {
        LayoutEditorTextRole.Label => FontWeights.Medium,
        LayoutEditorTextRole.Value => FontWeights.SemiBold,
        LayoutEditorTextRole.Primary => FontWeights.Bold,
        _ => FontWeights.Normal,
    };

    public static Brush ToForegroundBrush(this LayoutEditorTextRole role) => role switch
    {
        LayoutEditorTextRole.Label => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A7B2C7")),
        LayoutEditorTextRole.Value => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2F5FA")),
        LayoutEditorTextRole.Primary => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")),
        _ => Brushes.White,
    };
}
