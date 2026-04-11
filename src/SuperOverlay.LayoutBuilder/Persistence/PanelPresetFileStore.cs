using System.IO;
using SuperOverlay.LayoutBuilder.Panels;

namespace SuperOverlay.LayoutBuilder.Persistence;

public sealed class PanelPresetFileStore
{
    private readonly PanelPresetJsonSerializer _serializer = new();

    public PanelPresetDocument Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var json = File.ReadAllText(path);
        return _serializer.Deserialize(json);
    }

    public void Save(string path, PanelPresetDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(document);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = _serializer.Serialize(document);
        File.WriteAllText(path, json);
    }

    public IReadOnlyList<PanelPresetLibraryEntry> List(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            return Array.Empty<PanelPresetLibraryEntry>();
        }

        var presets = new List<PanelPresetLibraryEntry>();
        foreach (var path in Directory.EnumerateFiles(directoryPath, "*.panel.json", SearchOption.TopDirectoryOnly))
        {
            var document = Load(path);
            presets.Add(new PanelPresetLibraryEntry(
                document.Metadata.Id,
                document.Metadata.Name,
                document.Metadata.Category,
                path,
                document.Metadata.Width,
                document.Metadata.Height,
                document.Items.Count));
        }

        return presets
            .OrderBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
