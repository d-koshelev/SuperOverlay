using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SuperOverlay.LayoutEditor;

public partial class LayoutEditorWindow
{
    private void AddWidgetButton_OnClick(object sender, RoutedEventArgs e)
    {
        BeginWidgetPlacement();
    }

    private void CanvasAddWidgetMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        BeginWidgetPlacement();
    }

    private void CanvasLoadPresetMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        LoadPresetButton_OnClick(sender, e);
    }

    private void CanvasSaveLayoutMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SaveLayoutButton_OnClick(sender, e);
    }

    private void CanvasLoadLayoutMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        LoadLayoutButton_OnClick(sender, e);
    }

    private void LoadPresetButton_OnClick(object sender, RoutedEventArgs e)
    {
        var preset = _commands.PickPresetToLoad();
        if (preset is not null)
        {
            BeginPresetPlacement(preset);
        }
    }

    private void SaveLayoutButton_OnClick(object sender, RoutedEventArgs e)
    {
        var document = _commands.SaveLayout(_state.CurrentLayoutName, Widgets.ToList());
        if (document is null)
        {
            return;
        }

        _state.CurrentLayoutName = document.Name;
        Title = $"SuperOverlay LayoutEditor - {document.Name}";
    }

    private void LoadLayoutButton_OnClick(object sender, RoutedEventArgs e)
    {
        var layoutDocument = _commands.PickLayoutToLoad();
        if (layoutDocument is not null)
        {
            ApplyLayout(layoutDocument);
        }
    }

    private void RaceButton_OnClick(object sender, RoutedEventArgs e)
    {
        SetRaceMode(!_state.IsRaceMode);
    }

    private void OffButton_OnClick(object sender, RoutedEventArgs e)
    {
        _commands.Shutdown(this);
    }

    internal void DuplicateMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SyncEngineFromWidgets();
        var created = _commands.DuplicateSelection(
            Widgets,
            SelectedWidgets,
            new Size(RootGrid.ActualWidth, RootGrid.ActualHeight),
            24,
            24);

        if (created.Count > 0)
        {
            if (_engine is not null)
            {
                ApplyEngineSnapshot(_engine.GetSnapshot());
            }
            SelectWidgets(created, created.FirstOrDefault());
        }
    }

    internal void GroupMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SyncEngineFromWidgets();
        if (_commands.TryGroupSelection(SelectedWidgets))
        {
            if (_engine is not null)
            {
                ApplyEngineSnapshot(_engine.GetSnapshot());
            }
            RefreshSelectionDetails();
        }
    }

    internal void UngroupMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SyncEngineFromWidgets();
        if (_commands.TryUngroupSelection(SelectedWidgets))
        {
            if (_engine is not null)
            {
                ApplyEngineSnapshot(_engine.GetSnapshot());
            }
            RefreshSelectionDetails();
        }
    }


    internal void BringForwardMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SyncEngineFromWidgets();
        if (_commands.BringForwardSelection())
        {
            ApplyEngineSnapshot(_engine!.GetSnapshot());
        }
    }

    internal void SendBackwardMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SyncEngineFromWidgets();
        if (_commands.SendBackwardSelection())
        {
            ApplyEngineSnapshot(_engine!.GetSnapshot());
        }
    }

    internal void BringToFrontMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SyncEngineFromWidgets();
        if (_commands.BringToFrontSelection())
        {
            ApplyEngineSnapshot(_engine!.GetSnapshot());
        }
    }

    internal void SendToBackMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SyncEngineFromWidgets();
        if (_commands.SendToBackSelection())
        {
            ApplyEngineSnapshot(_engine!.GetSnapshot());
        }
    }

    private void SaveAsPresetMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        _commands.SaveSelectionAsPreset(SelectedWidgets);
    }


    private void LockMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SyncEngineFromWidgets();
        if (_commands.SetLockSelection(SelectedWidgets, true))
        {
            if (_engine is not null)
            {
                ApplyEngineSnapshot(_engine.GetSnapshot());
            }
            RefreshSelectionDetails();
        }
    }

    private void UnlockMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SyncEngineFromWidgets();
        if (_commands.SetLockSelection(SelectedWidgets, false))
        {
            if (_engine is not null)
            {
                ApplyEngineSnapshot(_engine.GetSnapshot());
            }
            RefreshSelectionDetails();
        }
    }

    private void ToggleShowInRaceMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_commands.ToggleShowInRace(SelectedWidgets))
        {
            RefreshSelectionDetails();
        }
    }

    internal void DeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        SyncEngineFromWidgets();
        if (_commands.DeleteSelection(Widgets, SelectedWidgets.ToList()))
        {
            if (_engine is not null)
            {
                ApplyEngineSnapshot(_engine.GetSnapshot());
            }
            SelectWidgets([], null);
        }
    }

    private void ShowInRaceLayoutCheckBox_OnChanged(object sender, RoutedEventArgs e)
    {
        if (_commands.ApplyShowInRace(SelectedWidgets, ShowInRaceLayoutCheckBox.IsChecked == true))
        {
            RefreshSelectionDetails();
        }
    }

    private void SnapToggleButton_OnClick(object sender, RoutedEventArgs e)
    {
        _state.IsSnappingEnabled = !_state.IsSnappingEnabled;
        if (!_state.IsSnappingEnabled)
        {
            HideGuides();
        }
        RefreshChromeToggleText();
    }

    private void GuidesToggleButton_OnClick(object sender, RoutedEventArgs e)
    {
        _state.AreGuidesEnabled = !_state.AreGuidesEnabled;
        if (!_state.AreGuidesEnabled)
        {
            HideGuides();
        }
        RefreshGridOverlay();
        RefreshChromeToggleText();
    }
}
