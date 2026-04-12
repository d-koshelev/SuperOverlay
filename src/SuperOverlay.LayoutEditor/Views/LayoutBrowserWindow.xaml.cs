using System.Windows;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutBrowserWindow : Window
{
    public LayoutBrowserWindow(IReadOnlyList<string> layoutNames)
    {
        InitializeComponent();
        LayoutListBox.ItemsSource = layoutNames;
        if (layoutNames.Count > 0)
        {
            LayoutListBox.SelectedIndex = 0;
        }
    }

    public string? SelectedLayoutName => LayoutListBox.SelectedItem as string;

    private void LoadButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (SelectedLayoutName is null)
        {
            MessageBox.Show(this, "Select a layout.", "LayoutEditor");
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
