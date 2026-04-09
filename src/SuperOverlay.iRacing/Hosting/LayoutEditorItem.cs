namespace SuperOverlay.iRacing.Hosting;

public sealed record LayoutEditorItem(
    Guid Id,
    string TypeId,
    string DisplayName)
{
    public string DisplayText => $"{DisplayName} ({Id.ToString()[..8]})";
}
