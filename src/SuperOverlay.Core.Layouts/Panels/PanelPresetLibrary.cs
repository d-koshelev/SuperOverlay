using System.IO;
using SuperOverlay.Core.Layouts.Persistence;
namespace SuperOverlay.Core.Layouts.Panels;

public sealed class PanelPresetLibrary
{
    private readonly PanelPresetFileStore _fileStore = new();

    public string GetDefaultDirectory(string layoutPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(layoutPath);

        var baseDirectory = Path.GetDirectoryName(layoutPath);
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            baseDirectory = AppContext.BaseDirectory;
        }

        return Path.Combine(baseDirectory, "PanelPresets");
    }

    public string BuildPresetPath(string directoryPath, PanelPresetDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var sanitizedName = string.Concat(document.Metadata.Name.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            sanitizedName = document.Metadata.Id.ToString("N");
        }

        return Path.Combine(directoryPath, $"{sanitizedName}.panel.json");
    }

    public void Save(string path, PanelPresetDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(document);

        _fileStore.Save(path, document);
    }

    public PanelPresetDocument Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return _fileStore.Load(path);
    }

    public void Delete(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public IReadOnlyList<PanelPresetLibraryEntry> List(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        return _fileStore.List(directoryPath);
    }
}
