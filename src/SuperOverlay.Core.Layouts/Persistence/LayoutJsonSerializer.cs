using System.Text.Json;
using SuperOverlay.Core.Layouts.Layout;

namespace SuperOverlay.Core.Layouts.Persistence;

public sealed class LayoutJsonSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize(LayoutDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var dto = LayoutDocumentDto.FromDocument(document);
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public LayoutDocument Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var dto = JsonSerializer.Deserialize<LayoutDocumentDto>(json, JsonOptions)
                 ?? throw new InvalidOperationException("Failed to deserialize layout document.");

        return dto.ToDocument();
    }
}
