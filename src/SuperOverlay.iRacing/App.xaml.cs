using System.Windows;
using WpfWindow = System.Windows.Window;

using SuperOverlay.LayoutEditor;
using SuperOverlay.Core.Layouts.Editing;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var runtimeMode = e.Args.Any(arg => string.Equals(arg, "--runtime", StringComparison.OrdinalIgnoreCase));

        WpfWindow window = runtimeMode
            ? new RuntimeWindow()
            : new LayoutEditorWindow(new LayoutEditorEngine());
        MainWindow = window;
        window.Show();
    }
}
