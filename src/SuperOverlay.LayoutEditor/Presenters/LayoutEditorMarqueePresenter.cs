using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorMarqueePresenter
{
    private readonly LayoutEditorState _state;
    private readonly Rectangle _selectionRectangle;
    private readonly ObservableCollection<LayoutEditorWidget> _widgets;
    private readonly Action<IReadOnlyCollection<LayoutEditorWidget>, LayoutEditorWidget?> _selectWidgets;
    private readonly ILayoutEditorInteractionEngine? _engine;

    public LayoutEditorMarqueePresenter(
        LayoutEditorState state,
        Rectangle selectionRectangle,
        ObservableCollection<LayoutEditorWidget> widgets,
        Action<IReadOnlyCollection<LayoutEditorWidget>, LayoutEditorWidget?> selectWidgets,
        ILayoutEditorInteractionEngine? engine = null)
    {
        _state = state;
        _selectionRectangle = selectionRectangle;
        _widgets = widgets;
        _selectWidgets = selectWidgets;
        _engine = engine;
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
        var marquee = LayoutEditorSelectionMarqueeService.CreateRect(_state.MarqueeStart, current);

        Canvas.SetLeft(_selectionRectangle, marquee.Left);
        Canvas.SetTop(_selectionRectangle, marquee.Top);
        _selectionRectangle.Width = marquee.Width;
        _selectionRectangle.Height = marquee.Height;

        IReadOnlyList<LayoutEditorWidget> selected;
        if (_engine is not null)
        {
            var ids = _engine.GetItemsInSelectionRect(marquee);
            selected = _widgets.Where(w => ids.Contains(w.Id)).ToList();
        }
        else
        {
            selected = LayoutEditorSelectionMarqueeService.ResolveSelection(marquee, _widgets);
        }

        _selectWidgets(selected, selected.FirstOrDefault());
    }

    public void End()
    {
        _state.IsMarqueeSelecting = false;
        _selectionRectangle.Visibility = Visibility.Collapsed;
    }
}
