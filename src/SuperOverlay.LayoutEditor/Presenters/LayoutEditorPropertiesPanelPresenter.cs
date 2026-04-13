using System.Windows;
using System.Windows.Controls;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorPropertiesPanelPresenter
{
    private readonly LayoutEditorPropertiesPanelView _view;

    public LayoutEditorPropertiesPanelPresenter(LayoutEditorPropertiesPanelView view)
    {
        _view = view;
    }

    public void Refresh(IReadOnlyList<LayoutEditorWidget> selected, LayoutEditorWidget? primaryWidget)
    {
        if (selected.Count == 0)
        {
            _view.SelectedObjectText.Text = "No widget selected";
            _view.SelectedObjectMetaText.Text = LayoutEditorPropertiesFormatter.FormatSelectionSummary(selected, null);
            _view.SelectedObjectMetaText.Visibility = Visibility.Visible;
            _view.ShowInRaceCheckBox.IsEnabled = false;
            _view.ShowInRaceCheckBox.IsChecked = false;
            ShowRawBindingConfiguration(false);
            ShowTextConfiguration(false);
            return;
        }

        if (selected.Count == 1 && primaryWidget is not null)
        {
            _view.SelectedObjectText.Text = "Selected widget";
            _view.SelectedObjectMetaText.Text = LayoutEditorPropertiesFormatter.FormatSelectionSummary(selected, primaryWidget);
            _view.SelectedObjectMetaText.Visibility = Visibility.Visible;
            _view.ShowInRaceCheckBox.IsEnabled = true;
            _view.ShowInRaceCheckBox.IsChecked = primaryWidget.ShowInRace;
            ShowRawBindingConfiguration(true);
            ApplyRawBindingSelection(primaryWidget);
            ShowTextConfiguration(true);
            ApplyTextSizeSelection(primaryWidget);
            ApplyTextRoleSelection(primaryWidget);
            return;
        }

        _view.SelectedObjectText.Text = $"{selected.Count} widgets selected";
        _view.SelectedObjectMetaText.Text = LayoutEditorPropertiesFormatter.FormatSelectionSummary(selected, primaryWidget);
        _view.SelectedObjectMetaText.Visibility = Visibility.Visible;
        _view.ShowInRaceCheckBox.IsEnabled = true;
        _view.ShowInRaceCheckBox.IsChecked = selected.All(x => x.ShowInRace);
        ShowRawBindingConfiguration(false);
        ShowTextConfiguration(false);
    }

    private void ShowRawBindingConfiguration(bool isVisible)
    {
        var visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        _view.RawBindingHeader.Visibility = visibility;
        _view.RawBindingGrid.Visibility = visibility;
    }

    private void ApplyRawBindingSelection(LayoutEditorWidget widget)
    {
        _view.RawBindingSourceComboBox.SelectedIndex = string.Equals(widget.RawBindingSource, "SessionInfo", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

        var fields = LayoutEditorRawFieldCatalog.GetFields(widget.RawBindingSource);
        _view.RawBindingFieldComboBox.ItemsSource = fields;
        _view.RawBindingFieldComboBox.SelectedItem = fields.FirstOrDefault(x => string.Equals(x, widget.RawBindingFieldPath, StringComparison.OrdinalIgnoreCase));
        if (_view.RawBindingFieldComboBox.SelectedItem is null)
        {
            _view.RawBindingFieldComboBox.Text = widget.RawBindingFieldPath;
        }
    }

    private void ShowTextConfiguration(bool isVisible)
    {
        var visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        _view.TextSizeHeader.Visibility = visibility;
        _view.TextSizeGrid.Visibility = visibility;
        _view.TextRoleHeader.Visibility = visibility;
        _view.TextRoleGrid.Visibility = visibility;
    }

    private void ApplyTextSizeSelection(LayoutEditorWidget widget)
    {
        LayoutEditorSlotEditingService.ApplyPresetSelectionToUi(_view.TopLeftTextSizeComboBox, widget.TopLeftTextSizePreset);
        LayoutEditorSlotEditingService.ApplyPresetSelectionToUi(_view.TopRightTextSizeComboBox, widget.TopRightTextSizePreset);
        LayoutEditorSlotEditingService.ApplyPresetSelectionToUi(_view.CenterTextSizeComboBox, widget.CenterTextSizePreset);
        LayoutEditorSlotEditingService.ApplyPresetSelectionToUi(_view.BottomLeftTextSizeComboBox, widget.BottomLeftTextSizePreset);
        LayoutEditorSlotEditingService.ApplyPresetSelectionToUi(_view.BottomRightTextSizeComboBox, widget.BottomRightTextSizePreset);
    }

    private void ApplyTextRoleSelection(LayoutEditorWidget widget)
    {
        LayoutEditorSlotEditingService.ApplyRoleSelectionToUi(_view.TopLeftTextRoleComboBox, widget.TopLeftTextRole);
        LayoutEditorSlotEditingService.ApplyRoleSelectionToUi(_view.TopRightTextRoleComboBox, widget.TopRightTextRole);
        LayoutEditorSlotEditingService.ApplyRoleSelectionToUi(_view.CenterTextRoleComboBox, widget.CenterTextRole);
        LayoutEditorSlotEditingService.ApplyRoleSelectionToUi(_view.BottomLeftTextRoleComboBox, widget.BottomLeftTextRole);
        LayoutEditorSlotEditingService.ApplyRoleSelectionToUi(_view.BottomRightTextRoleComboBox, widget.BottomRightTextRole);
    }
}
