namespace SuperOverlay.iRacing.Editor.WidgetSettings;

public sealed class EditorWidgetSettingsBinderRegistry
{
    private readonly IReadOnlyList<IEditorWidgetSettingsBinder> _binders;

    public EditorWidgetSettingsBinderRegistry(IEnumerable<IEditorWidgetSettingsBinder> binders)
    {
        if (binders is null)
        {
            throw new ArgumentNullException(nameof(binders));
        }

        _binders = binders.ToArray();
    }

    public IReadOnlyList<IEditorWidgetSettingsBinder> Binders => _binders;

    public static EditorWidgetSettingsBinderRegistry CreateDefault()
    {
        return new EditorWidgetSettingsBinderRegistry(
        [
            new ShiftLedWidgetSettingsBinder(),
            new DecorativePanelWidgetSettingsBinder()
        ]);
    }
}
