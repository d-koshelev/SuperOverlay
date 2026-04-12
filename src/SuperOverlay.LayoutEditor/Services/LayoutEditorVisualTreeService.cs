using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorVisualTreeService
{
    public static LayoutEditorWidget? ResolveWidgetFromSource(DependencyObject? source)
    {
        for (var current = source; current is not null; current = GetParent(current))
        {
            if (current is FrameworkElement { DataContext: LayoutEditorWidget widget })
            {
                return widget;
            }
        }

        return null;
    }

    public static bool IsDescendantOf(DependencyObject? source, DependencyObject ancestor)
    {
        for (var current = source; current is not null; current = GetParent(current))
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }
        }

        return false;
    }

    public static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T typed)
            {
                return typed;
            }

            current = GetParent(current);
        }

        return null;
    }

    public static bool IsResizeHandle(DependencyObject? source)
    {
        return FindAncestor<FrameworkElement>(source) is { Name: "WidgetResizeHandle" };
    }

    public static DependencyObject? GetParent(DependencyObject current)
    {
        return current is Visual
            ? VisualTreeHelper.GetParent(current)
            : null;
    }
}
