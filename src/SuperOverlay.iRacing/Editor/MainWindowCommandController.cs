using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Windows.Input;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor;

public sealed class MainWindowCommandController
{
    private const double MoveStep = 10;

    private readonly Func<OverlayRuntimeSession?> _getSession;
    private readonly LayoutSaveCoordinator _saveCoordinator;
    private readonly LayoutGuidePresenter _guidePresenter;
    private readonly Func<double> _getCanvasWidth;
    private readonly Func<double> _getCanvasHeight;
    private readonly Action _refreshCatalog;
    private readonly Action _refreshProperties;
    private readonly Action _hideSelectionMarquee;

    public MainWindowCommandController(
        Func<OverlayRuntimeSession?> getSession,
        LayoutSaveCoordinator saveCoordinator,
        LayoutGuidePresenter guidePresenter,
        Func<double> getCanvasWidth,
        Func<double> getCanvasHeight,
        Action refreshCatalog,
        Action refreshProperties,
        Action hideSelectionMarquee)
    {
        _getSession = getSession;
        _saveCoordinator = saveCoordinator;
        _guidePresenter = guidePresenter;
        _getCanvasWidth = getCanvasWidth;
        _getCanvasHeight = getCanvasHeight;
        _refreshCatalog = refreshCatalog;
        _refreshProperties = refreshProperties;
        _hideSelectionMarquee = hideSelectionMarquee;
    }

    public void SaveNow() => _saveCoordinator.SaveNow();

    public void QueueSave() => _saveCoordinator.QueueSave();

    public void CancelPendingSave() => _saveCoordinator.CancelPendingSave();

    public void ReloadLayout()
    {
        var session = _getSession();
        if (session is null)
        {
            return;
        }

        _saveCoordinator.CancelPendingSave();
        session.ReloadLayout();
        _refreshCatalog();
        _refreshProperties();
        _guidePresenter.Hide();
        _hideSelectionMarquee();
    }

    public bool AddSelectedCatalogItem(DashboardCatalogItem? item)
    {
        var session = _getSession();
        if (session is null || item is null)
        {
            return false;
        }

        if (!session.AddItem(item.TypeId))
        {
            return false;
        }

        session.SelectItem(session.GetLayoutItems().LastOrDefault()?.Id);
        _refreshProperties();
        _saveCoordinator.QueueSave();
        return true;
    }

    public void CopySelected() => _getSession()?.CopySelected();

    public void GroupSelected() => ExecuteAndSave(session => session.GroupSelectedItems());

    public void UngroupSelected() => ExecuteAndSave(session => session.UngroupSelected());

    public void DuplicateSelected() => ExecuteAndSave(session => session.DuplicateSelected());

    public void DeleteSelected() => ExecuteAndSave(session => session.DeleteSelected());

    public void PasteClipboard() => ExecuteAndSave(session => session.PasteClipboard());

    public void LockSelected() => ExecuteAndSave(session => session.SetLockSelected(true));

    public void UnlockSelected() => ExecuteAndSave(session => session.SetLockSelected(false));

    public void BringForward() => ExecuteAndSave(session => session.BringForwardSelected());

    public void SendBackward() => ExecuteAndSave(session => session.SendBackwardSelected());

    public void BringToFront() => ExecuteAndSave(session => session.BringToFrontSelected());

    public void SendToBack() => ExecuteAndSave(session => session.SendToBackSelected());

    public void ClearSelection()
    {
        var session = _getSession();
        if (session is null)
        {
            return;
        }

        session.SelectItem(null);
        _refreshProperties();
        _guidePresenter.Hide();
        _hideSelectionMarquee();
    }

    public void MoveSelected(double deltaX, double deltaY, ModifierKeys modifiers)
    {
        var session = _getSession();
        if (session is null)
        {
            return;
        }

        var result = session.MoveSelectedWithSnap(deltaX, deltaY, _getCanvasWidth(), _getCanvasHeight(), modifiers.HasFlag(ModifierKeys.Alt));
        if (!result.Moved)
        {
            return;
        }

        _refreshProperties();
        _saveCoordinator.QueueSave();
        _guidePresenter.Show(result);
    }

    public bool HandleKeyDown(WpfKeyEventArgs e)
    {
        var session = _getSession();
        if (session is null)
        {
            return false;
        }

        var modifiers = Keyboard.Modifiers;

        if (modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            _saveCoordinator.SaveNow();
        }
        else if (modifiers == ModifierKeys.Control && e.Key == Key.C)
        {
            session.CopySelected();
        }
        else if (modifiers == ModifierKeys.Control && e.Key == Key.V)
        {
            if (session.PasteClipboard())
            {
                _saveCoordinator.QueueSave();
            }
        }
        else if (modifiers == ModifierKeys.Control && e.Key == Key.D)
        {
            if (session.DuplicateSelected())
            {
                _saveCoordinator.QueueSave();
            }
        }
        else if (modifiers == ModifierKeys.Control && e.Key == Key.G)
        {
            if (session.GroupSelectedItems())
            {
                _saveCoordinator.QueueSave();
            }
        }
        else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.G)
        {
            if (session.UngroupSelected())
            {
                _saveCoordinator.QueueSave();
            }
        }
        else if (modifiers == ModifierKeys.Control && e.Key == Key.L)
        {
            if (session.SetLockSelected(true))
            {
                _saveCoordinator.QueueSave();
            }
        }
        else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.L)
        {
            if (session.SetLockSelected(false))
            {
                _saveCoordinator.QueueSave();
            }
        }
        else if (e.Key == Key.OemOpenBrackets)
        {
            if (modifiers.HasFlag(ModifierKeys.Shift) ? session.SendToBackSelected() : session.SendBackwardSelected())
            {
                _saveCoordinator.QueueSave();
            }
        }
        else if (e.Key == Key.Oem6)
        {
            if (modifiers.HasFlag(ModifierKeys.Shift) ? session.BringToFrontSelected() : session.BringForwardSelected())
            {
                _saveCoordinator.QueueSave();
            }
        }
        else if (e.Key == Key.Delete)
        {
            if (session.DeleteSelected())
            {
                _saveCoordinator.QueueSave();
            }
        }
        else if (e.Key == Key.Escape)
        {
            ClearSelection();
        }
        else if (e.Key == Key.F5)
        {
            ReloadLayout();
        }
        else if (e.Key is Key.Left or Key.Right or Key.Up or Key.Down)
        {
            var step = modifiers.HasFlag(ModifierKeys.Shift) ? MoveStep * 5 : MoveStep;
            var dx = e.Key == Key.Left ? -step : e.Key == Key.Right ? step : 0;
            var dy = e.Key == Key.Up ? -step : e.Key == Key.Down ? step : 0;
            MoveSelected(dx, dy, modifiers);
        }
        else
        {
            return false;
        }

        _refreshProperties();
        return true;
    }

    private void ExecuteAndSave(Func<OverlayRuntimeSession, bool> action)
    {
        var session = _getSession();
        if (session is null)
        {
            return;
        }

        if (action(session))
        {
            _saveCoordinator.QueueSave();
        }

        _refreshProperties();
    }
}
