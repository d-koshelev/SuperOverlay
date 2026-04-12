using System.IO;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorStoragePaths
{
    public static string RootDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SuperOverlay",
        "LayoutEditor");

    public static string PresetsDirectory => Path.Combine(RootDirectory, "Presets");

    public static string LayoutsDirectory => Path.Combine(RootDirectory, "Layouts");
}
