using System.Globalization;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using SuperOverlay.Dashboards.Items.ShiftLeds;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor.WidgetSettings;

internal sealed class ShiftLedWidgetSettingsBinder : IEditorWidgetSettingsBinder
{
    public void Refresh(OverlayRuntimeSession session, EditorPropertiesPanelView view)
    {
        var shiftSettings = session.GetSelectedShiftLedSettings();
        if (shiftSettings is null)
        {
            view.ShiftLedSettingsPanel.Visibility = Visibility.Collapsed;
            view.ShiftLedCountTextBox.Text = string.Empty;
            view.ShiftUsePerLedColorsCheckBox.IsChecked = false;
            view.ShiftShowBackgroundCheckBox.IsChecked = false;
            view.ShiftBackgroundColorTextBox.Text = string.Empty;
            view.ShiftOffColorTextBox.Text = string.Empty;
            view.ShiftOnColorTextBox.Text = string.Empty;
            view.ShiftOnColorsTextBox.Text = string.Empty;
            return;
        }

        view.ShiftLedSettingsPanel.Visibility = Visibility.Visible;
        view.ShiftLedCountTextBox.Text = shiftSettings.LedCount.ToString(CultureInfo.CurrentCulture);
        view.ShiftUsePerLedColorsCheckBox.IsChecked = shiftSettings.UsePerLedColors;
        view.ShiftShowBackgroundCheckBox.IsChecked = shiftSettings.ShowBackground;
        view.ShiftBackgroundColorTextBox.Text = shiftSettings.BackgroundColor;
        view.ShiftOffColorTextBox.Text = shiftSettings.LedOffColor;
        view.ShiftOnColorTextBox.Text = shiftSettings.LedOnColor;
        view.ShiftOnColorsTextBox.Text = shiftSettings.LedOnColors;
    }

    public bool Apply(OverlayRuntimeSession session, EditorPropertiesPanelView view, Window owner)
    {
        if (view.ShiftLedSettingsPanel.Visibility != Visibility.Visible)
        {
            return false;
        }

        if (!int.TryParse(view.ShiftLedCountTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var ledCount) ||
            ledCount < 4 || ledCount > 24 ||
            !EditorColorText.IsValidColorValue(view.ShiftBackgroundColorTextBox.Text) ||
            !EditorColorText.IsValidColorValue(view.ShiftOffColorTextBox.Text) ||
            !EditorColorText.IsValidColorValue(view.ShiftOnColorTextBox.Text) ||
            !EditorColorText.AreValidColorListValues(view.ShiftOnColorsTextBox.Text))
        {
            EditorValidationMessages.ShowInvalidShiftLedProperties(owner);
            Refresh(session, view);
            return false;
        }

        var ledSettings = new ShiftLedDashboardSettings(
            LedCount: ledCount,
            ShowBackground: view.ShiftShowBackgroundCheckBox.IsChecked == true,
            BackgroundColor: EditorColorText.NormalizeColorText(view.ShiftBackgroundColorTextBox.Text, "#E61F2937"),
            LedOffColor: EditorColorText.NormalizeColorText(view.ShiftOffColorTextBox.Text, "#0F0F0F"),
            LedOnColor: EditorColorText.NormalizeColorText(view.ShiftOnColorTextBox.Text, "#FF1E00"),
            UsePerLedColors: view.ShiftUsePerLedColorsCheckBox.IsChecked == true,
            LedOnColors: EditorColorText.NormalizeColorListText(view.ShiftOnColorsTextBox.Text, ShiftLedDashboardSettingsDefaults.LegacyLikeLedOnColors),
            CornerTopLeft: EditorNumericText.TryParseNonNegativeDouble(view.CommonCornerTopLeftTextBox.Text, out var cornerTopLeft) ? cornerTopLeft : 0,
            CornerTopRight: EditorNumericText.TryParseNonNegativeDouble(view.CommonCornerTopRightTextBox.Text, out var cornerTopRight) ? cornerTopRight : 0,
            CornerBottomRight: EditorNumericText.TryParseNonNegativeDouble(view.CommonCornerBottomRightTextBox.Text, out var cornerBottomRight) ? cornerBottomRight : 0,
            CornerBottomLeft: EditorNumericText.TryParseNonNegativeDouble(view.CommonCornerBottomLeftTextBox.Text, out var cornerBottomLeft) ? cornerBottomLeft : 0);

        return session.UpdateSelectedShiftLedSettings(ledSettings);
    }
}
