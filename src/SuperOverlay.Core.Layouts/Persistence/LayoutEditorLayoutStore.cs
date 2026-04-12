using System.IO;
using SuperOverlay.Core.Layouts.Persistence;

namespace SuperOverlay.Core.Layouts.Persistence;

using SuperOverlay.Core.Layouts.Editor;

public sealed class LayoutEditorLayoutStore
{
    private readonly JsonFileStore<LayoutEditorLayoutDocument> _store = new();
    private readonly string _layoutsDirectory;

    public LayoutEditorLayoutStore(string layoutsDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(layoutsDirectory);
        _layoutsDirectory = layoutsDirectory;
        Directory.CreateDirectory(_layoutsDirectory);
    }

    public IReadOnlyList<string> ListLayoutNames()
    {
        return Directory.EnumerateFiles(_layoutsDirectory, "*.json", SearchOption.TopDirectoryOnly)
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

        var path = GetLayoutPath(_layoutsDirectory, name);
        _store.Save(path, document);
    }

    public LayoutEditorLayoutDocument? Load(string layoutName)
    {
        if (string.IsNullOrWhiteSpace(layoutName))
        {
            return null;
        }

        var path = GetLayoutPath(_layoutsDirectory, layoutName);
        if (!File.Exists(path))
        {
            return null;
        }

        return _store.Load(path);
    }

    public static string SuggestLayoutName(string? currentLayoutName, int widgetCount)
    {
        if (!string.IsNullOrWhiteSpace(currentLayoutName))
        {
            return currentLayoutName!;
        }

        return widgetCount > 0 ? $"Layout_{widgetCount}Widgets" : "NewLayout";
    }

    private static string GetLayoutPath(string layoutsDirectory, string layoutName)
    {
        var fileName = SanitizeFileName(layoutName) + ".json";
        return Path.Combine(layoutsDirectory, fileName);
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var filtered = new string(value.Where(ch => !invalidChars.Contains(ch)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(filtered) ? "Layout" : filtered;
    }
}
