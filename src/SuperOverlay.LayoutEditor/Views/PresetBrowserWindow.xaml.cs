using System.Windows;

namespace SuperOverlay.LayoutEditor;

public partial class PresetBrowserWindow : Window
{
    public PresetBrowserWindow(IReadOnlyList<string> presetNames)
    {
        InitializeComponent();
        PresetListBox.ItemsSource = presetNames;
        if (presetNames.Count > 0)
        {
            PresetListBox.SelectedIndex = 0;
        }
    }

    public string? SelectedPresetName => PresetListBox.SelectedItem as string;

    private void InsertButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (SelectedPresetName is null)
        {
            MessageBox.Show(this, "Select a preset.", "LayoutEditor");
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
