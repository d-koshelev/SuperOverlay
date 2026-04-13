using System.Linq;
using SuperOverlay.Core.Layouts.Editor;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    public void LoadExternalLayout(LayoutEditorLayoutDocument document)
    {
        ApplyLayout(document);
    }

    public LayoutEditorLayoutDocument ExportCurrentLayout(string? layoutName = null)
    {
        var name = string.IsNullOrWhiteSpace(layoutName)
            ? (_state.CurrentLayoutName ?? "Current Overlay")
            : layoutName!;

        return new LayoutEditorLayoutDocument
        {
            Name = name,
            Widgets = Widgets
                .OrderBy(x => x.Y)
                .ThenBy(x => x.X)
                .Select(LayoutEditorDocumentMapper.ToLayoutWidget)
                .ToList(),
        };
    }
}
