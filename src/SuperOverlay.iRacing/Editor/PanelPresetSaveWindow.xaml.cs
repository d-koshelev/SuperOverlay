using System.IO;
using System.Windows;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;
using WpfMessageBoxResult = System.Windows.MessageBoxResult;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor;

public partial class PanelPresetSaveWindow : Window
{
    private readonly PanelPresetLibrary _library;
    private readonly string _directoryPath;

    public PanelPresetSaveWindow(PanelPresetLibrary library, string directoryPath, string suggestedName, string? suggestedCategory = null)
    {
        InitializeComponent();
        _library = library;
        _directoryPath = directoryPath;
        SuggestedName = suggestedName?.Trim() ?? string.Empty;
        SuggestedCategory = suggestedCategory?.Trim() ?? string.Empty;
        NameTextBox.Text = SuggestedName;
        CategoryTextBox.Text = SuggestedCategory;
        DirectoryTextBlock.Text = directoryPath;
        RefreshPreview();
        Loaded += (_, _) => NameTextBox.Focus();
    }

    public string SuggestedName { get; }
    public string SuggestedCategory { get; }
    public string PresetName => NameTextBox.Text.Trim();
    public string PresetCategory => CategoryTextBox.Text.Trim();
    public string? SelectedPath { get; private set; }

    private void Inputs_OnChanged(object sender, RoutedEventArgs e) => RefreshPreview();

    private void Save_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshPreview();
        if (string.IsNullOrWhiteSpace(PresetName) || string.IsNullOrWhiteSpace(SelectedPath))
        {
            ValidationTextBlock.Text = "Enter a panel preset name.";
            ValidationTextBlock.Visibility = Visibility.Visible;
            return;
        }

        if (File.Exists(SelectedPath))
        {
            var result = WpfMessageBox.Show(
                this,
                $"A panel preset named '{PresetName}' already exists. Overwrite it?",
                "Overwrite Panel Preset",
                WpfMessageBoxButton.YesNo,
                WpfMessageBoxImage.Warning);

            if (result != WpfMessageBoxResult.Yes)
            {
                return;
            }
        }

        DialogResult = true;
    }

    private void RefreshPreview()
    {
        ValidationTextBlock.Visibility = Visibility.Collapsed;
        var name = PresetName;
        if (string.IsNullOrWhiteSpace(name))
        {
            SelectedPath = null;
            FilePathTextBlock.Text = "—";
            SaveButton.IsEnabled = false;
            return;
        }

        var previewDocument = new SuperOverlay.LayoutBuilder.Panels.PanelPresetDocument(
            "1.0",
            new SuperOverlay.LayoutBuilder.Panels.PanelPresetMetadata(Guid.Empty, name, PresetCategory, 0, 0),
            Array.Empty<SuperOverlay.LayoutBuilder.Layout.LayoutItemInstance>(),
            Array.Empty<SuperOverlay.LayoutBuilder.Layout.LayoutItemPlacement>(),
            Array.Empty<SuperOverlay.LayoutBuilder.Layout.LayoutItemLink>());

        SelectedPath = _library.BuildPresetPath(_directoryPath, previewDocument);
        FilePathTextBlock.Text = SelectedPath;
        SaveButton.IsEnabled = true;
    }
}
