using System.Windows.Controls;

namespace SuperOverlay.iRacing.Editor;

public sealed record EditorCanvasView(
    Grid RootGrid,
    ContextMenu CanvasContextMenu,
    MenuItem CopyMenuItem,
    MenuItem PasteMenuItem,
    MenuItem DuplicateMenuItem,
    MenuItem DeleteMenuItem,
    MenuItem GroupSelectedMenuItem,
    MenuItem UngroupSelectedMenuItem,
    MenuItem LockSelectedMenuItem,
    MenuItem UnlockSelectedMenuItem,
    MenuItem BringForwardMenuItem,
    MenuItem SendBackwardMenuItem,
    MenuItem BringToFrontMenuItem,
    MenuItem SendToBackMenuItem,
    Border SelectionMarquee);
