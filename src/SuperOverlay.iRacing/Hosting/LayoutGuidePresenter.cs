using System.Windows;
using System.Windows.Controls;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutGuidePresenter
{
    private readonly FrameworkElement _verticalGuide;
    private readonly FrameworkElement _horizontalGuide;

    public LayoutGuidePresenter(FrameworkElement verticalGuide, FrameworkElement horizontalGuide)
    {
        ArgumentNullException.ThrowIfNull(verticalGuide);
        ArgumentNullException.ThrowIfNull(horizontalGuide);

        _verticalGuide = verticalGuide;
        _horizontalGuide = horizontalGuide;
    }

    public void Hide()
    {
        _verticalGuide.Visibility = Visibility.Collapsed;
        _horizontalGuide.Visibility = Visibility.Collapsed;
    }

    public void Show(LayoutMoveResult result)
    {
        if (result.SnapX is not null)
        {
            _verticalGuide.Margin = new Thickness(result.SnapX.Value, 0, 0, 0);
            _verticalGuide.Visibility = Visibility.Visible;
        }
        else
        {
            _verticalGuide.Visibility = Visibility.Collapsed;
        }

        if (result.SnapY is not null)
        {
            _horizontalGuide.Margin = new Thickness(0, result.SnapY.Value, 0, 0);
            _horizontalGuide.Visibility = Visibility.Visible;
        }
        else
        {
            _horizontalGuide.Visibility = Visibility.Collapsed;
        }
    }
}
