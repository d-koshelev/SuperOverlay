using System.Collections.ObjectModel;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorWindowRuntime
{
    public required LayoutEditorWorkspaceService Workspace { get; init; }
    public required LayoutEditorDialogService Dialogs { get; init; }
    public required LayoutEditorCommandService Commands { get; init; }
    public required LayoutEditorSlotEditingService SlotEditing { get; init; }
    public required LayoutEditorPropertiesPanelPresenter PropertiesPresenter { get; init; }
    public required LayoutEditorSelectionService Selection { get; init; }
    public required LayoutEditorMarqueePresenter Marquee { get; init; }
    public required LayoutEditorPlacementPresenter Placement { get; init; }
    public required LayoutEditorInteractionCoordinator Interaction { get; init; }
    public required LayoutEditorShortcutService Shortcuts { get; init; }
}
