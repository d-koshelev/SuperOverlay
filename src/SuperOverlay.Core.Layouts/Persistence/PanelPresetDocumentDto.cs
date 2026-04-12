using System.Text.Json;
using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.Panels;

namespace SuperOverlay.Core.Layouts.Persistence;

public sealed class PanelPresetDocumentDto
{
    public string Version { get; set; } = "1.0";
    public PanelPresetMetadataDto Metadata { get; set; } = new();
    public List<LayoutItemInstanceDto> Items { get; set; } = [];
    public List<LayoutItemPlacementDto> Placements { get; set; } = [];
    public List<LayoutItemLinkDto> Links { get; set; } = [];

    public PanelPresetDocument ToDocument()
    {
        return new PanelPresetDocument(
            Version,
            Metadata.ToMetadata(),
            Items.Select(x => x.ToInstance()).ToList(),
            Placements.Select(x => x.ToPlacement()).ToList(),
            Links.Select(x => x.ToLink()).ToList());
    }

    public static PanelPresetDocumentDto FromDocument(PanelPresetDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new PanelPresetDocumentDto
        {
            Version = document.Version,
            Metadata = PanelPresetMetadataDto.FromMetadata(document.Metadata),
            Items = document.Items.Select(LayoutItemInstanceDto.FromInstance).ToList(),
            Placements = document.Placements.Select(LayoutItemPlacementDto.FromPlacement).ToList(),
            Links = document.Links.Select(LayoutItemLinkDto.FromLink).ToList()
        };
    }
}

public sealed class PanelPresetMetadataDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Custom";
    public double Width { get; set; }
    public double Height { get; set; }

    public PanelPresetMetadata ToMetadata() => new(Id, Name, Category, Width, Height);

    public static PanelPresetMetadataDto FromMetadata(PanelPresetMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        return new PanelPresetMetadataDto
        {
            Id = metadata.Id,
            Name = metadata.Name,
            Category = metadata.Category,
            Width = metadata.Width,
            Height = metadata.Height
        };
    }
}
