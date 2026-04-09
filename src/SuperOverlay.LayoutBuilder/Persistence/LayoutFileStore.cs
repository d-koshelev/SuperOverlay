using System.IO;
using SuperOverlay.LayoutBuilder.Layout;

namespace SuperOverlay.LayoutBuilder.Persistence;

public sealed class LayoutFileStore
{
    private readonly LayoutJsonSerializer _serializer = new();

    public LayoutDocument Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var json = File.ReadAllText(path);
        return _serializer.Deserialize(json);
    }

    public void Save(string path, LayoutDocument document)
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
}
