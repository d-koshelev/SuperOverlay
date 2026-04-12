using System.IO;
using System.Text.Json;

namespace SuperOverlay.Core.Layouts.Persistence;

public sealed class JsonFileStore<TDocument>
{
    private readonly JsonSerializerOptions _options;

    public JsonFileStore(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public TDocument? Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return default;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TDocument>(json, _options);
    }

    public void Save(string path, TDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(document);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(document, _options);
        File.WriteAllText(path, json);
    }
}
