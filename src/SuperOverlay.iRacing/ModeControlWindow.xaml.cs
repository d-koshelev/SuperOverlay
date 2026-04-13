using System.Windows;
using SuperOverlay.Core.Layouts.Editing;
using SuperOverlay.Core.Layouts.Editor;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.LayoutEditor;

namespace SuperOverlay.iRacing;

public partial class ModeControlWindow : Window
{
    private readonly CurrentOverlayLayoutBridge _layoutBridge = new();
    private RuntimeWindow? _runtimeWindow;
    private LayoutEditorWindow? _editorWindow;

    public ModeControlWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => LaunchRaceMode();
    }

    private void RaceButton_OnClick(object sender, RoutedEventArgs e) => LaunchRaceMode();

    private void EditButton_OnClick(object sender, RoutedEventArgs e) => LaunchEditMode();

    private void LaunchRaceMode()
    {
        SaveEditorLayoutIfNeeded();
        CloseEditor();
        if (_runtimeWindow is null)
        {
            _runtimeWindow = new RuntimeWindow();
            _runtimeWindow.Closed += (_, _) => _runtimeWindow = null;
            _runtimeWindow.Show();
        }
        else
        {
            if (!_runtimeWindow.IsVisible)
            {
                _runtimeWindow.Show();
            }
            _runtimeWindow.Activate();
        }
    }

    private void LaunchEditMode()
    {
        CloseRuntime();
        if (_editorWindow is null)
        {
            _editorWindow = new LayoutEditorWindow(new LayoutEditorEngine());
            _editorWindow.LoadExternalLayout(_layoutBridge.LoadForEditor());
            _editorWindow.Closed += (_, _) => _editorWindow = null;
            _editorWindow.Title = "SuperOverlay Editor";
            _editorWindow.Show();
        }
        else
        {
            if (!_editorWindow.IsVisible)
            {
                _editorWindow.Show();
            }
            _editorWindow.Activate();
        }
    }


    private void SaveEditorLayoutIfNeeded()
    {
        if (_editorWindow is null)
        {
            return;
        }

        var document = _editorWindow.ExportCurrentLayout("Current Overlay");
        _layoutBridge.SaveFromEditor(document);
    }
    private void CloseRuntime()
    {
        _runtimeWindow?.Close();
        _runtimeWindow = null;
    }

    private void CloseEditor()
    {
        _editorWindow?.Close();
        _editorWindow = null;
    }
}
