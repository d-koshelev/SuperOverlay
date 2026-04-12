using System.Windows;

namespace SuperOverlay.LayoutEditor;

public partial class PresetNameWindow : Window
{
    public PresetNameWindow(string initialName)
    {
        InitializeComponent();
        NameTextBox.Text = initialName;
        Loaded += (_, _) =>
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        };
    }

    public string PresetName => NameTextBox.Text.Trim();

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PresetName))
        {
            MessageBox.Show(this, "Enter a preset name.", "LayoutEditor");
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
