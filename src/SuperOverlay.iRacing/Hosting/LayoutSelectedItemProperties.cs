namespace SuperOverlay.iRacing.Hosting;

public sealed record LayoutSelectedItemProperties(
    Guid ItemId,
    string TypeId,
    string DisplayName,
    double X,
    double Y,
    double Width,
    double Height,
    int ZIndex,
    bool IsLocked,
    int SelectedCount,
    bool IsGrouped);
