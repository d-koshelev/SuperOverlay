using System.Windows;
using WpfWindow = System.Windows.Window;
using SuperOverlay.Dashboards.Items.DecorativePanel;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor.WidgetSettings;

internal sealed class DecorativePanelWidgetSettingsBinder : EditorWidgetSettingsBinderBase
{
    protected override bool TryRefreshCore(OverlayRuntimeSession session, EditorPropertiesPanelView view)
    {
        var settings = session.GetSelectedDecorativePanelSettings();
        if (settings is null)
        {
            return false;
        }

        view.DecorativeBackgroundColorTextBox.Text = settings.BackgroundColor;
        view.DecorativeOpacityTextBox.Text = EditorNumericText.FormatDouble(settings.Opacity);
        return true;
    }

    protected override bool ApplyCore(OverlayRuntimeSession session, EditorPropertiesPanelView view, WpfWindow owner)
    {
        if (!EditorColorText.IsValidColorValue(view.DecorativeBackgroundColorTextBox.Text))
        {
            EditorValidationMessages.ShowInvalidDecorativePanelColor(owner);
            Refresh(session, view);
            return false;
        }

        if (!EditorNumericText.TryParseUnitInterval(view.DecorativeOpacityTextBox.Text, out var opacity))
        {
            EditorValidationMessages.ShowInvalidDecorativePanelOpacity(owner);
            Refresh(session, view);
            return false;
        }

        var current = session.GetSelectedDecorativePanelSettings();
        if (current is null)
        {
            return false;
        }

        var updated = current with
        {
            BackgroundColor = EditorColorText.NormalizeColorText(view.DecorativeBackgroundColorTextBox.Text, "#CC1F2937"),
            Opacity = opacity
        };

        return session.UpdateSelectedDecorativePanelSettings(updated);
    }

    protected override void Reset(EditorPropertiesPanelView view)
    {
        view.DecorativeBackgroundColorTextBox.Text = string.Empty;
        view.DecorativeOpacityTextBox.Text = string.Empty;
    }

    protected override bool IsSectionVisible(EditorPropertiesPanelView view)
    {
        return view.DecorativePanelSettingsPanel.Visibility == Visibility.Visible;
    }

    protected override void SetSectionVisible(EditorPropertiesPanelView view, bool isVisible)
    {
        SetPanelVisibility(view.DecorativePanelSettingsPanel, isVisible);
    }
}
