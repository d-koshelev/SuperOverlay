using System.IO;
using System.Text.Json;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorPresetStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public LayoutEditorPresetStore()
    {
        Directory.CreateDirectory(LayoutEditorStoragePaths.PresetsDirectory);
    }

    public IReadOnlyList<string> ListPresetNames()
    {
        return Directory.EnumerateFiles(LayoutEditorStoragePaths.PresetsDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public void Save(LayoutEditorPresetDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var name = SanitizeFileName(document.Name);
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Preset name cannot be empty.");
        }

        document.Name = name;
        document.SavedAtUtc = DateTime.UtcNow;

        var path = GetPresetPath(name);
        var json = JsonSerializer.Serialize(document, JsonOptions);
        File.WriteAllText(path, json);
    }

    public LayoutEditorPresetDocument? Load(string presetName)
    {
        if (string.IsNullOrWhiteSpace(presetName))
        {
            return null;
        }

        var path = GetPresetPath(presetName);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<LayoutEditorPresetDocument>(json, JsonOptions);
    }

    public static string SuggestPresetName(IReadOnlyList<LayoutEditorWidget> selected)
    {
        if (selected.Count == 1)
        {
            return $"WidgetPreset_{selected[0].Id.ToString()[..8]}";
        }

        var groupId = selected.Select(x => x.GroupId).FirstOrDefault(x => x.HasValue);
        return groupId.HasValue
            ? $"GroupPreset_{groupId.Value.ToString()[..8]}"
            : $"GroupPreset_{selected.Count}Widgets";
    }

    private static string GetPresetPath(string presetName)
    {
        var fileName = SanitizeFileName(presetName) + ".json";
        return Path.Combine(LayoutEditorStoragePaths.PresetsDirectory, fileName);
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var filtered = new string(value.Where(ch => !invalidChars.Contains(ch)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(filtered) ? "Preset" : filtered;
    }
}
