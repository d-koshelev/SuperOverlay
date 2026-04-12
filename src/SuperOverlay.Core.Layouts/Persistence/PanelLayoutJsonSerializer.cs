using System.Text.Json;
using SuperOverlay.Core.Layouts.PanelLayouts;

namespace SuperOverlay.Core.Layouts.Persistence;

public sealed class PanelLayoutJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize(PanelLayoutDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return JsonSerializer.Serialize(PanelLayoutDocumentDto.FromDocument(document), Options);
    }

    public PanelLayoutDocument Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var dto = JsonSerializer.Deserialize<PanelLayoutDocumentDto>(json, Options)
                  ?? throw new InvalidOperationException("Unable to deserialize panel layout document.");

        return dto.ToDocument();
    }
}
