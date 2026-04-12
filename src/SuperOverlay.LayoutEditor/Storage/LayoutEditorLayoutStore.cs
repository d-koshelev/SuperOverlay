using System.IO;
using System.Text.Json;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorLayoutStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public LayoutEditorLayoutStore()
    {
        Directory.CreateDirectory(LayoutEditorStoragePaths.LayoutsDirectory);
    }

    public IReadOnlyList<string> ListLayoutNames()
    {
        return Directory.EnumerateFiles(LayoutEditorStoragePaths.LayoutsDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public void Save(LayoutEditorLayoutDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var name = SanitizeFileName(document.Name);
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Layout name cannot be empty.");
        }

        document.Name = name;
        document.SavedAtUtc = DateTime.UtcNow;

        var path = GetLayoutPath(name);
        var json = JsonSerializer.Serialize(document, JsonOptions);
        File.WriteAllText(path, json);
    }

    public LayoutEditorLayoutDocument? Load(string layoutName)
    {
        if (string.IsNullOrWhiteSpace(layoutName))
        {
            return null;
        }

        var path = GetLayoutPath(layoutName);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<LayoutEditorLayoutDocument>(json, JsonOptions);
    }

    public static string SuggestLayoutName(string? currentLayoutName, int widgetCount)
    {
        if (!string.IsNullOrWhiteSpace(currentLayoutName))
        {
            return currentLayoutName!;
        }

        return widgetCount > 0 ? $"Layout_{widgetCount}Widgets" : "NewLayout";
    }

    private static string GetLayoutPath(string layoutName)
    {
        var fileName = SanitizeFileName(layoutName) + ".json";
        return Path.Combine(LayoutEditorStoragePaths.LayoutsDirectory, fileName);
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var filtered = new string(value.Where(ch => !invalidChars.Contains(ch)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(filtered) ? "Layout" : filtered;
    }
}
