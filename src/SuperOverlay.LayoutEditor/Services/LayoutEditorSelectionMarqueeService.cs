using System.Windows;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorSelectionMarqueeService
{
    public static Rect CreateRect(Point start, Point current)
    {
        var left = Math.Min(start.X, current.X);
        var top = Math.Min(start.Y, current.Y);
        var width = Math.Abs(current.X - start.X);
        var height = Math.Abs(current.Y - start.Y);
        return new Rect(left, top, width, height);
    }

    public static IReadOnlyList<LayoutEditorWidget> ResolveSelection(Rect marquee, IEnumerable<LayoutEditorWidget> widgets)
    {
        return widgets
            .Where(widget => marquee.IntersectsWith(new Rect(widget.X, widget.Y, widget.Width, widget.Height)))
            .ToList();
    }
}
