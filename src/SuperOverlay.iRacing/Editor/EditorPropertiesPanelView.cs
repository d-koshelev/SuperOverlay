using System.Windows;

namespace SuperOverlay.iRacing.Editor;

public sealed record EditorPropertiesPanelView(
    System.Windows.Controls.StackPanel PropertiesPanel,
    System.Windows.Controls.TextBlock SelectedWidgetNameTextBlock,
    System.Windows.Controls.TextBlock SelectedWidgetMetaTextBlock,
    System.Windows.Controls.TextBlock PropertiesHintTextBlock,
    System.Windows.Controls.TextBox XTextBox,
    System.Windows.Controls.TextBox YTextBox,
    System.Windows.Controls.TextBox WidthTextBox,
    System.Windows.Controls.TextBox HeightTextBox,
    System.Windows.Controls.TextBox ZIndexTextBox,
    System.Windows.Controls.CheckBox LockedCheckBox,
    System.Windows.Controls.StackPanel CommonCornerSettingsPanel,
    System.Windows.Controls.TextBox CommonCornerTopLeftTextBox,
    System.Windows.Controls.TextBox CommonCornerTopRightTextBox,
    System.Windows.Controls.TextBox CommonCornerBottomRightTextBox,
    System.Windows.Controls.TextBox CommonCornerBottomLeftTextBox,
    System.Windows.Controls.StackPanel ShiftLedSettingsPanel,
    System.Windows.Controls.TextBox ShiftLedCountTextBox,
    System.Windows.Controls.CheckBox ShiftUsePerLedColorsCheckBox,
    System.Windows.Controls.CheckBox ShiftShowBackgroundCheckBox,
    System.Windows.Controls.TextBox ShiftBackgroundColorTextBox,
    System.Windows.Controls.TextBox ShiftOffColorTextBox,
    System.Windows.Controls.TextBox ShiftOnColorTextBox,
    System.Windows.Controls.TextBox ShiftOnColorsTextBox,
    System.Windows.Controls.StackPanel DecorativePanelSettingsPanel,
    System.Windows.Controls.TextBox DecorativeBackgroundColorTextBox,
    System.Windows.Controls.TextBox DecorativeOpacityTextBox)
{
    public void Clear()
    {
        PropertiesPanel.IsEnabled = false;
        SelectedWidgetNameTextBlock.Text = "No selection";
        SelectedWidgetMetaTextBlock.Text = string.Empty;
        PropertiesHintTextBlock.Text = "Select an item to edit its properties. Multi-select keeps one primary item here.";
        XTextBox.Text = string.Empty;
        YTextBox.Text = string.Empty;
        WidthTextBox.Text = string.Empty;
        HeightTextBox.Text = string.Empty;
        ZIndexTextBox.Text = string.Empty;
        LockedCheckBox.IsChecked = false;

        CommonCornerSettingsPanel.Visibility = Visibility.Collapsed;
        CommonCornerTopLeftTextBox.Text = string.Empty;
        CommonCornerTopRightTextBox.Text = string.Empty;
        CommonCornerBottomRightTextBox.Text = string.Empty;
        CommonCornerBottomLeftTextBox.Text = string.Empty;

        ShiftLedSettingsPanel.Visibility = Visibility.Collapsed;
        ShiftLedCountTextBox.Text = string.Empty;
        ShiftUsePerLedColorsCheckBox.IsChecked = false;
        ShiftShowBackgroundCheckBox.IsChecked = false;
        ShiftBackgroundColorTextBox.Text = string.Empty;
        ShiftOffColorTextBox.Text = string.Empty;
        ShiftOnColorTextBox.Text = string.Empty;
        ShiftOnColorsTextBox.Text = string.Empty;

        DecorativePanelSettingsPanel.Visibility = Visibility.Collapsed;
        DecorativeBackgroundColorTextBox.Text = string.Empty;
        DecorativeOpacityTextBox.Text = string.Empty;
    }
}
