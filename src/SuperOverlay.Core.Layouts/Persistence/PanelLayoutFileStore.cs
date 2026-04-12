using System.IO;
using SuperOverlay.Core.Layouts.PanelLayouts;

namespace SuperOverlay.Core.Layouts.Persistence;

public sealed class PanelLayoutFileStore
{
    private readonly PanelLayoutJsonSerializer _serializer = new();

    public PanelLayoutDocument Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return _serializer.Deserialize(File.ReadAllText(path));
    }

    public void Save(string path, PanelLayoutDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(document);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, _serializer.Serialize(document));
    }
}
