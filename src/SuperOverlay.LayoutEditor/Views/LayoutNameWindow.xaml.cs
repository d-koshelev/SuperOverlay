using System.Windows;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutNameWindow : Window
{
    public LayoutNameWindow(string initialName)
    {
        InitializeComponent();
        NameTextBox.Text = initialName;
        Loaded += (_, _) =>
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        };
    }

    public string LayoutName => NameTextBox.Text.Trim();

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LayoutName))
        {
            MessageBox.Show(this, "Enter a layout name.", "LayoutEditor");
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
