namespace SuperOverlay.Core.Layouts.Editing;

public sealed record LayoutMoveResult(
    bool Moved,
    double? SnapX,
    double? SnapY);
