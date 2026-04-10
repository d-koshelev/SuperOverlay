using System.Windows;
using System.Windows.Controls;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor;

public sealed class MainWindowCanvasController
{
    private readonly EditorCanvasView _view;
    private readonly Func<OverlayRuntimeSession?> _getSession;
    private readonly Action _refreshProperties;

    public MainWindowCanvasController(
        EditorCanvasView view,
        Func<OverlayRuntimeSession?> getSession,
        Action refreshProperties)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _getSession = getSession ?? throw new ArgumentNullException(nameof(getSession));
        _refreshProperties = refreshProperties ?? throw new ArgumentNullException(nameof(refreshProperties));
    }

    public bool HandlePreviewRightMouseDown(DependencyObject? originalSource)
    {
        var session = _getSession();
        if (session is null)
        {
            return false;
        }

        HideSelectionMarquee();

        var hitItemId = session.HitTestItemId(originalSource);
        if (hitItemId is null)
        {
            session.SelectItem(null);
            _refreshProperties();
            return !session.CanPaste;
        }

        if (!session.GetSelectedItemIds().Contains(hitItemId.Value))
        {
            session.SelectItem(hitItemId.Value);
            _refreshProperties();
        }

        return false;
    }

    public bool PrepareContextMenu()
    {
        var session = _getSession();
        if (session is null)
        {
            return false;
        }

        var selectedIds = session.GetSelectedItemIds();
        var hasSelection = selectedIds.Count > 0;
        _view.CopyMenuItem.IsEnabled = hasSelection;
        _view.PasteMenuItem.IsEnabled = session.CanPaste;
        _view.DuplicateMenuItem.IsEnabled = hasSelection;
        _view.DeleteMenuItem.IsEnabled = hasSelection;
        _view.GroupSelectedMenuItem.IsEnabled = selectedIds.Count > 1;
        _view.UngroupSelectedMenuItem.IsEnabled = hasSelection;
        _view.LockSelectedMenuItem.IsEnabled = hasSelection && !session.HasLockedSelection;
        _view.UnlockSelectedMenuItem.IsEnabled = hasSelection && session.HasLockedSelection;
        _view.BringForwardMenuItem.IsEnabled = hasSelection;
        _view.SendBackwardMenuItem.IsEnabled = hasSelection;
        _view.BringToFrontMenuItem.IsEnabled = hasSelection;
        _view.SendToBackMenuItem.IsEnabled = hasSelection;

        return hasSelection || session.CanPaste;
    }

    public void ShowSelectionMarquee(Rect rect)
    {
        _view.SelectionMarquee.Visibility = Visibility.Visible;
        _view.SelectionMarquee.Margin = new Thickness(rect.X, rect.Y, 0, 0);
        _view.SelectionMarquee.Width = Math.Max(0, rect.Width);
        _view.SelectionMarquee.Height = Math.Max(0, rect.Height);
    }

    public void HideSelectionMarquee()
    {
        _view.SelectionMarquee.Visibility = Visibility.Collapsed;
        _view.SelectionMarquee.Width = 0;
        _view.SelectionMarquee.Height = 0;
        _view.SelectionMarquee.Margin = new Thickness(0);
    }
}
