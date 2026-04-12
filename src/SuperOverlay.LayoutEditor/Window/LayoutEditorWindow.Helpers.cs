using System.Windows;
using System.Windows.Controls;
using SuperOverlay.Core.Layouts.Editing;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    private static double Clamp(double value, double min, double max) => LayoutEditorMath.Clamp(value, min, max);

    internal void SetRaceMode(bool isRaceMode)
    {
        _chrome.SetRaceMode(isRaceMode);
    }

    private void RefreshChromeToggleText()
    {
        _chrome.RefreshChromeToggleText();
    }
}