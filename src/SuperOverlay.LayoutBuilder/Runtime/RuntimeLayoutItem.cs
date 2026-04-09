using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using SuperOverlay.LayoutBuilder.Contracts;
using SuperOverlay.LayoutBuilder.Layout;

namespace SuperOverlay.LayoutBuilder.Runtime;

public sealed class RuntimeLayoutItem
{
    private const double ResizeHandleSize = 14;

    private readonly Border _selectionBorder;
    private readonly Thumb _resizeThumb;
    private readonly Grid _chrome;

    public LayoutItemInstance Item { get; }
    public LayoutItemPlacement Placement { get; private set; }
    public ILayoutItemPresenter Presenter { get; }
    public UIElement ContentView { get; }
    public FrameworkElement View { get; }

    public RuntimeLayoutItem(
        LayoutItemInstance item,
        LayoutItemPlacement placement,
        ILayoutItemPresenter presenter)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(placement);
        ArgumentNullException.ThrowIfNull(presenter);

        Item = item;
        Placement = placement;
        Presenter = presenter;
        ContentView = (UIElement)presenter.View;

        _selectionBorder = new Border
        {
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6),
            IsHitTestVisible = false
        };

        _resizeThumb = new Thumb
        {
            Width = ResizeHandleSize,
            Height = ResizeHandleSize,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 2, 2),
            Cursor = Cursors.SizeNWSE,
            Background = new SolidColorBrush(Color.FromRgb(96, 165, 250)),
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1),
            Visibility = Visibility.Collapsed
        };

        _chrome = new Grid
        {
            Background = Brushes.Transparent,
            Tag = item.Id
        };

        _chrome.Children.Add(ContentView);
        _chrome.Children.Add(_selectionBorder);
        _chrome.Children.Add(_resizeThumb);
        Panel.SetZIndex(_resizeThumb, 10);

        View = _chrome;
    }

    public void ApplySettings()
    {
        Presenter.ApplySettings(Item.Settings);
    }

    public void ApplyPlacement()
    {
        View.Width = Placement.Width;
        View.Height = Placement.Height;
        View.Margin = new Thickness(Placement.X, Placement.Y, 0, 0);
        View.HorizontalAlignment = HorizontalAlignment.Left;
        View.VerticalAlignment = VerticalAlignment.Top;

        if (ContentView is FrameworkElement fe)
        {
            fe.Width = double.NaN;
            fe.Height = double.NaN;
            fe.HorizontalAlignment = HorizontalAlignment.Stretch;
            fe.VerticalAlignment = VerticalAlignment.Stretch;
        }

        Panel.SetZIndex(View, Placement.ZIndex);
    }

    public void SetSelected(bool isSelected)
    {
        _selectionBorder.BorderBrush = isSelected
            ? new SolidColorBrush(Color.FromRgb(96, 165, 250))
            : Brushes.Transparent;
        _selectionBorder.BorderThickness = isSelected ? new Thickness(2) : new Thickness(0);
        _resizeThumb.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
    }

    public void Update(object runtimeState)
    {
        Presenter.Update(runtimeState);
    }

    public void UpdatePlacement(LayoutItemPlacement placement)
    {
        Placement = placement;
        ApplyPlacement();
    }

    public bool ContainsElement(DependencyObject? element)
    {
        return FindAncestor(element, View) is not null;
    }

    public bool IsResizeHandleElement(DependencyObject? element)
    {
        return FindAncestor(element, _resizeThumb) is not null;
    }

    private static DependencyObject? FindAncestor(DependencyObject? origin, DependencyObject target)
    {
        var current = origin;

        while (current is not null)
        {
            if (ReferenceEquals(current, target))
            {
                return current;
            }

            current = current switch
            {
                Visual visual => VisualTreeHelper.GetParent(visual),
                Visual3D visual3D => VisualTreeHelper.GetParent(visual3D),
                FrameworkContentElement content => content.Parent,
                _ => null
            };
        }

        return null;
    }
}
