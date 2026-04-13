using SuperOverlay.Dashboards.Items.DecorativePanel;
using SuperOverlay.Dashboards.Items.RawValue;
using SuperOverlay.Dashboards.Items.ShiftLeds;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.Core.Layouts.Layout;

using SuperOverlay.Core.Layouts.Editing;
namespace SuperOverlay.iRacing.Hosting;

public sealed class WidgetSettingsSessionService
{
    private readonly DashboardRegistry _registry;
    private readonly LayoutMutationCore _mutationService;

    public WidgetSettingsSessionService(DashboardRegistry registry, LayoutMutationCore mutationService)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _mutationService = mutationService ?? throw new ArgumentNullException(nameof(mutationService));
    }

    public TSettings? GetSelectedSettings<TSettings>(LayoutDocument layout, Guid? selectedItemId, string expectedTypeId)
        where TSettings : class
    {
        if (selectedItemId is null)
        {
            return null;
        }

        var item = layout.Items.FirstOrDefault(x => x.Id == selectedItemId.Value);
        if (item is null || item.TypeId != expectedTypeId)
        {
            return null;
        }

        return _registry.GetItemSettings<TSettings>(item.TypeId, item.Settings);
    }

    public bool UpdateSelectedSettings<TSettings>(ref LayoutDocument layout, Guid? selectedItemId, string expectedTypeId, TSettings settings)
        where TSettings : class
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (selectedItemId is null)
        {
            return false;
        }

        var item = layout.Items.FirstOrDefault(x => x.Id == selectedItemId.Value);
        if (item is null || item.TypeId != expectedTypeId)
        {
            return false;
        }

        return _mutationService.UpdateItemSettings(ref layout, selectedItemId.Value, settings);
    }

    public WidgetCornerSettings? GetSelectedCornerSettings(LayoutDocument layout, Guid? selectedItemId)
    {
        if (selectedItemId is null)
        {
            return null;
        }

        var item = layout.Items.FirstOrDefault(x => x.Id == selectedItemId.Value);
        if (item is null)
        {
            return null;
        }

        return item.TypeId switch
        {
            "dashboard.shift-leds" => _registry.GetItemSettings<ShiftLedDashboardSettings>(item.TypeId, item.Settings) is { } shift
                ? new WidgetCornerSettings(shift.CornerTopLeft, shift.CornerTopRight, shift.CornerBottomRight, shift.CornerBottomLeft)
                : null,
            "dashboard.raw-value" => _registry.GetItemSettings<RawValueDashboardSettings>(item.TypeId, item.Settings) is { } rawValue
                ? new WidgetCornerSettings(rawValue.CornerTopLeft, rawValue.CornerTopRight, rawValue.CornerBottomRight, rawValue.CornerBottomLeft)
                : null,
            "dashboard.decorative-panel" => _registry.GetItemSettings<DecorativePanelDashboardSettings>(item.TypeId, item.Settings) is { } panel
                ? new WidgetCornerSettings(panel.CornerTopLeft, panel.CornerTopRight, panel.CornerBottomRight, panel.CornerBottomLeft)
                : null,
            _ => null
        };
    }

    public bool UpdateSelectedCornerSettings(ref LayoutDocument layout, Guid? selectedItemId, double topLeft, double topRight, double bottomRight, double bottomLeft)
    {
        if (selectedItemId is null)
        {
            return false;
        }

        var item = layout.Items.FirstOrDefault(x => x.Id == selectedItemId.Value);
        if (item is null)
        {
            return false;
        }

        object? updatedSettings = item.TypeId switch
        {
            "dashboard.shift-leds" => _registry.GetItemSettings<ShiftLedDashboardSettings>(item.TypeId, item.Settings) is { } shift
                ? shift with
                {
                    CornerTopLeft = topLeft,
                    CornerTopRight = topRight,
                    CornerBottomRight = bottomRight,
                    CornerBottomLeft = bottomLeft
                }
                : null,
            "dashboard.raw-value" => _registry.GetItemSettings<RawValueDashboardSettings>(item.TypeId, item.Settings) is { } rawValue
                ? rawValue with
                {
                    CornerTopLeft = topLeft,
                    CornerTopRight = topRight,
                    CornerBottomRight = bottomRight,
                    CornerBottomLeft = bottomLeft
                }
                : null,
            "dashboard.decorative-panel" => _registry.GetItemSettings<DecorativePanelDashboardSettings>(item.TypeId, item.Settings) is { } panel
                ? panel with
                {
                    CornerTopLeft = topLeft,
                    CornerTopRight = topRight,
                    CornerBottomRight = bottomRight,
                    CornerBottomLeft = bottomLeft
                }
                : null,
            _ => null
        };

        return updatedSettings is not null && _mutationService.UpdateItemSettings(ref layout, selectedItemId.Value, updatedSettings);
    }
}
