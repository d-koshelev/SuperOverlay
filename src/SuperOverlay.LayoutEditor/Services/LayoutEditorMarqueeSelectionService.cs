using System.Collections.ObjectModel;
using System.Windows;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorMarqueeSelectionService
{
    private readonly LayoutEditorState _state;
    private readonly ObservableCollection<LayoutEditorWidget> _widgets;
    private readonly Action<IReadOnlyCollection<LayoutEditorWidget>, LayoutEditorWidget?> _selectWidgets;

    public LayoutEditorMarqueeSelectionService(
        LayoutEditorState state,
        ObservableCollection<LayoutEditorWidget> widgets,
        Action<IReadOnlyCollection<LayoutEditorWidget>, LayoutEditorWidget?> selectWidgets)
    {
        _state = state;
        _widgets = widgets;
        _selectWidgets = selectWidgets;
    }

    public Rect CreateRect(Point current)
    {
        var left = Math.Min(_state.MarqueeStart.X, current.X);
        var top = Math.Min(_state.MarqueeStart.Y, current.Y);
        var width = Math.Abs(current.X - _state.MarqueeStart.X);
        var height = Math.Abs(current.Y - _state.MarqueeStart.Y);
        return new Rect(left, top, width, height);
    }

    public IReadOnlyList<LayoutEditorWidget> ResolveSelection(Rect marquee)
    {
        return _widgets
            .Where(widget => marquee.IntersectsWith(new Rect(widget.X, widget.Y, widget.Width, widget.Height)))
            .ToList();
    }

    public void ApplySelection(Rect marquee)
    {
        var selected = ResolveSelection(marquee);
        _selectWidgets(selected, selected.FirstOrDefault());
    }
}
