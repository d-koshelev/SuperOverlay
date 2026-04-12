using System.Collections.Generic;
using System.Windows;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorDialogService
{
    private readonly Window _owner;

    public LayoutEditorDialogService(Window owner)
    {
        _owner = owner;
    }

    public void ShowInfo(string message, string caption = "LayoutEditor")
    {
        MessageBox.Show(_owner, message, caption);
    }

    public string? PromptPresetName(string suggestedName)
    {
        var prompt = new PresetNameWindow(suggestedName) { Owner = _owner };
        return prompt.ShowDialog() == true ? prompt.PresetName : null;
    }

    public string? PickPresetName(IReadOnlyList<string> presetNames)
    {
        var browser = new PresetBrowserWindow(presetNames) { Owner = _owner };
        return browser.ShowDialog() == true ? browser.SelectedPresetName : null;
    }

    public string? PromptLayoutName(string suggestedName)
    {
        var prompt = new LayoutNameWindow(suggestedName) { Owner = _owner };
        return prompt.ShowDialog() == true ? prompt.LayoutName : null;
    }

    public string? PickLayoutName(IReadOnlyList<string> layoutNames)
    {
        var browser = new LayoutBrowserWindow(layoutNames) { Owner = _owner };
        return browser.ShowDialog() == true ? browser.SelectedLayoutName : null;
    }
}
