namespace SuperOverlay.Core.Layouts.Contracts;

public interface ILayoutItemDefinition
{
    string TypeId { get; }
    string DisplayName { get; }

    object CreateDefaultSettings();

    ILayoutItemPresenter CreatePresenter();
}
