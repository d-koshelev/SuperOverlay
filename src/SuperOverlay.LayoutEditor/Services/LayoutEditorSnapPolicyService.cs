using System.Windows.Input;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorSnapPolicyService
{
    private readonly LayoutEditorState _state;

    public LayoutEditorSnapPolicyService(LayoutEditorState state)
    {
        _state = state;
    }

    public bool IsSnapTemporarilyDisabled()
    {
        return Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);
    }

    public bool IsInteractionSnapEnabled()
    {
        if (IsSnapTemporarilyDisabled())
        {
            return false;
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return true;
        }

        return _state.IsSnappingEnabled;
    }

    public bool IsPanelSnapEnabled()
    {
        if (IsSnapTemporarilyDisabled())
        {
            return false;
        }

        return _state.IsSnappingEnabled;
    }
}
