using System.Windows;
using System.Windows.Controls;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    private bool _isUpdatingRawBindingUi;

    private void RawBindingSourceComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingRawBindingUi)
        {
            return;
        }

        var widget = SelectedWidgets.Count == 1 ? SelectedWidgets[0] : null;
        if (widget is null)
        {
            return;
        }

        var source = (RawBindingSourceComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (string.IsNullOrWhiteSpace(source))
        {
            return;
        }

        if (string.Equals(widget.RawBindingSource, source, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        widget.RawBindingSource = source;

        _isUpdatingRawBindingUi = true;
        try
        {
            var fields = LayoutEditorRawFieldCatalog.GetFields(source);
            RawBindingFieldComboBox.ItemsSource = fields;
            var nextField = fields.FirstOrDefault() ?? string.Empty;
            widget.RawBindingFieldPath = nextField;
            widget.CenterContent = LayoutEditorRawFieldCatalog.GetPreviewValue(source, nextField);
            RawBindingFieldComboBox.SelectedItem = nextField;
            RawBindingFieldComboBox.Text = nextField;
        }
        finally
        {
            _isUpdatingRawBindingUi = false;
        }

        RefreshSelectionDetails();
    }

    private void RawBindingFieldComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingRawBindingUi)
        {
            return;
        }

        ApplyRawBindingFieldFromUi();
    }

    private void RawBindingFieldComboBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingRawBindingUi)
        {
            return;
        }

        ApplyRawBindingFieldFromUi();
    }

    private void ApplyRawBindingFieldFromUi()
    {
        var widget = SelectedWidgets.Count == 1 ? SelectedWidgets[0] : null;
        if (widget is null)
        {
            return;
        }

        var fieldPath = RawBindingFieldComboBox.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(fieldPath))
        {
            fieldPath = RawBindingFieldComboBox.Text;
        }

        if (string.IsNullOrWhiteSpace(fieldPath))
        {
            return;
        }

        widget.RawBindingFieldPath = fieldPath;
        widget.CenterContent = LayoutEditorRawFieldCatalog.GetPreviewValue(widget.RawBindingSource, fieldPath);
        RefreshSelectionDetails();
    }
}
