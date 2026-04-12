using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    private void SlotButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not LayoutEditorWidget widget)
        {
            return;
        }

        if (!Enum.TryParse<LayoutEditorSlotId>(element.Tag?.ToString(), out var slotId))
        {
            return;
        }

        if (!widget.IsSelected)
        {
            var preserveSelection = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)
                || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

            if (preserveSelection)
            {
                _selection.EnsureWidgetSelected(widget);
            }
            else
            {
                _selection.ToggleWidgetSelection(widget);
            }

            SyncEngineSelection();
            e.Handled = true;
            return;
        }

        var menu = _slotEditing.CreateSlotMenu(element, widget, slotId, AssignTextToSlot, AssignMetricToSlot, RefreshSelectionDetails);
        menu.IsOpen = true;
        e.Handled = true;
    }

    private void AssignTextToSlot(LayoutEditorWidget widget, LayoutEditorSlotId slotId)
    {
        _slotEditing.AssignTextToSlot(widget, slotId, RefreshSelectionDetails);
    }

    private void AssignMetricToSlot(LayoutEditorWidget widget, LayoutEditorSlotId slotId)
    {
        _slotEditing.AssignMetricToSlot(widget, slotId, RefreshSelectionDetails);
    }

    private void TextSizeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_state.PrimarySelectedWidget is null || sender is not ComboBox comboBox || comboBox.SelectedItem is not ComboBoxItem item)
        {
            return;
        }

        _slotEditing.ApplyTextSizeChange(
            _state.PrimarySelectedWidget,
            comboBox,
            TopLeftTextSizeComboBox,
            TopRightTextSizeComboBox,
            CenterTextSizeComboBox,
            BottomLeftTextSizeComboBox,
            BottomRightTextSizeComboBox);

        RefreshSelectionDetails();
    }


    private void TextRoleComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_state.PrimarySelectedWidget is null || sender is not ComboBox comboBox || comboBox.SelectedItem is not ComboBoxItem item)
        {
            return;
        }

        _slotEditing.ApplyTextRoleChange(
            _state.PrimarySelectedWidget,
            comboBox,
            TopLeftTextRoleComboBox,
            TopRightTextRoleComboBox,
            CenterTextRoleComboBox,
            BottomLeftTextRoleComboBox,
            BottomRightTextRoleComboBox);

        RefreshSelectionDetails();
    }

}
