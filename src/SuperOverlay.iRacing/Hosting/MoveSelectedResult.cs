namespace SuperOverlay.iRacing.Hosting;

public sealed record MoveSelectedResult(
    bool Changed,
    double? VerticalGuideX,
    double? HorizontalGuideY);
