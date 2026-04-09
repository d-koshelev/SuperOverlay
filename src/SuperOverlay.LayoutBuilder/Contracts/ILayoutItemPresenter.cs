namespace SuperOverlay.LayoutBuilder.Contracts;

public interface ILayoutItemPresenter
{
    object View { get; }

    void ApplySettings(object settings);

    void Update(object runtimeState);
}