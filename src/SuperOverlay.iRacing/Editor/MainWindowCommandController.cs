using System.IO;
using System.Windows;
using WpfWindow = System.Windows.Window;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Windows.Input;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;
using WpfMessageBoxResult = System.Windows.MessageBoxResult;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.LayoutBuilder.Panels;

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
    private readonly PanelPresetLibrary _panelPresetLibrary = new();
    private readonly PanelLayoutLibrary _panelLayoutLibrary = new();

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

    public bool NewPanelLayout(WpfWindow owner)
    {
        var session = _getSession();
        if (session is null)
        {
            return false;
        }

        if (session.GetLayoutItems().Count > 0)
        {
            var result = WpfMessageBox.Show(
                owner,
                "New Panel Layout will replace the current builder canvas in memory until you reload the flat layout. Continue?",
                "New Panel Layout",
                WpfMessageBoxButton.YesNo,
                WpfMessageBoxImage.Warning);

            if (result != WpfMessageBoxResult.Yes)
            {
                return false;
            }
        }

        var layoutName = $"Panel Layout {DateTime.Now:yyyy-MM-dd HHmm}";
        if (!session.StartNewPanelLayout(layoutName))
        {
            return false;
        }

        _refreshProperties();
        _guidePresenter.Hide();
        _hideSelectionMarquee();
        return true;
    }

    public bool OpenPanelLayout(WpfWindow owner)
    {
        var session = _getSession();
        if (session is null)
        {
            return false;
        }

        var directory = _panelLayoutLibrary.GetDefaultDirectory(session.GetLayoutPath());
        Directory.CreateDirectory(directory);

        var dialog = new WpfOpenFileDialog
        {
            Title = "Open Panel Layout",
            InitialDirectory = directory,
            Filter = "Panel Layout (*.panel-layout.json)|*.panel-layout.json|JSON (*.json)|*.json|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog(owner) != true || string.IsNullOrWhiteSpace(dialog.FileName))
        {
            return false;
        }

        if (!session.OpenPanelLayout(dialog.FileName))
        {
            return false;
        }

        _refreshProperties();
        _guidePresenter.Hide();
        _hideSelectionMarquee();
        return true;
    }

    public bool SavePanelLayout(WpfWindow owner)
    {
        var session = _getSession();
        if (session is null || !session.HasPanelLayout)
        {
            return false;
        }

        var currentPath = session.GetPanelLayoutPath();
        var targetPath = currentPath;
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            var directory = _panelLayoutLibrary.GetDefaultDirectory(session.GetLayoutPath());
            Directory.CreateDirectory(directory);
            var documentName = session.GetLayoutItems().Count > 0 ? "panel-layout" : "panel-layout";

            var dialog = new WpfSaveFileDialog
            {
                Title = "Save Panel Layout",
                InitialDirectory = directory,
                FileName = documentName,
                DefaultExt = ".panel-layout.json",
                Filter = "Panel Layout (*.panel-layout.json)|*.panel-layout.json|JSON (*.json)|*.json"
            };

            if (dialog.ShowDialog(owner) != true || string.IsNullOrWhiteSpace(dialog.FileName))
            {
                return false;
            }

            targetPath = dialog.FileName;
        }

        if (!session.SavePanelLayout(targetPath))
        {
            return false;
        }

        return true;
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

    public bool SaveSelectionAsPanelPreset(WpfWindow owner)
    {
        var session = _getSession();
        if (session is null || !session.HasSelection)
        {
            return false;
        }

        var suggestedName = session.GetSelectedItemProperties()?.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(suggestedName))
        {
            suggestedName = $"Panel-{DateTime.Now:yyyyMMdd-HHmmss}";
        }

        var layoutPath = session.GetLayoutPath();
        var presetDirectory = _panelPresetLibrary.GetDefaultDirectory(layoutPath);
        Directory.CreateDirectory(presetDirectory);

        var saveWindow = new PanelPresetSaveWindow(_panelPresetLibrary, presetDirectory, suggestedName)
        {
            Owner = owner
        };

        if (saveWindow.ShowDialog() != true || string.IsNullOrWhiteSpace(saveWindow.SelectedPath))
        {
            return false;
        }

        var preset = session.CreateSelectedPanelPreset(saveWindow.PresetName, saveWindow.PresetCategory);
        if (preset is null)
        {
            return false;
        }

        _panelPresetLibrary.Save(saveWindow.SelectedPath, preset);
        return true;
    }

    public bool InsertPanelPreset(WpfWindow owner)
    {
        var session = _getSession();
        if (session is null)
        {
            return false;
        }

        var presetDirectory = _panelPresetLibrary.GetDefaultDirectory(session.GetLayoutPath());
        Directory.CreateDirectory(presetDirectory);

        var browser = new PanelPresetBrowserWindow(_panelPresetLibrary, presetDirectory)
        {
            Owner = owner
        };

        if (browser.ShowDialog() != true || browser.SelectedAction != PanelPresetBrowserAction.Insert || string.IsNullOrWhiteSpace(browser.SelectedPresetPath))
        {
            return false;
        }

        var preset = _panelPresetLibrary.Load(browser.SelectedPresetPath);
        var inserted = session.HasPanelLayout
            ? session.InsertPanelPresetAsPanelInstance(preset, 40, 40)
            : session.InsertPanelPreset(preset, 40, 40);

        if (!inserted)
        {
            return false;
        }

        _refreshProperties();
        _saveCoordinator.QueueSave();
        return true;
    }


    public bool OpenPanelPresetForEditing(WpfWindow owner)
    {
        var session = _getSession();
        if (session is null)
        {
            return false;
        }

        var presetDirectory = _panelPresetLibrary.GetDefaultDirectory(session.GetLayoutPath());
        Directory.CreateDirectory(presetDirectory);

        var browser = new PanelPresetBrowserWindow(_panelPresetLibrary, presetDirectory)
        {
            Owner = owner
        };

        if (browser.ShowDialog() != true ||
            browser.SelectedAction != PanelPresetBrowserAction.OpenForEdit ||
            string.IsNullOrWhiteSpace(browser.SelectedPresetPath))
        {
            return false;
        }

        if (session.GetLayoutItems().Count > 0)
        {
            var result = WpfMessageBox.Show(
                owner,
                "Open Panel will replace the current builder canvas in memory until you reload the layout. Continue?",
                "Open Panel Preset",
                WpfMessageBoxButton.YesNo,
                WpfMessageBoxImage.Warning);

            if (result != WpfMessageBoxResult.Yes)
            {
                return false;
            }
        }

        var preset = _panelPresetLibrary.Load(browser.SelectedPresetPath);
        if (!session.OpenPanelPresetForEditing(preset, 40, 40))
        {
            return false;
        }

        _refreshProperties();
        return true;
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
