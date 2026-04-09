using SuperOverlay.LayoutBuilder.Contracts;
using SuperOverlay.LayoutBuilder.Layout;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace SuperOverlay.LayoutBuilder.Runtime;

public sealed class RuntimeLayoutItem
{
    public LayoutItemInstance Item { get; }
    public LayoutItemPlacement Placement { get; private set; }
    public ILayoutItemPresenter Presenter { get; }
    public UIElement View { get; }

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
        View = (UIElement)presenter.View;
    }

    public void ApplySettings()
    {
        Presenter.ApplySettings(Item.Settings);
    }

    public void ApplyPlacement()
    {
        if (View is FrameworkElement fe)
        {
            fe.Width = Placement.Width;
            fe.Height = Placement.Height;
            fe.Margin = new Thickness(Placement.X, Placement.Y, 0, 0);
            fe.HorizontalAlignment = HorizontalAlignment.Left;
            fe.VerticalAlignment = VerticalAlignment.Top;
        }

        Panel.SetZIndex(View, Placement.ZIndex);
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
}