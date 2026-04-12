using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorCommandService
{
    private readonly LayoutEditorWorkspaceService _workspace;
    private readonly LayoutEditorDialogService _dialogs;
    private readonly ILayoutEditorInteractionEngine? _engine;

    public LayoutEditorCommandService(LayoutEditorWorkspaceService workspace, LayoutEditorDialogService dialogs, ILayoutEditorInteractionEngine? engine = null)
    {
        _workspace = workspace;
        _dialogs = dialogs;
        _engine = engine;
    }

    public LayoutEditorPresetDocument? PickPresetToLoad()
    {
        var presetNames = _workspace.ListPresetNames();
        if (presetNames.Count == 0)
        {
            _dialogs.ShowInfo($"No presets saved yet. Save a widget selection as a preset first.\nFolder: {LayoutEditorStoragePaths.PresetsDirectory}");
            return null;
        }

        var selectedName = _dialogs.PickPresetName(presetNames);
        if (string.IsNullOrWhiteSpace(selectedName))
        {
            return null;
        }

        var preset = _workspace.LoadPreset(selectedName!);
        if (preset is null || preset.Widgets.Count == 0)
        {
            _dialogs.ShowInfo("Preset could not be loaded or is empty.");
            return null;
        }

        return preset;
    }

    public LayoutEditorLayoutDocument? PickLayoutToLoad()
    {
        var layoutNames = _workspace.ListLayoutNames();
        if (layoutNames.Count == 0)
        {
            _dialogs.ShowInfo($"No layouts saved yet. Save a layout first.\nFolder: {LayoutEditorStoragePaths.LayoutsDirectory}");
            return null;
        }

        var selectedName = _dialogs.PickLayoutName(layoutNames);
        if (string.IsNullOrWhiteSpace(selectedName))
        {
            return null;
        }

        var layout = _workspace.LoadLayout(selectedName!);
        if (layout is null)
        {
            _dialogs.ShowInfo("Layout could not be loaded.");
            return null;
        }

        return layout;
    }

    public LayoutEditorLayoutDocument? SaveLayout(string? currentLayoutName, IReadOnlyList<LayoutEditorWidget> widgets)
    {
        var suggestedName = _workspace.SuggestLayoutName(currentLayoutName, widgets.Count);
        var layoutName = _dialogs.PromptLayoutName(suggestedName);
        if (string.IsNullOrWhiteSpace(layoutName))
        {
            return null;
        }

        var document = _workspace.SaveLayout(layoutName!, widgets);
        _dialogs.ShowInfo($"Layout saved to:\n{LayoutEditorStoragePaths.LayoutsDirectory}");
        return document;
    }

    public bool SaveSelectionAsPreset(IReadOnlyList<LayoutEditorWidget> selectedWidgets)
    {
        if (selectedWidgets.Count == 0)
        {
            return false;
        }

        var suggestedName = _workspace.SuggestPresetName(selectedWidgets);
        var presetName = _dialogs.PromptPresetName(suggestedName);
        if (string.IsNullOrWhiteSpace(presetName))
        {
            return false;
        }

        _workspace.SavePreset(presetName!, selectedWidgets);
        _dialogs.ShowInfo($"Preset saved to:\n{LayoutEditorStoragePaths.PresetsDirectory}");
        return true;
    }

    public IReadOnlyList<LayoutEditorWidget> DuplicateSelection(
        ObservableCollection<LayoutEditorWidget> widgets,
        IReadOnlyList<LayoutEditorWidget> selectedWidgets,
        Size viewport,
        double offsetX,
        double offsetY)
    {
        if (selectedWidgets.Count == 0)
        {
            return [];
        }

        if (_engine is not null)
        {
            var sourceById = selectedWidgets.ToDictionary(x => x.Id);
            var newIds = _engine.DuplicateSelection();
            if (newIds.Count == 0)
            {
                return [];
            }

            var orderedSources = selectedWidgets.ToList();
            var created = new List<LayoutEditorWidget>();
            for (var i = 0; i < System.Math.Min(orderedSources.Count, newIds.Count); i++)
            {
                var source = orderedSources[i];
                var copy = LayoutEditorWidgetFactory.CreateCopy(source, id: newIds[i], groupId: null);
                widgets.Add(copy);
                created.Add(copy);
            }

            return created;
        }

        return LayoutEditorWidgetCollectionService.Duplicate(widgets, selectedWidgets, viewport.Width, viewport.Height, offsetX, offsetY);
    }

    public bool TryGroupSelection(IReadOnlyList<LayoutEditorWidget> selectedWidgets)
    {
        if (selectedWidgets.Count < 2)
        {
            _dialogs.ShowInfo("Select at least two widgets to create a group.");
            return false;
        }

        if (_engine is not null)
        {
            return _engine.GroupSelection();
        }

        return LayoutEditorWidgetCollectionService.Group(selectedWidgets);
    }

    public bool TryUngroupSelection(IReadOnlyList<LayoutEditorWidget> selectedWidgets)
    {
        if (selectedWidgets.Count == 0)
        {
            return false;
        }

        if (_engine is not null)
        {
            return _engine.UngroupSelection();
        }

        LayoutEditorWidgetCollectionService.Ungroup(selectedWidgets);
        return true;
    }


    public bool BringToFrontSelection()
    {
        return _engine is not null && _engine.BringToFrontSelection();
    }

    public bool SendToBackSelection()
    {
        return _engine is not null && _engine.SendToBackSelection();
    }

    public bool BringForwardSelection()
    {
        return _engine is not null && _engine.BringForwardSelection();
    }

    public bool SendBackwardSelection()
    {
        return _engine is not null && _engine.SendBackwardSelection();
    }

    public bool ToggleShowInRace(IReadOnlyList<LayoutEditorWidget> selectedWidgets)
    {
        return selectedWidgets.Count > 0 && LayoutEditorWidgetCollectionService.ToggleShowInRace(selectedWidgets);
    }

    public bool SetLockSelection(IReadOnlyList<LayoutEditorWidget> selectedWidgets, bool isLocked)
    {
        if (selectedWidgets.Count == 0)
        {
            return false;
        }

        if (_engine is not null)
        {
            return _engine.SetLockSelection(isLocked);
        }

        foreach (var widget in selectedWidgets)
        {
            widget.IsLocked = isLocked;
        }

        return true;
    }

    public bool ToggleLockSelection(IReadOnlyList<LayoutEditorWidget> selectedWidgets)
    {
        if (selectedWidgets.Count == 0)
        {
            return false;
        }

        var shouldLock = selectedWidgets.Any(x => !x.IsLocked);
        return SetLockSelection(selectedWidgets, shouldLock);
    }

    public bool ApplyShowInRace(IReadOnlyList<LayoutEditorWidget> selectedWidgets, bool showInRace)
    {
        if (selectedWidgets.Count == 0)
        {
            return false;
        }

        LayoutEditorWidgetCollectionService.ApplyShowInRace(selectedWidgets, showInRace);
        return true;
    }

    public bool DeleteSelection(ObservableCollection<LayoutEditorWidget> widgets, IReadOnlyList<LayoutEditorWidget> selectedWidgets)
    {
        if (selectedWidgets.Count == 0)
        {
            return false;
        }

        if (_engine is not null)
        {
            return _engine.DeleteSelection();
        }

        LayoutEditorWidgetCollectionService.Delete(widgets, selectedWidgets);
        return true;
    }

    public void Shutdown(Window window)
    {
        window.Close();
        Application.Current?.Shutdown();
    }
}
