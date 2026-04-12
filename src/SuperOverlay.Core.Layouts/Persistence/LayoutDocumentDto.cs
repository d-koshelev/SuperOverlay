using System.Text.Json;
using SuperOverlay.Core.Layouts.Layout;

namespace SuperOverlay.Core.Layouts.Persistence;

public sealed class LayoutDocumentDto
{
    public string Version { get; set; } = "1.0";
    public string Name { get; set; } = "Layout";
    public LayoutCanvasDto Canvas { get; set; } = new();
    public List<LayoutItemInstanceDto> Items { get; set; } = [];
    public List<LayoutItemPlacementDto> Placements { get; set; } = [];
    public List<LayoutItemLinkDto> Links { get; set; } = [];

    public LayoutDocument ToDocument()
    {
        return new LayoutDocument(
            Version,
            Name,
            Canvas.ToCanvas(),
            Items.Select(x => x.ToInstance()).ToList(),
            Placements.Select(x => x.ToPlacement()).ToList(),
            Links.Select(x => x.ToLink()).ToList());
    }

    public static LayoutDocumentDto FromDocument(LayoutDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new LayoutDocumentDto
        {
            Version = document.Version,
            Name = document.Name,
            Canvas = LayoutCanvasDto.FromCanvas(document.Canvas),
            Items = document.Items.Select(LayoutItemInstanceDto.FromInstance).ToList(),
            Placements = document.Placements.Select(LayoutItemPlacementDto.FromPlacement).ToList(),
            Links = document.Links.Select(LayoutItemLinkDto.FromLink).ToList()
        };
    }
}

public sealed class LayoutCanvasDto
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double RuntimeOffsetX { get; set; }
    public double RuntimeOffsetY { get; set; }

    public LayoutCanvas ToCanvas() => new(Width, Height, RuntimeOffsetX, RuntimeOffsetY);

    public static LayoutCanvasDto FromCanvas(LayoutCanvas canvas)
    {
        return new LayoutCanvasDto
        {
            Width = canvas.Width,
            Height = canvas.Height,
            RuntimeOffsetX = canvas.RuntimeOffsetX,
            RuntimeOffsetY = canvas.RuntimeOffsetY
        };
    }
}

public sealed class LayoutItemInstanceDto
{
    public Guid Id { get; set; }
    public string TypeId { get; set; } = string.Empty;
    public string SettingsJson { get; set; } = "{}";
    public bool IsLocked { get; set; }

    public LayoutItemInstance ToInstance()
    {
        return new LayoutItemInstance(Id, TypeId, SettingsJson, IsLocked);
    }

    public static LayoutItemInstanceDto FromInstance(LayoutItemInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        string settingsJson;

        if (instance.Settings is null)
        {
            settingsJson = "{}";
        }
        else if (instance.Settings is string rawString)
        {
            settingsJson = rawString;
        }
        else
        {
            settingsJson = JsonSerializer.Serialize(
                instance.Settings,
                instance.Settings.GetType());
        }

        return new LayoutItemInstanceDto
        {
            Id = instance.Id,
            TypeId = instance.TypeId,
            SettingsJson = settingsJson,
            IsLocked = instance.IsLocked
        };
    }
}

public sealed class LayoutItemPlacementDto
{
    public Guid ItemId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public int ZIndex { get; set; }
    public double RuntimeDeltaX { get; set; }
    public double RuntimeDeltaY { get; set; }
    public double? RuntimeX { get; set; }
    public double? RuntimeY { get; set; }
    public bool HasRuntimeOverride { get; set; }

    public LayoutItemPlacement ToPlacement()
    {
        return new LayoutItemPlacement(ItemId, X, Y, Width, Height, ZIndex, RuntimeDeltaX, RuntimeDeltaY, RuntimeX, RuntimeY, HasRuntimeOverride);
    }

    public static LayoutItemPlacementDto FromPlacement(LayoutItemPlacement placement)
    {
        return new LayoutItemPlacementDto
        {
            ItemId = placement.ItemId,
            X = placement.X,
            Y = placement.Y,
            Width = placement.Width,
            Height = placement.Height,
            ZIndex = placement.ZIndex,
            RuntimeDeltaX = placement.RuntimeDeltaX,
            RuntimeDeltaY = placement.RuntimeDeltaY,
            RuntimeX = placement.RuntimeX,
            RuntimeY = placement.RuntimeY,
            HasRuntimeOverride = placement.HasRuntimeOverride
        };
    }
}

public sealed class LayoutItemLinkDto
{
    public Guid SourceItemId { get; set; }
    public Guid TargetItemId { get; set; }
    public LayoutDockSide SourceSide { get; set; }
    public LayoutDockSide TargetSide { get; set; }
    public double Gap { get; set; }

    public LayoutItemLink ToLink()
    {
        return new LayoutItemLink(SourceItemId, TargetItemId, SourceSide, TargetSide, Gap);
    }

    public static LayoutItemLinkDto FromLink(LayoutItemLink link)
    {
        return new LayoutItemLinkDto
        {
            SourceItemId = link.SourceItemId,
            TargetItemId = link.TargetItemId,
            SourceSide = link.SourceSide,
            TargetSide = link.TargetSide,
            Gap = link.Gap
        };
    }
}
