namespace SuperOverlay.iRacing.Hosting;

public sealed record LayoutEditorItem(
    Guid Id,
    string TypeId,
    string DisplayName,
    bool IsGrouped,
    bool IsLocked)
{
    public string DisplayText
    {
        get
        {
            var suffix = string.Empty;
            if (IsGrouped)
            {
                suffix += " [G]";
            }

            if (IsLocked)
            {
                suffix += " [L]";
            }

            return $"{DisplayName} ({Id.ToString()[..8]}){suffix}";
        }
    }
}
