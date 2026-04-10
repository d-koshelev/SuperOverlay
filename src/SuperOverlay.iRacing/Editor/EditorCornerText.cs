using System.Windows;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor;

internal static class EditorCornerText
{
    public static void Populate(EditorPropertiesPanelView view, WidgetCornerSettings? settings)
    {
        if (settings is null)
        {
            view.CommonCornerSettingsPanel.Visibility = Visibility.Collapsed;
            view.CommonCornerTopLeftTextBox.Text = string.Empty;
            view.CommonCornerTopRightTextBox.Text = string.Empty;
            view.CommonCornerBottomRightTextBox.Text = string.Empty;
            view.CommonCornerBottomLeftTextBox.Text = string.Empty;
            return;
        }

        view.CommonCornerSettingsPanel.Visibility = Visibility.Visible;
        view.CommonCornerTopLeftTextBox.Text = EditorNumericText.FormatDouble(settings.TopLeft);
        view.CommonCornerTopRightTextBox.Text = EditorNumericText.FormatDouble(settings.TopRight);
        view.CommonCornerBottomRightTextBox.Text = EditorNumericText.FormatDouble(settings.BottomRight);
        view.CommonCornerBottomLeftTextBox.Text = EditorNumericText.FormatDouble(settings.BottomLeft);
    }

    public static bool TryRead(EditorPropertiesPanelView view, out WidgetCornerSettings settings)
    {
        if (!EditorNumericText.TryParseNonNegativeDouble(view.CommonCornerTopLeftTextBox.Text, out var topLeft) ||
            !EditorNumericText.TryParseNonNegativeDouble(view.CommonCornerTopRightTextBox.Text, out var topRight) ||
            !EditorNumericText.TryParseNonNegativeDouble(view.CommonCornerBottomRightTextBox.Text, out var bottomRight) ||
            !EditorNumericText.TryParseNonNegativeDouble(view.CommonCornerBottomLeftTextBox.Text, out var bottomLeft))
        {
            settings = new WidgetCornerSettings(0, 0, 0, 0);
            return false;
        }

        settings = new WidgetCornerSettings(topLeft, topRight, bottomRight, bottomLeft);
        return true;
    }

    public static WidgetCornerSettings ReadOrDefault(EditorPropertiesPanelView view)
    {
        return TryRead(view, out var settings)
            ? settings
            : new WidgetCornerSettings(0, 0, 0, 0);
    }
}
