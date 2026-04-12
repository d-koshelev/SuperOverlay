namespace SuperOverlay.Core.Layouts.PanelLayouts;

using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.Panels;

public sealed class PanelLayoutCompiler
{
    private readonly LayoutDocumentEditor _editor = new();

    public LayoutDocument Compile(
        PanelLayoutDocument panelLayout,
        Func<Guid, PanelPresetDocument?> presetResolver,
        out IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> panelItemMap)
    {
        ArgumentNullException.ThrowIfNull(panelLayout);
        ArgumentNullException.ThrowIfNull(presetResolver);

        var compiled = new LayoutDocument(
            Version: panelLayout.Version,
            Name: panelLayout.Name,
            Canvas: panelLayout.Canvas,
            Items: Array.Empty<LayoutItemInstance>(),
            Placements: Array.Empty<LayoutItemPlacement>(),
            Links: Array.Empty<LayoutItemLink>());

        var resultMap = new Dictionary<Guid, IReadOnlyList<Guid>>();
        var panelIndex = 0;

        foreach (var panel in panelLayout.Panels.Where(x => x.IsVisible).OrderBy(x => x.ZIndex).ThenBy(x => x.Id))
        {
            var preset = presetResolver(panel.PanelPresetId);
            if (preset is null)
            {
                continue;
            }

            var idMap = new Dictionary<Guid, Guid>();
            var sourcePlacements = preset.Placements.OrderBy(x => x.ZIndex).ToList();
            var sourceItems = preset.Items.ToDictionary(x => x.Id);
            var insertedIds = new List<Guid>();
            var panelZBase = panelIndex * 1000;

            foreach (var sourcePlacement in sourcePlacements)
            {
                if (!sourceItems.TryGetValue(sourcePlacement.ItemId, out var sourceItem))
                {
                    continue;
                }

                var newItemId = Guid.NewGuid();
                idMap[sourcePlacement.ItemId] = newItemId;
                insertedIds.Add(newItemId);

                var item = sourceItem with
                {
                    Id = newItemId,
                    IsLocked = panel.IsLocked || sourceItem.IsLocked
                };

                var placement = sourcePlacement with
                {
                    ItemId = newItemId,
                    X = panel.X + (sourcePlacement.X * panel.Scale),
                    Y = panel.Y + (sourcePlacement.Y * panel.Scale),
                    Width = sourcePlacement.Width * panel.Scale,
                    Height = sourcePlacement.Height * panel.Scale,
                    ZIndex = panelZBase + sourcePlacement.ZIndex,
                    RuntimeDeltaX = 0,
                    RuntimeDeltaY = 0,
                    RuntimeX = null,
                    RuntimeY = null,
                    HasRuntimeOverride = false
                };

                compiled = _editor.AddItem(compiled, item, placement);
            }

            foreach (var link in preset.Links)
            {
                if (idMap.TryGetValue(link.SourceItemId, out var newSourceId) && idMap.TryGetValue(link.TargetItemId, out var newTargetId))
                {
                    compiled = _editor.AddLink(compiled, link with
                    {
                        SourceItemId = newSourceId,
                        TargetItemId = newTargetId,
                        Gap = link.Gap * panel.Scale
                    });
                }
            }

            resultMap[panel.Id] = insertedIds;
            panelIndex++;
        }

        panelItemMap = resultMap;
        return compiled;
    }
}
