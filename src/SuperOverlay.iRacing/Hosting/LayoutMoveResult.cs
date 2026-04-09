namespace SuperOverlay.iRacing.Hosting;

public sealed record LayoutMoveResult(
    bool Moved,
    double? SnapX,
    double? SnapY);
