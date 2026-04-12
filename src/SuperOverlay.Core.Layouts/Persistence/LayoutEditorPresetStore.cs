using System.IO;
using SuperOverlay.Core.Layouts.Persistence;

namespace SuperOverlay.Core.Layouts.Persistence;

using SuperOverlay.Core.Layouts.Editor;

public sealed class LayoutEditorPresetStore
{
    private readonly JsonFileStore<LayoutEditorPresetDocument> _store = new();
    private readonly string _presetsDirectory;

    public LayoutEditorPresetStore(string presetsDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(presetsDirectory);
        _presetsDirectory = presetsDirectory;
        Directory.CreateDirectory(_presetsDirectory);
    }

    public IReadOnlyList<string> ListPresetNames()
    {
        return Directory.EnumerateFiles(_presetsDirectory, "*.json", SearchOption.TopDirectoryOnly)
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

        var path = GetPresetPath(_presetsDirectory, name);
        _store.Save(path, document);
    }

    public LayoutEditorPresetDocument? Load(string presetName)
    {
        if (string.IsNullOrWhiteSpace(presetName))
        {
            return null;
        }

        var path = GetPresetPath(_presetsDirectory, presetName);
        if (!File.Exists(path))
        {
            return null;
        }

        return _store.Load(path);
    }

    public static string SuggestPresetName(int selectedCount, Guid? firstWidgetId, Guid? groupId)
    {
        if (selectedCount <= 0)
        {
            return "GroupPreset";
        }

        if (selectedCount == 1 && firstWidgetId.HasValue)
        {
            return $"WidgetPreset_{firstWidgetId.Value.ToString()[..8]}";
        }

        return groupId.HasValue
            ? $"GroupPreset_{groupId.Value.ToString()[..8]}"
            : $"GroupPreset_{selectedCount}Widgets";
    }

    private static string GetPresetPath(string presetsDirectory, string presetName)
    {
        var fileName = SanitizeFileName(presetName) + ".json";
        return Path.Combine(presetsDirectory, fileName);
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var filtered = new string(value.Where(ch => !invalidChars.Contains(ch)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(filtered) ? "Preset" : filtered;
    }
}
