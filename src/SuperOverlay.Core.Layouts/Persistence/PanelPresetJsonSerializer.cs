using System.Text.Json;
using SuperOverlay.Core.Layouts.Panels;

namespace SuperOverlay.Core.Layouts.Persistence;

public sealed class PanelPresetJsonSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize(PanelPresetDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var dto = PanelPresetDocumentDto.FromDocument(document);
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public PanelPresetDocument Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var dto = JsonSerializer.Deserialize<PanelPresetDocumentDto>(json, JsonOptions)
                 ?? throw new InvalidOperationException("Failed to deserialize panel preset document.");

        return dto.ToDocument();
    }
}
