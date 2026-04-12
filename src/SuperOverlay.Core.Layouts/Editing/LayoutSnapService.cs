using SuperOverlay.Core.Layouts.Layout;

namespace SuperOverlay.Core.Layouts.Editing;

public sealed class LayoutSnapService
{
    private const double SnapThreshold = 10;
    private const double ReleaseThreshold = 16;

    private bool _positionSnapActiveX;
    private bool _positionSnapActiveY;
    private double? _positionSnapValueX;
    private double? _positionSnapValueY;

    private bool _resizeSnapActiveX;
    private bool _resizeSnapActiveY;
    private double? _resizeSnapValueX;
    private double? _resizeSnapValueY;

    public (double X, double Y, double? SnapX, double? SnapY) SnapPosition(
        LayoutDocument layout,
        Guid movingItemId,
        double targetX,
        double targetY,
        double canvasWidth,
        double canvasHeight)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var placement = layout.Placements.FirstOrDefault(x => x.ItemId == movingItemId);
        if (placement is null)
        {
            return (targetX, targetY, null, null);
        }

        return SnapGroupPosition(
            layout,
            new[] { movingItemId },
            targetX,
            targetY,
            placement.Width,
            placement.Height,
            canvasWidth,
            canvasHeight);
    }

    public (double X, double Y, double? SnapX, double? SnapY) SnapGroupPosition(
        LayoutDocument layout,
        IReadOnlyCollection<Guid> movingItemIds,
        double targetX,
        double targetY,
        double width,
        double height,
        double canvasWidth,
        double canvasHeight)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(movingItemIds);

        var candidateX = FindSnapX(layout, movingItemIds, targetX, width, canvasWidth);
        var candidateY = FindSnapY(layout, movingItemIds, targetY, height, canvasHeight);

        var finalX = ApplyAxisHysteresis(targetX, candidateX, ref _positionSnapActiveX, ref _positionSnapValueX);
        var finalY = ApplyAxisHysteresis(targetY, candidateY, ref _positionSnapActiveY, ref _positionSnapValueY);

        return (
            finalX,
            finalY,
            _positionSnapActiveX ? _positionSnapValueX : null,
            _positionSnapActiveY ? _positionSnapValueY : null);
    }

    public (double Width, double Height, double? SnapX, double? SnapY) SnapResize(
        LayoutDocument layout,
        Guid resizingItemId,
        double targetWidth,
        double targetHeight,
        double canvasWidth,
        double canvasHeight)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var placement = layout.Placements.FirstOrDefault(x => x.ItemId == resizingItemId);
        if (placement is null)
        {
            return (targetWidth, targetHeight, null, null);
        }

        const double minWidth = 40;
        const double minHeight = 30;

        var desiredWidth = Math.Max(minWidth, targetWidth);
        var desiredHeight = Math.Max(minHeight, targetHeight);

        var targetRight = placement.X + desiredWidth;
        var targetBottom = placement.Y + desiredHeight;

        var candidateRight = FindResizeSnapRight(layout, resizingItemId, targetRight, canvasWidth);
        var candidateBottom = FindResizeSnapBottom(layout, resizingItemId, targetBottom, canvasHeight);

        var finalRight = ApplyAxisHysteresis(targetRight, candidateRight, ref _resizeSnapActiveX, ref _resizeSnapValueX);
        var finalBottom = ApplyAxisHysteresis(targetBottom, candidateBottom, ref _resizeSnapActiveY, ref _resizeSnapValueY);

        var finalWidth = Math.Max(minWidth, finalRight - placement.X);
        var finalHeight = Math.Max(minHeight, finalBottom - placement.Y);

        return (
            finalWidth,
            finalHeight,
            _resizeSnapActiveX ? _resizeSnapValueX : null,
            _resizeSnapActiveY ? _resizeSnapValueY : null);
    }

    public void EndDrag()
    {
        _positionSnapActiveX = false;
        _positionSnapActiveY = false;
        _positionSnapValueX = null;
        _positionSnapValueY = null;

        _resizeSnapActiveX = false;
        _resizeSnapActiveY = false;
        _resizeSnapValueX = null;
        _resizeSnapValueY = null;
    }

    private static double ApplyAxisHysteresis(double target, double? candidate, ref bool snapActive, ref double? snapValue)
    {
        if (!snapActive)
        {
            if (candidate is not null && Math.Abs(target - candidate.Value) <= SnapThreshold)
            {
                snapActive = true;
                snapValue = candidate.Value;
                return candidate.Value;
            }

            return target;
        }

        if (snapValue is null)
        {
            snapActive = false;
            return target;
        }

        if (Math.Abs(target - snapValue.Value) <= ReleaseThreshold)
        {
            return snapValue.Value;
        }

        snapActive = false;
        snapValue = null;

        if (candidate is not null && Math.Abs(target - candidate.Value) <= SnapThreshold)
        {
            snapActive = true;
            snapValue = candidate.Value;
            return candidate.Value;
        }

        return target;
    }

    private static double? FindSnapX(LayoutDocument layout, IReadOnlyCollection<Guid> movingItemIds, double targetX, double width, double canvasWidth)
    {
        var candidates = new List<double>
        {
            0,
            canvasWidth - width,
            canvasWidth / 2 - width / 2
        };

        foreach (var other in layout.Placements.Where(x => !movingItemIds.Contains(x.ItemId)))
        {
            var otherLeft = other.X;
            var otherRight = other.X + other.Width;
            var otherCenter = other.X + other.Width / 2;

            candidates.Add(otherLeft);
            candidates.Add(otherRight);
            candidates.Add(otherLeft - width);
            candidates.Add(otherRight - width);
            candidates.Add(otherCenter - width / 2);
        }

        return FindClosest(targetX, candidates);
    }

    private static double? FindSnapY(LayoutDocument layout, IReadOnlyCollection<Guid> movingItemIds, double targetY, double height, double canvasHeight)
    {
        var candidates = new List<double>
        {
            0,
            canvasHeight - height,
            canvasHeight / 2 - height / 2
        };

        foreach (var other in layout.Placements.Where(x => !movingItemIds.Contains(x.ItemId)))
        {
            var otherTop = other.Y;
            var otherBottom = other.Y + other.Height;
            var otherCenter = other.Y + other.Height / 2;

            candidates.Add(otherTop);
            candidates.Add(otherBottom);
            candidates.Add(otherTop - height);
            candidates.Add(otherBottom - height);
            candidates.Add(otherCenter - height / 2);
        }

        return FindClosest(targetY, candidates);
    }

    private static double? FindResizeSnapRight(LayoutDocument layout, Guid resizingItemId, double targetRight, double canvasWidth)
    {
        var candidates = new List<double>
        {
            canvasWidth,
            canvasWidth / 2
        };

        foreach (var other in layout.Placements.Where(x => x.ItemId != resizingItemId))
        {
            candidates.Add(other.X);
            candidates.Add(other.X + other.Width);
            candidates.Add(other.X + other.Width / 2);
        }

        return FindClosest(targetRight, candidates);
    }

    private static double? FindResizeSnapBottom(LayoutDocument layout, Guid resizingItemId, double targetBottom, double canvasHeight)
    {
        var candidates = new List<double>
        {
            canvasHeight,
            canvasHeight / 2
        };

        foreach (var other in layout.Placements.Where(x => x.ItemId != resizingItemId))
        {
            candidates.Add(other.Y);
            candidates.Add(other.Y + other.Height);
            candidates.Add(other.Y + other.Height / 2);
        }

        return FindClosest(targetBottom, candidates);
    }

    private static double? FindClosest(double target, IEnumerable<double> candidates)
    {
        double? best = null;
        var bestDistance = double.MaxValue;

        foreach (var candidate in candidates)
        {
            var distance = Math.Abs(target - candidate);
            if (distance < bestDistance)
            {
                best = candidate;
                bestDistance = distance;
            }
        }

        return best;
    }
}
