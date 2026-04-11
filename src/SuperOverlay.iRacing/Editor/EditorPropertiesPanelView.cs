namespace SuperOverlay.iRacing.Editor;

public sealed class EditorPropertiesPanelView
{
    public System.Windows.Controls.Panel PropertiesPanel { get; }
    public System.Windows.Controls.TextBlock SelectedWidgetNameTextBlock { get; }
    public System.Windows.Controls.TextBlock SelectedWidgetMetaTextBlock { get; }
    public System.Windows.Controls.TextBlock PropertiesHintTextBlock { get; }
    public System.Windows.Controls.TextBox XTextBox { get; }
    public System.Windows.Controls.TextBox YTextBox { get; }
    public System.Windows.Controls.TextBox WidthTextBox { get; }
    public System.Windows.Controls.TextBox HeightTextBox { get; }
    public System.Windows.Controls.TextBox ZIndexTextBox { get; }
    public System.Windows.Controls.CheckBox LockedCheckBox { get; }

    public System.Windows.FrameworkElement CommonCornerSettingsPanel { get; }
    public System.Windows.Controls.TextBox CommonCornerTopLeftTextBox { get; }
    public System.Windows.Controls.TextBox CommonCornerTopRightTextBox { get; }
    public System.Windows.Controls.TextBox CommonCornerBottomRightTextBox { get; }
    public System.Windows.Controls.TextBox CommonCornerBottomLeftTextBox { get; }

    public System.Windows.FrameworkElement ShiftLedSettingsPanel { get; }
    public System.Windows.Controls.TextBox ShiftLedCountTextBox { get; }
    public System.Windows.Controls.CheckBox ShiftUsePerLedColorsCheckBox { get; }
    public System.Windows.Controls.CheckBox ShiftShowBackgroundCheckBox { get; }
    public System.Windows.Controls.TextBox ShiftBackgroundColorTextBox { get; }
    public System.Windows.Controls.TextBox ShiftOffColorTextBox { get; }
    public System.Windows.Controls.TextBox ShiftOnColorTextBox { get; }
    public System.Windows.Controls.TextBox ShiftOnColorsTextBox { get; }

    public System.Windows.FrameworkElement DecorativePanelSettingsPanel { get; }
    public System.Windows.Controls.TextBox DecorativeBackgroundColorTextBox { get; }
    public System.Windows.Controls.TextBox DecorativeOpacityTextBox { get; }

