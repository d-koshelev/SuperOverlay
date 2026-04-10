using System.Windows;

namespace SuperOverlay.iRacing;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var runtimeMode = e.Args.Any(arg => string.Equals(arg, "--runtime", StringComparison.OrdinalIgnoreCase));
        Window window = runtimeMode ? new RuntimeWindow() : new MainWindow();
        MainWindow = window;
        window.Show();
    }
}
