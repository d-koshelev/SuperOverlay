namespace SuperOverlay.LayoutBuilder.Contracts;

public interface ILayoutItemDefinition
{
    string TypeId { get; }
    string DisplayName { get; }

    object CreateDefaultSettings();

    ILayoutItemPresenter CreatePresenter();
}
