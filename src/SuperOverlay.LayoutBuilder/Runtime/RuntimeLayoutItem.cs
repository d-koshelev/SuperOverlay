using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using SuperOverlay.LayoutBuilder.Contracts;
using SuperOverlay.LayoutBuilder.Layout;

namespace SuperOverlay.LayoutBuilder.Runtime;

public sealed class RuntimeLayoutItem
{
    private const double ResizeHandleSize = 14;

    private readonly Border _selectionFill;
    private readonly Border _selectionBorder;
    private readonly Border _secondarySelectionBorder;
    private readonly Border _resizeThumbHost;
    private readonly Thumb _resizeThumb;
    private readonly Border _groupBadge;
    private readonly TextBlock _groupBadgeText;
    private readonly Border _lockBadge;
    private readonly TextBlock _lockBadgeText;
    private readonly Border _selectionBadge;
    private readonly TextBlock _selectionBadgeText;
    private readonly Grid _chrome;
    private readonly OverlayShellMode _shellMode;

    public LayoutItemInstance Item { get; }
    public LayoutItemPlacement Placement { get; private set; }
    public ILayoutItemPresenter Presenter { get; }
    public UIElement ContentView { get; }
    public FrameworkElement View { get; }

    public RuntimeLayoutItem(
        LayoutItemInstance item,
        LayoutItemPlacement placement,
        ILayoutItemPresenter presenter,
        OverlayShellMode shellMode)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(placement);
        ArgumentNullException.ThrowIfNull(presenter);

        Item = item;
        Placement = placement;
        Presenter = presenter;
        _shellMode = shellMode;
        ContentView = (UIElement)presenter.View;

        _selectionFill = new Border
        {
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(8),
            IsHitTestVisible = false,
            Margin = new Thickness(1)
        };

        _secondarySelectionBorder = new Border
        {
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(8),
            IsHitTestVisible = false
        };

