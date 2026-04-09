namespace SuperOverlay.LayoutBuilder.Layout;

public static class LayoutItemFactory
{
    public static LayoutItemInstance Create(string typeId, object settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);
        ArgumentNullException.ThrowIfNull(settings);

        return new LayoutItemInstance(Guid.NewGuid(), typeId, settings);
    }
}
