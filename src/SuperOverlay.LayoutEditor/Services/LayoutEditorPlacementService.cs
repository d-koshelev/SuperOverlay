using System.Collections.Generic;
using System.Linq;
using System.Windows;

using SuperOverlay.Core.Layouts.Editor;

namespace SuperOverlay.LayoutEditor;

public static class LayoutEditorPlacementService
{
    public static void BeginWidgetPlacement(LayoutEditorState state, LayoutEditorWidget pendingWidget)
    {
        state.PendingPresetPlacement = null;
        state.IsPlacingPreset = false;
        state.PendingWidgetPlacement = pendingWidget;
        state.IsPlacingWidget = true;
    }

    public static void BeginPresetPlacement(LayoutEditorState state, LayoutEditorPresetDocument preset)
    {
        state.PendingWidgetPlacement = null;
        state.IsPlacingWidget = false;
        state.PendingPresetPlacement = preset;
        state.IsPlacingPreset = true;
    }

    public static void Cancel(LayoutEditorState state)
    {
        state.PendingPresetPlacement = null;
        state.IsPlacingPreset = false;
        state.PendingWidgetPlacement = null;
        state.IsPlacingWidget = false;
    }

    public static IReadOnlyList<LayoutEditorWidget> BuildWidgetPreview(LayoutEditorState state, Point pointer, Size viewport)
    {
        if (!state.IsPlacingWidget || state.PendingWidgetPlacement is null)
        {
            return [];
        }

        var pending = state.PendingWidgetPlacement;
        var targetX = LayoutEditorMath.Clamp(pointer.X - (pending.Width / 2), 0, System.Math.Max(0, viewport.Width - pending.Width));
        var targetY = LayoutEditorMath.Clamp(pointer.Y - (pending.Height / 2), 0, System.Math.Max(0, viewport.Height - pending.Height));
        return [LayoutEditorWidgetFactory.CreateCopy(pending, targetX, targetY)];
    }

    public static IReadOnlyList<LayoutEditorWidget> BuildPresetPreview(LayoutEditorState state, Point pointer, Size viewport)
    {
        if (!state.IsPlacingPreset || state.PendingPresetPlacement is null || state.PendingPresetPlacement.Widgets.Count == 0)
        {
            return [];
        }

        var widgets = state.PendingPresetPlacement.Widgets;
        var minX = widgets.Min(x => x.X);
        var minY = widgets.Min(x => x.Y);
        var maxX = widgets.Max(x => x.X + x.Width);
        var maxY = widgets.Max(x => x.Y + x.Height);
        var presetWidth = maxX - minX;
        var presetHeight = maxY - minY;

        var targetX = LayoutEditorMath.Clamp(pointer.X - (presetWidth / 2), 0, System.Math.Max(0, viewport.Width - presetWidth));
        var targetY = LayoutEditorMath.Clamp(pointer.Y - (presetHeight / 2), 0, System.Math.Max(0, viewport.Height - presetHeight));

        return widgets
            .Select(source => LayoutEditorWidgetFactory.CreateFromPreset(
                source,
                LayoutEditorMath.Clamp(targetX + (source.X - minX), 0, System.Math.Max(0, viewport.Width - source.Width)),
                LayoutEditorMath.Clamp(targetY + (source.Y - minY), 0, System.Math.Max(0, viewport.Height - source.Height))))
            .ToList();
    }

    public static IReadOnlyList<LayoutEditorWidget> MaterializePresetPreview(IReadOnlyList<LayoutEditorWidget> previewWidgets)
    {
        var groupMap = new Dictionary<System.Guid, System.Guid>();
        var created = new List<LayoutEditorWidget>(previewWidgets.Count);

        foreach (var preview in previewWidgets)
        {
            System.Guid? groupId = null;
            if (preview.GroupId.HasValue)
            {
                if (!groupMap.TryGetValue(preview.GroupId.Value, out var remappedGroupId))
                {
                    remappedGroupId = System.Guid.NewGuid();
                    groupMap[preview.GroupId.Value] = remappedGroupId;
                }

                groupId = remappedGroupId;
            }

            created.Add(LayoutEditorWidgetFactory.CreateCopy(preview, groupId: groupId));
        }

        return created;
    }

    public static IReadOnlyList<LayoutEditorWidget> CreateWidgetsFromLayout(LayoutEditorLayoutDocument document, Size viewport)
    {
        return document.Widgets
            .Select(source => LayoutEditorWidgetFactory.CreateFromLayout(
                source,
                LayoutEditorMath.Clamp(source.X, 0, System.Math.Max(0, viewport.Width - source.Width)),
                LayoutEditorMath.Clamp(source.Y, 0, System.Math.Max(0, viewport.Height - source.Height))))
            .ToList();
    }
}
