using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace SuperOverlay.iRacing.Editor;

internal static class EditorValidationMessages
{
    public static void ShowInvalidBaseProperties(Window owner)
    {
        MessageBox.Show(owner, "Check X, Y, Width, Height and Z-Index values.", "Invalid properties", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public static void ShowInvalidCornerProperties(Window owner)
    {
        MessageBox.Show(owner, "Check corner radius values. Use non-negative numbers.", "Invalid corner properties", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public static void ShowInvalidShiftLedProperties(Window owner)
    {
        MessageBox.Show(owner, "Check LED count and color values. Use 4-24 lamps and color formats like #20262E or #E6111827.", "Invalid LED properties", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public static void ShowInvalidDecorativePanelColor(Window owner)
    {
        MessageBox.Show(owner, "Check decorative panel background color. Use values like #20262E or #CC1F2937.", "Invalid decorative panel properties", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public static void ShowInvalidDecorativePanelOpacity(Window owner)
    {
        MessageBox.Show(owner, "Check decorative panel opacity. Use a value between 0 and 1.", "Invalid decorative panel properties", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
