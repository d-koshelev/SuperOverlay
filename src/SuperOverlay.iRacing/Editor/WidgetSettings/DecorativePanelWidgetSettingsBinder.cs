using System.Windows;
using SuperOverlay.Dashboards.Items.DecorativePanel;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor.WidgetSettings;

internal sealed class DecorativePanelWidgetSettingsBinder : IEditorWidgetSettingsBinder
{
    public void Refresh(OverlayRuntimeSession session, EditorPropertiesPanelView view)
    {
        var settings = session.GetSelectedDecorativePanelSettings();
        if (settings is null)
        {
            view.DecorativePanelSettingsPanel.Visibility = Visibility.Collapsed;
            view.DecorativeBackgroundColorTextBox.Text = string.Empty;
            view.DecorativeOpacityTextBox.Text = string.Empty;
            return;
        }

        view.DecorativePanelSettingsPanel.Visibility = Visibility.Visible;
        view.DecorativeBackgroundColorTextBox.Text = settings.BackgroundColor;
        view.DecorativeOpacityTextBox.Text = EditorNumericText.FormatDouble(settings.Opacity);
    }

    public bool Apply(OverlayRuntimeSession session, EditorPropertiesPanelView view, Window owner)
    {
        if (view.DecorativePanelSettingsPanel.Visibility != Visibility.Visible)
        {
            return false;
        }

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
}
