using System.IO;
using SuperOverlay.LayoutBuilder.PanelLayouts;
using SuperOverlay.LayoutBuilder.Persistence;


namespace SuperOverlay.iRacing.Hosting;

public sealed class PanelLayoutLibrary
{
    private readonly PanelLayoutFileStore _layoutStore = new();
    private readonly PanelPresetLibrary _presetLibrary = new();
    private readonly PanelLayoutCompiler _compiler = new();

    public string GetDefaultDirectory(string layoutPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(layoutPath);

        var baseDirectory = Path.GetDirectoryName(layoutPath);
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            baseDirectory = AppContext.BaseDirectory;
        }

        return Path.Combine(baseDirectory, "Layouts");
    }

    public string BuildLayoutPath(string directoryPath, PanelLayoutDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var sanitizedName = string.Concat(document.Name.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            sanitizedName = "panel-layout";
        }

        return Path.Combine(directoryPath, $"{sanitizedName}.panel-layout.json");
    }

    public void Save(string path, PanelLayoutDocument document)
    {
        _layoutStore.Save(path, document);
    }

    public PanelLayoutDocument Load(string path)
    {
        return _layoutStore.Load(path);
    }

    public IReadOnlyList<string> List(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(directoryPath, "*.panel-layout.json", SearchOption.TopDirectoryOnly)
            .OrderBy(Path.GetFileNameWithoutExtension)
            .ToList();
    }

    public SuperOverlay.LayoutBuilder.Layout.LayoutDocument CompileToFlatLayout(string panelLayoutPath, string panelPresetDirectory)
    {
        var panelLayout = Load(panelLayoutPath);
        var presetEntries = _presetLibrary.List(panelPresetDirectory)
            .GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.Last());

        return _compiler.Compile(
            panelLayout,
            presetId => presetEntries.TryGetValue(presetId, out var entry)
                ? _presetLibrary.Load(entry.Path)
                : null,
            out _);
    }
}
