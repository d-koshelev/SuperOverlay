namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorPropertiesPanelViewFactory
{
    public static LayoutEditorPropertiesPanelView Create(LayoutEditorWindow window)
    {
        return new LayoutEditorPropertiesPanelView
        {
            SelectedObjectText = window.SelectedObjectTextWidget,
            SelectedObjectMetaText = window.SelectedObjectMetaTextWidget,
            ShowInRaceCheckBox = window.ShowInRaceLayoutCheckBox,
            RawBindingHeader = window.RawBindingHeaderTextBlock,
            RawBindingGrid = window.RawBindingGrid,
            RawBindingSourceComboBox = window.RawBindingSourceComboBox,
            RawBindingFieldComboBox = window.RawBindingFieldComboBox,
            TextSizeHeader = window.TextSizeHeaderTextBlock,
            TextSizeGrid = window.TextSizeGrid,
            TopLeftTextSizeComboBox = window.TopLeftTextSizeComboBox,
            TopRightTextSizeComboBox = window.TopRightTextSizeComboBox,
            CenterTextSizeComboBox = window.CenterTextSizeComboBox,
            BottomLeftTextSizeComboBox = window.BottomLeftTextSizeComboBox,
            BottomRightTextSizeComboBox = window.BottomRightTextSizeComboBox,
            TextRoleHeader = window.TextRoleHeaderTextBlock,
            TextRoleGrid = window.TextRoleGrid,
            TopLeftTextRoleComboBox = window.TopLeftTextRoleComboBox,
            TopRightTextRoleComboBox = window.TopRightTextRoleComboBox,
            CenterTextRoleComboBox = window.CenterTextRoleComboBox,
            BottomLeftTextRoleComboBox = window.BottomLeftTextRoleComboBox,
            BottomRightTextRoleComboBox = window.BottomRightTextRoleComboBox
        };
    }
}
