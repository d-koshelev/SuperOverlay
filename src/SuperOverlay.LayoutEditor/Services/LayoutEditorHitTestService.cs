using System.Windows;
using System.Windows.Controls;

namespace SuperOverlay.LayoutEditor;

public enum LayoutEditorHitKind
{
    EmptyCanvas,
    WidgetBody,
    ResizeHandle,
    FloatingMenu,
    PropertiesPanel
}

public readonly record struct LayoutEditorHitResult(LayoutEditorHitKind Kind, LayoutEditorWidget? Widget)
{
    public static LayoutEditorHitResult Empty => new(LayoutEditorHitKind.EmptyCanvas, null);
}

public interface ILayoutEditorHitTestService
{
    LayoutEditorHitResult Resolve(DependencyObject? source);
    bool IsInteractiveContent(DependencyObject? source);
    bool IsResizeHandle(DependencyObject? source);
}

public sealed class LayoutEditorHitTestService : ILayoutEditorHitTestService
{
    private readonly Border _floatingMenu;
    private readonly Border _propertiesPanel;
    private readonly Func<DependencyObject?, LayoutEditorWidget?> _resolveWidget;

    public LayoutEditorHitTestService(
        Border floatingMenu,
        Border propertiesPanel,
        Func<DependencyObject?, LayoutEditorWidget?> resolveWidget)
    {
        _floatingMenu = floatingMenu;
        _propertiesPanel = propertiesPanel;
        _resolveWidget = resolveWidget;
    }

    public LayoutEditorHitResult Resolve(DependencyObject? source)
    {
        if (LayoutEditorVisualTreeService.IsWithinOrOwnedBy(source, _floatingMenu))
        {
            return new LayoutEditorHitResult(LayoutEditorHitKind.FloatingMenu, null);
        }

        if (LayoutEditorVisualTreeService.IsWithinOrOwnedBy(source, _propertiesPanel))
        {
            return new LayoutEditorHitResult(LayoutEditorHitKind.PropertiesPanel, null);
        }

        var widget = _resolveWidget(source);
        if (widget is null)
        {
            return LayoutEditorHitResult.Empty;
        }

        return IsResizeHandle(source)
            ? new LayoutEditorHitResult(LayoutEditorHitKind.ResizeHandle, widget)
            : new LayoutEditorHitResult(LayoutEditorHitKind.WidgetBody, widget);
    }

    public bool IsInteractiveContent(DependencyObject? source)
        => LayoutEditorVisualTreeService.FindAncestor<ContentPresenter>(source) is not null;

    public bool IsResizeHandle(DependencyObject? source)
        => LayoutEditorVisualTreeService.IsResizeHandle(source);
}