        _selectionBorder = new Border
        {
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6),
            IsHitTestVisible = false,
            Margin = new Thickness(2)
        };

        _resizeThumb = new Thumb
        {
            Width = ResizeHandleSize,
            Height = ResizeHandleSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.SizeNWSE,
            Background = new SolidColorBrush(Color.FromRgb(96, 165, 250)),
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1)
        };

        _resizeThumbHost = new Border
        {
            Width = ResizeHandleSize + 4,
            Height = ResizeHandleSize + 4,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 2, 2),
            Background = new SolidColorBrush(Color.FromArgb(220, 15, 23, 42)),
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Child = _resizeThumb,
            Visibility = Visibility.Collapsed
        };

        _groupBadgeText = new TextBlock
        {
            Text = "G",
            Foreground = Brushes.White,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        _groupBadge = new Border
        {
            Width = 18,
            Height = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(6, 6, 0, 0),
            CornerRadius = new CornerRadius(9),
            Background = new SolidColorBrush(Color.FromRgb(14, 116, 144)),
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1),
            Child = _groupBadgeText,
            Visibility = Visibility.Collapsed,
            IsHitTestVisible = false
        };

        _lockBadgeText = new TextBlock
        {
            Text = "L",
            Foreground = Brushes.White,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        _lockBadge = new Border
        {
            Width = 18,
            Height = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(28, 6, 0, 0),
            CornerRadius = new CornerRadius(9),
            Background = new SolidColorBrush(Color.FromRgb(120, 53, 15)),
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1),
            Child = _lockBadgeText,
            Visibility = Visibility.Collapsed,
            IsHitTestVisible = false
        };

        _selectionBadgeText = new TextBlock
        {
            Foreground = Brushes.White,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 2, 8, 2)
        };

        _selectionBadge = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 6, 6, 0),
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(Color.FromArgb(230, 30, 64, 175)),
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1),
            Child = _selectionBadgeText,
            Visibility = Visibility.Collapsed,
            IsHitTestVisible = false
        };

        _chrome = new Grid
        {
            Background = Brushes.Transparent,
            Tag = item.Id
        };

        if (_shellMode == OverlayShellMode.Runtime)
        {
            _selectionFill.Visibility = Visibility.Collapsed;
            _secondarySelectionBorder.Visibility = Visibility.Collapsed;
            _selectionBorder.Visibility = Visibility.Collapsed;
            _resizeThumbHost.Visibility = Visibility.Collapsed;
            _groupBadge.Visibility = Visibility.Collapsed;
            _lockBadge.Visibility = item.IsLocked ? Visibility.Visible : Visibility.Collapsed;
            _selectionBadge.Visibility = Visibility.Collapsed;
        }

        _chrome.Children.Add(ContentView);
        _chrome.Children.Add(_selectionFill);
        _chrome.Children.Add(_secondarySelectionBorder);
        _chrome.Children.Add(_selectionBorder);
        _chrome.Children.Add(_groupBadge);
        _chrome.Children.Add(_lockBadge);
        _chrome.Children.Add(_selectionBadge);
        _chrome.Children.Add(_resizeThumbHost);
        Panel.SetZIndex(_groupBadge, 9);
        Panel.SetZIndex(_lockBadge, 9);
        Panel.SetZIndex(_selectionBadge, 9);
        Panel.SetZIndex(_resizeThumbHost, 10);

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

    public void SetSelectionState(bool isPrimarySelected, bool isSelected, bool isGroupMember)
    {
        if (_shellMode == OverlayShellMode.Runtime)
        {
            _selectionFill.Background = Brushes.Transparent;
            _secondarySelectionBorder.BorderBrush = Brushes.Transparent;
            _secondarySelectionBorder.BorderThickness = new Thickness(0);
            _selectionBorder.BorderBrush = Brushes.Transparent;
            _selectionBorder.BorderThickness = new Thickness(0);
            _selectionBadge.Visibility = Visibility.Collapsed;
            _groupBadge.Visibility = Visibility.Collapsed;
            _lockBadge.Visibility = Item.IsLocked ? Visibility.Visible : Visibility.Collapsed;
            _resizeThumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        var moveOnlyRuntimeEdit = _shellMode == OverlayShellMode.RuntimeMoveEdit;

        if (isPrimarySelected)
        {
            _selectionFill.Background = new SolidColorBrush(Color.FromArgb(44, 96, 165, 250));
            _secondarySelectionBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255));
            _secondarySelectionBorder.BorderThickness = new Thickness(1);
            _selectionBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            _selectionBorder.BorderThickness = new Thickness(3);

            _selectionBadge.Background = new SolidColorBrush(Color.FromArgb(235, 29, 78, 216));
            _selectionBadgeText.Text = "SELECTED";
            _selectionBadge.Visibility = Visibility.Visible;
        }
        else if (isSelected)
        {
            _selectionFill.Background = new SolidColorBrush(Color.FromArgb(26, 147, 197, 253));
            _secondarySelectionBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
            _secondarySelectionBorder.BorderThickness = new Thickness(1);
            _selectionBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(240, 147, 197, 253));
            _selectionBorder.BorderThickness = new Thickness(2);

            _selectionBadge.Background = new SolidColorBrush(Color.FromArgb(230, 3, 105, 161));
            _selectionBadgeText.Text = "MULTI";
            _selectionBadge.Visibility = Visibility.Visible;
        }
        else if (isGroupMember)
        {
            _selectionFill.Background = new SolidColorBrush(Color.FromArgb(20, 34, 197, 94));
            _secondarySelectionBorder.BorderBrush = Brushes.Transparent;
            _secondarySelectionBorder.BorderThickness = new Thickness(0);
            _selectionBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(230, 34, 197, 94));
            _selectionBorder.BorderThickness = new Thickness(2);

            _selectionBadge.Background = new SolidColorBrush(Color.FromArgb(225, 21, 128, 61));
            _selectionBadgeText.Text = "GROUP";
            _selectionBadge.Visibility = Visibility.Visible;
        }
        else
        {
            _selectionFill.Background = Brushes.Transparent;
            _secondarySelectionBorder.BorderBrush = Brushes.Transparent;
            _secondarySelectionBorder.BorderThickness = new Thickness(0);
            _selectionBorder.BorderBrush = Brushes.Transparent;
            _selectionBorder.BorderThickness = new Thickness(0);
            _selectionBadge.Visibility = Visibility.Collapsed;
        }

        _groupBadge.Visibility = !moveOnlyRuntimeEdit && isGroupMember ? Visibility.Visible : Visibility.Collapsed;
        _lockBadge.Visibility = Item.IsLocked ? Visibility.Visible : Visibility.Collapsed;
        _resizeThumbHost.Visibility = !moveOnlyRuntimeEdit && isPrimarySelected && !Item.IsLocked ? Visibility.Visible : Visibility.Collapsed;

        if (moveOnlyRuntimeEdit && isPrimarySelected)
        {
            _selectionBadge.Background = new SolidColorBrush(Color.FromArgb(235, 14, 116, 144));
            _selectionBadgeText.Text = "MOVE";
            _selectionBadge.Visibility = Visibility.Visible;
        }
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
        return FindAncestor(element, _resizeThumb) is not null || FindAncestor(element, _resizeThumbHost) is not null;
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
                FrameworkContentElement content => content.Parent,
                _ => null
            };
        }

        return null;
    }
}
