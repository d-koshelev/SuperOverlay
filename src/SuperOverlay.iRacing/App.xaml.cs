using System.Windows;

namespace SuperOverlay.iRacing;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = new ModeControlWindow();
        MainWindow = window;
        window.Show();
    }
}
