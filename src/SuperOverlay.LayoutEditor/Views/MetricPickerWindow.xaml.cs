using System.Windows;
using System.Windows.Input;

namespace SuperOverlay.LayoutEditor;

public partial class MetricPickerWindow : Window
{
    public MetricPickerWindow(IReadOnlyList<string> metrics)
    {
        InitializeComponent();
        MetricListBox.ItemsSource = metrics;
        if (metrics.Count > 0)
        {
            MetricListBox.SelectedIndex = 0;
        }
    }

    public string? SelectedMetric => MetricListBox.SelectedItem as string;

    private void SelectButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (MetricListBox.SelectedItem is null)
        {
            return;
        }
        DialogResult = true;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void MetricListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (MetricListBox.SelectedItem is not null)
        {
            DialogResult = true;
        }
    }
}
