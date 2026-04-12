using System.Windows.Controls;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorPropertiesPanelView
{
    public required TextBlock SelectedObjectText { get; init; }
    public required TextBlock SelectedObjectMetaText { get; init; }
    public required CheckBox ShowInRaceCheckBox { get; init; }

    public required TextBlock TextSizeHeader { get; init; }
    public required Grid TextSizeGrid { get; init; }
    public required ComboBox TopLeftTextSizeComboBox { get; init; }
    public required ComboBox TopRightTextSizeComboBox { get; init; }
    public required ComboBox CenterTextSizeComboBox { get; init; }
    public required ComboBox BottomLeftTextSizeComboBox { get; init; }
    public required ComboBox BottomRightTextSizeComboBox { get; init; }

    public required TextBlock TextRoleHeader { get; init; }
    public required Grid TextRoleGrid { get; init; }
    public required ComboBox TopLeftTextRoleComboBox { get; init; }
    public required ComboBox TopRightTextRoleComboBox { get; init; }
    public required ComboBox CenterTextRoleComboBox { get; init; }
    public required ComboBox BottomLeftTextRoleComboBox { get; init; }
    public required ComboBox BottomRightTextRoleComboBox { get; init; }
}
