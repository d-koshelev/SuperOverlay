using SuperOverlay.LayoutBuilder.Layout;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutSnapService
{
    private const double SnapThreshold = 8;
    private const double ReleaseThreshold = 14;

    public SnapMoveResult SnapMove(
        LayoutDocument layout,
        Guid movingItemId,
        LayoutItemPlacement currentPlacement,
        double deltaX,
        double deltaY,
        double canvasWidth,
        double canvasHeight,
        SnapSessionState snapState,
        bool isEnabled)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(currentPlacement);
        ArgumentNullException.ThrowIfNull(snapState);

        var rawX = currentPlacement.X + deltaX;
        var rawY = currentPlacement.Y + deltaY;

        if (!isEnabled)
        {
            snapState.Reset();
            return new SnapMoveResult(rawX, rawY, null, null);
        }

        var xResult = SnapAxisX(layout, movingItemId, currentPlacement, rawX, canvasWidth, snapState.X);
        var yResult = SnapAxisY(layout, movingItemId, currentPlacement, rawY, canvasHeight, snapState.Y);

        return new SnapMoveResult(xResult.SnappedValue, yResult.SnappedValue, xResult.GuideValue, yResult.GuideValue);
    }

    private static AxisResult SnapAxisX(
        LayoutDocument layout,
        Guid movingItemId,
        LayoutItemPlacement moving,
        double rawX,
        double canvasWidth,
        AxisSnapState state)
    {
        var candidates = new List<AxisCandidate>
        {
            new(0, 0),
            new(Math.Max(0, canvasWidth - moving.Width), Math.Max(0, canvasWidth - moving.Width))
        };

        foreach (var other in layout.Placements.Where(x => x.ItemId != movingItemId))
        {
            var otherLeft = other.X;
            var otherRight = other.X + other.Width;
            var otherCenter = other.X + other.Width / 2.0;

            candidates.Add(new(otherLeft, otherLeft));
            candidates.Add(new(otherRight, otherRight));
            candidates.Add(new(otherRight - moving.Width, otherRight));
            candidates.Add(new(otherLeft - moving.Width, otherLeft));
            candidates.Add(new(otherCenter - moving.Width / 2.0, otherCenter));
        }

        return ResolveAxis(rawX, candidates, state);
    }

    private static AxisResult SnapAxisY(
        LayoutDocument layout,
        Guid movingItemId,
        LayoutItemPlacement moving,
        double rawY,
        double canvasHeight,
        AxisSnapState state)
    {
        var candidates = new List<AxisCandidate>
        {
            new(0, 0),
            new(Math.Max(0, canvasHeight - moving.Height), Math.Max(0, canvasHeight - moving.Height))
        };

        foreach (var other in layout.Placements.Where(x => x.ItemId != movingItemId))
        {
            var otherTop = other.Y;
            var otherBottom = other.Y + other.Height;
            var otherCenter = other.Y + other.Height / 2.0;

            candidates.Add(new(otherTop, otherTop));
            candidates.Add(new(otherBottom, otherBottom));
            candidates.Add(new(otherBottom - moving.Height, otherBottom));
            candidates.Add(new(otherTop - moving.Height, otherTop));
            candidates.Add(new(otherCenter - moving.Height / 2.0, otherCenter));
        }

        return ResolveAxis(rawY, candidates, state);
    }

    private static AxisResult ResolveAxis(double rawValue, List<AxisCandidate> candidates, AxisSnapState state)
    {
        AxisCandidate? nearest = null;
        double nearestDistance = double.MaxValue;

        foreach (var candidate in candidates)
        {
            var distance = Math.Abs(rawValue - candidate.SnappedValue);
            if (distance < nearestDistance)
            {
                nearest = candidate;
                nearestDistance = distance;
            }
        }

        if (nearest is null)
        {
            state.Reset();
            return new AxisResult(rawValue, null);
        }

        if (state.IsActive)
        {
            var activeDistance = Math.Abs(rawValue - state.Target);
            if (activeDistance <= ReleaseThreshold)
            {
                return new AxisResult(state.Target, state.GuideValue);
            }

            state.Reset();
        }

        if (nearestDistance <= SnapThreshold)
        {
            state.Activate(nearest.SnappedValue, nearest.GuideValue);
            return new AxisResult(nearest.SnappedValue, nearest.GuideValue);
        }

        return new AxisResult(rawValue, null);
    }

    private sealed record AxisCandidate(double SnappedValue, double GuideValue);
    private sealed record AxisResult(double SnappedValue, double? GuideValue);
}

public sealed record SnapMoveResult(
    double X,
    double Y,
    double? VerticalGuideX,
    double? HorizontalGuideY);
