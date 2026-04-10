using System.Globalization;
using System.Windows;
using WpfWindow = System.Windows.Window;
using SuperOverlay.iRacing.Editor.WidgetSettings;
using SuperOverlay.iRacing.Hosting;
using WpfTextBox = System.Windows.Controls.TextBox;
using WinFormsColorDialog = System.Windows.Forms.ColorDialog;
using WinFormsDialogResult = System.Windows.Forms.DialogResult;

namespace SuperOverlay.iRacing.Editor;

public sealed class EditorPropertiesPanelController
{
    private readonly WpfWindow _owner;
    private readonly EditorPropertiesPanelView _view;
    private readonly Func<OverlayRuntimeSession?> _getSession;
    private readonly EditorWidgetSettingsBinderRegistry _widgetSettingsBinderRegistry;

    public EditorPropertiesPanelController(WpfWindow owner, EditorPropertiesPanelView view, Func<OverlayRuntimeSession?> getSession, EditorWidgetSettingsBinderRegistry widgetSettingsBinderRegistry)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _getSession = getSession ?? throw new ArgumentNullException(nameof(getSession));
        _widgetSettingsBinderRegistry = widgetSettingsBinderRegistry ?? throw new ArgumentNullException(nameof(widgetSettingsBinderRegistry));
    }

    public void Refresh()
    {
        var session = _getSession();
        if (session is null)
        {
            _view.Clear();
            return;
        }

        var properties = session.GetSelectedItemProperties();
        if (properties is null)
        {
            _view.Clear();
            return;
        }

        _view.PropertiesPanel.IsEnabled = true;
        _view.SelectedWidgetNameTextBlock.Text = properties.DisplayName;
        _view.SelectedWidgetMetaTextBlock.Text = $"Type: {properties.TypeId} • Id: {properties.ItemId.ToString()[..8]}";
        _view.PropertiesHintTextBlock.Text = properties.SelectedCount > 1
            ? $"Editing primary selected item. Current selection: {properties.SelectedCount} items{(properties.IsGrouped ? " in a group" : string.Empty)}."
            : properties.IsGrouped
                ? "This item belongs to a linked group. Position and size edits apply to the primary selected item only."
                : "Edit base layout properties for the selected item.";

        _view.XTextBox.Text = EditorNumericText.FormatDouble(properties.X);
        _view.YTextBox.Text = EditorNumericText.FormatDouble(properties.Y);
        _view.WidthTextBox.Text = EditorNumericText.FormatDouble(properties.Width);
        _view.HeightTextBox.Text = EditorNumericText.FormatDouble(properties.Height);
        _view.ZIndexTextBox.Text = properties.ZIndex.ToString(CultureInfo.CurrentCulture);
        _view.LockedCheckBox.IsChecked = properties.IsLocked;

        EditorCornerText.Populate(_view, session.GetSelectedWidgetCornerSettings());

        foreach (var binder in _widgetSettingsBinderRegistry.Binders)
        {
            binder.Refresh(session, _view);
        }
    }

    public bool Apply()
    {
        var session = _getSession();
        if (session is null)
        {
            return false;
        }

        var current = session.GetSelectedItemProperties();
        if (current is null)
        {
            Refresh();
            return false;
        }

        if (!EditorNumericText.TryParseDouble(_view.XTextBox.Text, out var x) ||
            !EditorNumericText.TryParseDouble(_view.YTextBox.Text, out var y) ||
            !EditorNumericText.TryParseDouble(_view.WidthTextBox.Text, out var width) ||
            !EditorNumericText.TryParseDouble(_view.HeightTextBox.Text, out var height) ||
            !int.TryParse(_view.ZIndexTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var zIndex))
        {
            EditorValidationMessages.ShowInvalidBaseProperties(_owner);
            return false;
        }

        var anyChanged = false;
        if (session.UpdateSelectedItemProperties(x, y, width, height, zIndex, _view.LockedCheckBox.IsChecked == true))
        {
            anyChanged = true;
        }

        if (_view.CommonCornerSettingsPanel.Visibility == Visibility.Visible)
        {
            if (!EditorCornerText.TryRead(_view, out var commonCorners))
            {
                EditorValidationMessages.ShowInvalidCornerProperties(_owner);
                Refresh();
                return false;
            }

            if (session.UpdateSelectedWidgetCornerSettings(commonCorners.TopLeft, commonCorners.TopRight, commonCorners.BottomRight, commonCorners.BottomLeft))
            {
                anyChanged = true;
            }
        }

        foreach (var binder in _widgetSettingsBinderRegistry.Binders)
        {
            if (binder.Apply(session, _view, _owner))
            {
                anyChanged = true;
            }
        }

        Refresh();
        return anyChanged;
    }

    public void PickShiftBackgroundColor() => PickColorInto(_view.ShiftBackgroundColorTextBox, "#E61F2937");
    public void PickShiftOffColor() => PickColorInto(_view.ShiftOffColorTextBox, "#0F0F0F");
    public void PickShiftOnColor() => PickColorInto(_view.ShiftOnColorTextBox, "#FF1E00");
    public void PickDecorativeBackgroundColor() => PickColorInto(_view.DecorativeBackgroundColorTextBox, "#CC1F2937");

    private void PickColorInto(WpfTextBox targetTextBox, string fallback)
    {
        var initial = EditorColorText.ParseMediaColor(targetTextBox.Text, fallback);
        using var dialog = new WinFormsColorDialog
        {
            FullOpen = true,
            AnyColor = true,
            Color = System.Drawing.Color.FromArgb(initial.A, initial.R, initial.G, initial.B)
        };

        if (dialog.ShowDialog() != WinFormsDialogResult.OK)
        {
            return;
        }

        targetTextBox.Text = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
    }

}
