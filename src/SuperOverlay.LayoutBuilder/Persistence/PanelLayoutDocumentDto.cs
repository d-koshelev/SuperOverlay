using SuperOverlay.LayoutBuilder.Layout;
using SuperOverlay.LayoutBuilder.PanelLayouts;

namespace SuperOverlay.LayoutBuilder.Persistence;

public sealed class PanelLayoutDocumentDto
{
    public string Version { get; set; } = "1.0";
    public string Name { get; set; } = "Panel Layout";
    public LayoutCanvasDto Canvas { get; set; } = new();
    public List<PanelLayoutInstanceDto> Panels { get; set; } = [];

    public PanelLayoutDocument ToDocument()
    {
        return new PanelLayoutDocument(
            Version,
            Name,
            Canvas.ToCanvas(),
            Panels.Select(x => x.ToInstance()).ToList());
    }

    public static PanelLayoutDocumentDto FromDocument(PanelLayoutDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new PanelLayoutDocumentDto
        {
            Version = document.Version,
            Name = document.Name,
            Canvas = LayoutCanvasDto.FromCanvas(document.Canvas),
            Panels = document.Panels.Select(PanelLayoutInstanceDto.FromInstance).ToList()
        };
    }
}

public sealed class PanelLayoutInstanceDto
{
    public Guid Id { get; set; }
    public Guid PanelPresetId { get; set; }
    public string PanelName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public int ZIndex { get; set; }
    public bool IsLocked { get; set; }
    public double Scale { get; set; } = 1.0;
    public bool IsVisible { get; set; } = true;

    public PanelLayoutInstance ToInstance()
    {
        return new PanelLayoutInstance(Id, PanelPresetId, PanelName, Category, X, Y, ZIndex, IsLocked, Scale, IsVisible);
    }

    public static PanelLayoutInstanceDto FromInstance(PanelLayoutInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        return new PanelLayoutInstanceDto
        {
            Id = instance.Id,
            PanelPresetId = instance.PanelPresetId,
            PanelName = instance.PanelName,
            Category = instance.Category,
            X = instance.X,
            Y = instance.Y,
            ZIndex = instance.ZIndex,
            IsLocked = instance.IsLocked,
            Scale = instance.Scale,
            IsVisible = instance.IsVisible
        };
    }
}