    public EditorPropertiesPanelView(
        System.Windows.Controls.Panel propertiesPanel,
        System.Windows.Controls.TextBlock selectedWidgetNameTextBlock,
        System.Windows.Controls.TextBlock selectedWidgetMetaTextBlock,
        System.Windows.Controls.TextBlock propertiesHintTextBlock,
        System.Windows.Controls.TextBox xTextBox,
        System.Windows.Controls.TextBox yTextBox,
        System.Windows.Controls.TextBox widthTextBox,
        System.Windows.Controls.TextBox heightTextBox,
        System.Windows.Controls.TextBox zIndexTextBox,
        System.Windows.Controls.CheckBox lockedCheckBox,
        System.Windows.FrameworkElement commonCornerSettingsPanel,
        System.Windows.Controls.TextBox commonCornerTopLeftTextBox,
        System.Windows.Controls.TextBox commonCornerTopRightTextBox,
        System.Windows.Controls.TextBox commonCornerBottomRightTextBox,
        System.Windows.Controls.TextBox commonCornerBottomLeftTextBox,
        System.Windows.FrameworkElement shiftLedSettingsPanel,
        System.Windows.Controls.TextBox shiftLedCountTextBox,
        System.Windows.Controls.CheckBox shiftUsePerLedColorsCheckBox,
        System.Windows.Controls.CheckBox shiftShowBackgroundCheckBox,
        System.Windows.Controls.TextBox shiftBackgroundColorTextBox,
        System.Windows.Controls.TextBox shiftOffColorTextBox,
        System.Windows.Controls.TextBox shiftOnColorTextBox,
        System.Windows.Controls.TextBox shiftOnColorsTextBox,
        System.Windows.FrameworkElement decorativePanelSettingsPanel,
        System.Windows.Controls.TextBox decorativeBackgroundColorTextBox,
        System.Windows.Controls.TextBox decorativeOpacityTextBox)
    {
        PropertiesPanel = propertiesPanel;
        SelectedWidgetNameTextBlock = selectedWidgetNameTextBlock;
        SelectedWidgetMetaTextBlock = selectedWidgetMetaTextBlock;
        PropertiesHintTextBlock = propertiesHintTextBlock;
        XTextBox = xTextBox;
        YTextBox = yTextBox;
        WidthTextBox = widthTextBox;
        HeightTextBox = heightTextBox;
        ZIndexTextBox = zIndexTextBox;
        LockedCheckBox = lockedCheckBox;

        CommonCornerSettingsPanel = commonCornerSettingsPanel;
        CommonCornerTopLeftTextBox = commonCornerTopLeftTextBox;
        CommonCornerTopRightTextBox = commonCornerTopRightTextBox;
        CommonCornerBottomRightTextBox = commonCornerBottomRightTextBox;
        CommonCornerBottomLeftTextBox = commonCornerBottomLeftTextBox;

        ShiftLedSettingsPanel = shiftLedSettingsPanel;
        ShiftLedCountTextBox = shiftLedCountTextBox;
        ShiftUsePerLedColorsCheckBox = shiftUsePerLedColorsCheckBox;
        ShiftShowBackgroundCheckBox = shiftShowBackgroundCheckBox;
        ShiftBackgroundColorTextBox = shiftBackgroundColorTextBox;
        ShiftOffColorTextBox = shiftOffColorTextBox;
        ShiftOnColorTextBox = shiftOnColorTextBox;
        ShiftOnColorsTextBox = shiftOnColorsTextBox;

        DecorativePanelSettingsPanel = decorativePanelSettingsPanel;
        DecorativeBackgroundColorTextBox = decorativeBackgroundColorTextBox;
        DecorativeOpacityTextBox = decorativeOpacityTextBox;
    }

    public void Clear()
    {
        PropertiesPanel.IsEnabled = false;

        SelectedWidgetNameTextBlock.Text = string.Empty;
        SelectedWidgetMetaTextBlock.Text = string.Empty;

        XTextBox.Text = string.Empty;
        YTextBox.Text = string.Empty;
        WidthTextBox.Text = string.Empty;
        HeightTextBox.Text = string.Empty;
        ZIndexTextBox.Text = string.Empty;
        LockedCheckBox.IsChecked = false;

        CommonCornerTopLeftTextBox.Text = string.Empty;
        CommonCornerTopRightTextBox.Text = string.Empty;
        CommonCornerBottomRightTextBox.Text = string.Empty;
        CommonCornerBottomLeftTextBox.Text = string.Empty;

        ShiftLedCountTextBox.Text = string.Empty;
        ShiftUsePerLedColorsCheckBox.IsChecked = false;
        ShiftShowBackgroundCheckBox.IsChecked = false;
        ShiftBackgroundColorTextBox.Text = string.Empty;
        ShiftOffColorTextBox.Text = string.Empty;
        ShiftOnColorTextBox.Text = string.Empty;
        ShiftOnColorsTextBox.Text = string.Empty;

        DecorativeBackgroundColorTextBox.Text = string.Empty;
        DecorativeOpacityTextBox.Text = string.Empty;

        CommonCornerSettingsPanel.Visibility = System.Windows.Visibility.Collapsed;
        ShiftLedSettingsPanel.Visibility = System.Windows.Visibility.Collapsed;
        DecorativePanelSettingsPanel.Visibility = System.Windows.Visibility.Collapsed;
    }
}
