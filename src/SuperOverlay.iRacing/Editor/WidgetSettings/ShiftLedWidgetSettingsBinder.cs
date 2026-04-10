using System.Globalization;
using System.Windows;
using WpfWindow = System.Windows.Window;
using SuperOverlay.Dashboards.Items.ShiftLeds;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor.WidgetSettings;

internal sealed class ShiftLedWidgetSettingsBinder : EditorWidgetSettingsBinderBase
{
    protected override bool TryRefreshCore(OverlayRuntimeSession session, EditorPropertiesPanelView view)
    {
        var shiftSettings = session.GetSelectedShiftLedSettings();
        if (shiftSettings is null)
        {
            return false;
        }

        view.ShiftLedCountTextBox.Text = shiftSettings.LedCount.ToString(CultureInfo.CurrentCulture);
        view.ShiftUsePerLedColorsCheckBox.IsChecked = shiftSettings.UsePerLedColors;
        view.ShiftShowBackgroundCheckBox.IsChecked = shiftSettings.ShowBackground;
        view.ShiftBackgroundColorTextBox.Text = shiftSettings.BackgroundColor;
        view.ShiftOffColorTextBox.Text = shiftSettings.LedOffColor;
        view.ShiftOnColorTextBox.Text = shiftSettings.LedOnColor;
        view.ShiftOnColorsTextBox.Text = shiftSettings.LedOnColors;
        return true;
    }

    protected override bool ApplyCore(OverlayRuntimeSession session, EditorPropertiesPanelView view, WpfWindow owner)
    {
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

        var corners = EditorCornerText.ReadOrDefault(view);

        var ledSettings = new ShiftLedDashboardSettings(
            LedCount: ledCount,
            ShowBackground: view.ShiftShowBackgroundCheckBox.IsChecked == true,
            BackgroundColor: EditorColorText.NormalizeColorText(view.ShiftBackgroundColorTextBox.Text, "#E61F2937"),
            LedOffColor: EditorColorText.NormalizeColorText(view.ShiftOffColorTextBox.Text, "#0F0F0F"),
            LedOnColor: EditorColorText.NormalizeColorText(view.ShiftOnColorTextBox.Text, "#FF1E00"),
            UsePerLedColors: view.ShiftUsePerLedColorsCheckBox.IsChecked == true,
            LedOnColors: EditorColorText.NormalizeColorListText(view.ShiftOnColorsTextBox.Text, ShiftLedDashboardSettingsDefaults.LegacyLikeLedOnColors),
            CornerTopLeft: corners.TopLeft,
            CornerTopRight: corners.TopRight,
            CornerBottomRight: corners.BottomRight,
            CornerBottomLeft: corners.BottomLeft);

        return session.UpdateSelectedShiftLedSettings(ledSettings);
    }

    protected override void Reset(EditorPropertiesPanelView view)
    {
        view.ShiftLedCountTextBox.Text = string.Empty;
        view.ShiftUsePerLedColorsCheckBox.IsChecked = false;
        view.ShiftShowBackgroundCheckBox.IsChecked = false;
        view.ShiftBackgroundColorTextBox.Text = string.Empty;
        view.ShiftOffColorTextBox.Text = string.Empty;
        view.ShiftOnColorTextBox.Text = string.Empty;
        view.ShiftOnColorsTextBox.Text = string.Empty;
    }

    protected override bool IsSectionVisible(EditorPropertiesPanelView view)
    {
        return view.ShiftLedSettingsPanel.Visibility == Visibility.Visible;
    }

    protected override void SetSectionVisible(EditorPropertiesPanelView view, bool isVisible)
    {
        SetPanelVisibility(view.ShiftLedSettingsPanel, isVisible);
    }
}
