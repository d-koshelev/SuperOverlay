using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorMarqueePresenter
{
    private readonly LayoutEditorState _state;
    private readonly Rectangle _selectionRectangle;
    private readonly LayoutEditorMarqueeSelectionService _selectionService;

    public LayoutEditorMarqueePresenter(
        LayoutEditorState state,
        Rectangle selectionRectangle,
        LayoutEditorMarqueeSelectionService selectionService)
    {
        _state = state;
        _selectionRectangle = selectionRectangle;
        _selectionService = selectionService;
    }

    public void Begin(Point start)
    {
        _state.IsMarqueeSelecting = true;
        Canvas.SetLeft(_selectionRectangle, start.X);
        Canvas.SetTop(_selectionRectangle, start.Y);
        _selectionRectangle.Width = 0;
        _selectionRectangle.Height = 0;
        _selectionRectangle.Visibility = Visibility.Visible;
    }

    public void Update(Point current)
    {
        var marquee = _selectionService.CreateRect(current);

        Canvas.SetLeft(_selectionRectangle, marquee.Left);
        Canvas.SetTop(_selectionRectangle, marquee.Top);
        _selectionRectangle.Width = marquee.Width;
        _selectionRectangle.Height = marquee.Height;

        _selectionService.ApplySelection(marquee);
    }

    public void End()
    {
        _state.IsMarqueeSelecting = false;
        _selectionRectangle.Visibility = Visibility.Collapsed;
    }
}
