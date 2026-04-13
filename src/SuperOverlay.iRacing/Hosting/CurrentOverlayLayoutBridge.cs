using System.IO;
using System.Text.Json;
using SuperOverlay.Core.Layouts.Editor;
using SuperOverlay.Core.Layouts.Layout;
using SuperOverlay.Core.Layouts.Persistence;
using SuperOverlay.Dashboards.Items.RawValue;
using SuperOverlay.Dashboards.Runtime;

namespace SuperOverlay.iRacing.Hosting;

public sealed class CurrentOverlayLayoutBridge
{
    private readonly LayoutFileStore _layoutStore = new();

    public string GetCurrentLayoutPath()
        => Path.Combine(AppContext.BaseDirectory, "Layouts", "default-layout.json");

    public LayoutEditorLayoutDocument LoadForEditor(string? path = null)
    {
        var layoutPath = path ?? GetCurrentLayoutPath();
        var provider = new LayoutDocumentProvider();
        var layout = provider.GetOrCreateDefault(layoutPath);

        var document = new LayoutEditorLayoutDocument
        {
            Name = layout.Name,
            Widgets = new List<LayoutEditorLayoutWidget>()
        };

        foreach (var placement in layout.Placements.OrderBy(x => x.ZIndex))
        {
            var item = layout.Items.FirstOrDefault(x => x.Id == placement.ItemId);
            if (item is null)
            {
                continue;
            }

            var widget = new LayoutEditorLayoutWidget
            {
                Id = item.Id,
                X = placement.X,
                Y = placement.Y,
                Width = placement.Width,
                Height = placement.Height,
                IsLocked = item.IsLocked,
                ShowInRace = true,
            };

            if (string.Equals(item.TypeId, "dashboard.raw-value", StringComparison.OrdinalIgnoreCase))
            {
                var settingsJson = item.Settings?.ToString() ?? string.Empty;
                var settings = string.IsNullOrWhiteSpace(settingsJson)
                    ? new RawValueDashboardSettings()
                    : JsonSerializer.Deserialize<RawValueDashboardSettings>(settingsJson) ?? new RawValueDashboardSettings();

                widget.RawBindingSource = settings.ValueBinding.Source switch
                {
                    DashboardFieldSource.SessionInfo => nameof(DashboardFieldSource.SessionInfo),
                    _ => nameof(DashboardFieldSource.TelemetryRaw),
                };
                widget.RawBindingFieldPath = settings.ValueBinding.FieldPath;
                widget.CenterContent = settings.ValueBinding.FieldPath;
            }

            document.Widgets.Add(widget);
        }

        return document;
    }

    public void SaveFromEditor(LayoutEditorLayoutDocument document, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        var layoutPath = path ?? GetCurrentLayoutPath();
        Directory.CreateDirectory(Path.GetDirectoryName(layoutPath)!);

        var items = new List<LayoutItemInstance>();
        var placements = new List<LayoutItemPlacement>();

        foreach (var widget in document.Widgets.OrderBy(x => x.Y).ThenBy(x => x.X).ThenBy(x => x.Id))
        {
            var source = string.Equals(widget.RawBindingSource, nameof(DashboardFieldSource.SessionInfo), StringComparison.OrdinalIgnoreCase)
                ? DashboardFieldSource.SessionInfo
                : DashboardFieldSource.TelemetryRaw;

            var settings = new RawValueDashboardSettings
            {
                ValueBinding = new DashboardFieldBinding(source, string.IsNullOrWhiteSpace(widget.RawBindingFieldPath) ? "Speed" : widget.RawBindingFieldPath),
            };

            items.Add(new LayoutItemInstance(
                widget.Id,
                "dashboard.raw-value",
                JsonSerializer.Serialize(settings),
                widget.IsLocked));

            placements.Add(new LayoutItemPlacement(
                widget.Id,
                widget.X,
                widget.Y,
                widget.Width,
                widget.Height,
                0));
        }

        var runtimeLayout = new LayoutDocument(
            "1.0",
            string.IsNullOrWhiteSpace(document.Name) ? "Current Overlay" : document.Name,
            new LayoutCanvas(1920, 1080),
            items,
            placements,
            new List<LayoutItemLink>());

        _layoutStore.Save(layoutPath, runtimeLayout);
    }
}
