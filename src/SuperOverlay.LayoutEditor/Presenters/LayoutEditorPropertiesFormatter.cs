namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorPropertiesFormatter
{
    public static string FormatSelectionSummary(
        IReadOnlyList<LayoutEditorWidget> selected,
        LayoutEditorWidget? primary)
    {
        if (selected.Count == 0)
        {
            return
                "No widget selected.\n" +
                "Right click on canvas for menu.\n" +
                "Use Add Widget or Load Preset.";
        }

        if (selected.Count == 1 && primary is not null)
        {
            return
                $"Position: {primary.X:0}, {primary.Y:0}\n" +
                $"Size: {primary.Width:0} x {primary.Height:0}\n" +
                $"Visible in race: {(primary.ShowInRace ? "Yes" : "No")}\n" +
                $"Locked: {(primary.IsLocked ? "Yes" : "No")}\n" +
                $"Grouped: {(primary.IsGrouped ? primary.GroupId!.Value.ToString()[..8] : "No")}\n" +
                $"Slots: C={SlotLabel(primary.CenterContent)}, " +
                $"TL={SlotLabel(primary.TopLeftContent)}, " +
                $"TR={SlotLabel(primary.TopRightContent)}, " +
                $"BL={SlotLabel(primary.BottomLeftContent)}, " +
                $"BR={SlotLabel(primary.BottomRightContent)}\n" +
                $"Sizes: C={primary.CenterTextSizePreset}, " +
                $"TL={primary.TopLeftTextSizePreset}, " +
                $"TR={primary.TopRightTextSizePreset}, " +
                $"BL={primary.BottomLeftTextSizePreset}, " +
                $"BR={primary.BottomRightTextSizePreset}\n" +
                $"Roles: C={primary.CenterTextRole}, " +
                $"TL={primary.TopLeftTextRole}, " +
                $"TR={primary.TopRightTextRole}, " +
                $"BL={primary.BottomLeftTextRole}, " +
                $"BR={primary.BottomRightTextRole}";
        }

        var groupedCount = selected.Count(x => x.IsGrouped);
        var lockedCount = selected.Count(x => x.IsLocked);
        var allVisibleInRace = selected.All(x => x.ShowInRace);

        return
            $"Grouped widgets: {groupedCount}/{selected.Count}\n" +
            $"Locked widgets: {lockedCount}/{selected.Count}\n" +
            $"Visible in race: {(allVisibleInRace ? "Yes" : "Mixed / No")}\n" +
            "Right click selection for Group / Save as Preset.";
    }

    private static string SlotLabel(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Empty" : value;
    }
}
