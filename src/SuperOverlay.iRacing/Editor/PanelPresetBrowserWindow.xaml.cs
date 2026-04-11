using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;
using WpfMessageBoxResult = System.Windows.MessageBoxResult;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.LayoutBuilder.Panels;

namespace SuperOverlay.iRacing.Editor;

public enum PanelPresetBrowserAction
{
    None,
    Insert,
    OpenForEdit
}

public partial class PanelPresetBrowserWindow : Window
{
    private readonly PanelPresetLibrary _library;
    private readonly string _directoryPath;
    private readonly ObservableCollection<PanelPresetBrowserItem> _items = new();
    private PanelPresetDocument? _selectedDocument;

    public PanelPresetBrowserWindow(PanelPresetLibrary library, string directoryPath)
    {
        InitializeComponent();
        _library = library;
        _directoryPath = directoryPath;
        PresetListBox.ItemsSource = _items;
        LibraryPathTextBlock.Text = directoryPath;
        Reload();
    }

    public string? SelectedPresetPath => (PresetListBox.SelectedItem as PanelPresetBrowserItem)?.Path;
    public PanelPresetBrowserAction SelectedAction { get; private set; }

    private void Reload()
    {
        _items.Clear();

        foreach (var entry in _library.List(_directoryPath).OrderBy(item => item.Category).ThenBy(item => item.Name))
        {
            _items.Add(new PanelPresetBrowserItem(entry));
        }

        if (_items.Count > 0)
        {
            PresetListBox.SelectedIndex = 0;
        }

        EmptyStateTextBlock.Visibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        InsertButton.IsEnabled = _items.Count > 0;
        OpenButton.IsEnabled = _items.Count > 0;
        DeleteButton.IsEnabled = _items.Count > 0;
        UpdateSummary();
    }

    private void Refresh_OnClick(object sender, RoutedEventArgs e) => Reload();

    private void Insert_OnClick(object sender, RoutedEventArgs e)
    {
        if (PresetListBox.SelectedItem is null)
        {
            return;
        }

        SelectedAction = PanelPresetBrowserAction.Insert;
        DialogResult = true;
    }

    private void Open_OnClick(object sender, RoutedEventArgs e)
    {
        if (PresetListBox.SelectedItem is null)
        {
            return;
        }

        SelectedAction = PanelPresetBrowserAction.OpenForEdit;
        DialogResult = true;
    }

    private void PresetListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var hasSelection = PresetListBox.SelectedItem is not null;
        InsertButton.IsEnabled = hasSelection;
        OpenButton.IsEnabled = hasSelection;
        DeleteButton.IsEnabled = hasSelection;
        UpdateSummary();
    }

    private void PresetListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (PresetListBox.SelectedItem is null)
        {
            return;
        }

        SelectedAction = PanelPresetBrowserAction.Insert;
        DialogResult = true;
    }

    private void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        if (PresetListBox.SelectedItem is not PanelPresetBrowserItem item)
        {
            return;
        }

        var result = WpfMessageBox.Show(
            this,
            $"Delete panel preset '{item.Name}'?",
            "Delete Panel Preset",
            WpfMessageBoxButton.YesNo,
            WpfMessageBoxImage.Warning);

        if (result != WpfMessageBoxResult.Yes)
        {
            return;
        }

        _library.Delete(item.Path);
        Reload();
    }

    private void UpdateSummary()
    {
        if (PresetListBox.SelectedItem is not PanelPresetBrowserItem item)
        {
            _selectedDocument = null;
            SummaryNameTextBlock.Text = "Select a preset";
            SummaryCategoryTextBlock.Text = "Category: —";
            SummarySizeTextBlock.Text = "Size: —";
            SummaryItemCountTextBlock.Text = "Widgets: —";
            SummaryLinkCountTextBlock.Text = "Links: —";
            SummaryPathTextBlock.Text = string.Empty;
            return;
        }

        _selectedDocument = File.Exists(item.Path) ? _library.Load(item.Path) : null;
        SummaryNameTextBlock.Text = item.Name;
        SummaryCategoryTextBlock.Text = $"Category: {item.CategoryLine}";
        SummarySizeTextBlock.Text = $"Size: {item.SizeLine}";
        SummaryItemCountTextBlock.Text = $"Widgets: {item.ItemCountLine}";
        SummaryLinkCountTextBlock.Text = $"Links: {_selectedDocument?.Links.Count ?? 0}";
        SummaryPathTextBlock.Text = item.Path;
    }
}


public sealed class PanelPresetBrowserItem
{
    public PanelPresetBrowserItem(PanelPresetLibraryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        Name = entry.Name;
        CategoryLine = string.IsNullOrWhiteSpace(entry.Category) ? "Uncategorized" : entry.Category;
        SizeLine = $"{entry.Width:0.#} × {entry.Height:0.#}";
        Path = entry.Path;
        ItemCountLine = entry.ItemCount.ToString();
    }

    public string Name { get; }
    public string CategoryLine { get; }
    public string SizeLine { get; }
    public string Path { get; }
    public string ItemCountLine { get; }
}
