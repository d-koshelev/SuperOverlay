using System.Windows;

namespace SuperOverlay.LayoutEditor;

public partial class TextValueWindow : Window
{
    public TextValueWindow(string? initialValue = null)
    {
        InitializeComponent();
        ValueTextBox.Text = initialValue ?? string.Empty;
        Loaded += (_, _) =>
        {
            ValueTextBox.Focus();
            ValueTextBox.SelectAll();
        };
    }

    public string ValueText => ValueTextBox.Text.Trim();

    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
