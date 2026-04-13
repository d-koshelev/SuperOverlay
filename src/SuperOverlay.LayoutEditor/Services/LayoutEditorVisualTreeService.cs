using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
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

    public static bool IsWithinOrOwnedBy(DependencyObject? source, DependencyObject owner)
    {
        if (IsDescendantOf(source, owner))
        {
            return true;
        }

        var popup = FindAncestor<Popup>(source);
        if (popup?.PlacementTarget is null)
        {
            return false;
        }

        return ReferenceEquals(popup.PlacementTarget, owner)
            || IsDescendantOf(popup.PlacementTarget, owner);
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
        for (var current = source; current is not null; current = GetParent(current))
        {
            if (current is FrameworkElement { Name: "WidgetResizeHandle" })
            {
                return true;
            }
        }

        return false;
    }

    public static DependencyObject? GetParent(DependencyObject current)
    {
        if (current is null)
        {
            return null;
        }

        if (current is Visual)
        {
            var visualParent = VisualTreeHelper.GetParent(current);
            if (visualParent is not null)
            {
                return visualParent;
            }
        }

        if (current is FrameworkContentElement frameworkContentElement)
        {
            if (frameworkContentElement.Parent is not null)
            {
                return frameworkContentElement.Parent;
            }

            if (frameworkContentElement.TemplatedParent is not null)
            {
                return frameworkContentElement.TemplatedParent;
            }
        }

        if (current is FrameworkElement frameworkElement)
        {
            if (frameworkElement.Parent is not null)
            {
                return frameworkElement.Parent;
            }

            if (frameworkElement.TemplatedParent is not null)
            {
                return frameworkElement.TemplatedParent;
            }
        }

        if (current is Popup popup)
        {
            return popup.PlacementTarget;
        }

        return LogicalTreeHelper.GetParent(current);
    }
}
