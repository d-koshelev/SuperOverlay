using SuperOverlay.LayoutBuilder.Layout;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutSnapService
{
    private const double SnapThreshold = 10;
    private const double ReleaseThreshold = 16;

    private bool _snapActiveX;
    private bool _snapActiveY;
    private double? _snapValueX;
    private double? _snapValueY;

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

        var width = placement.Width;
        var height = placement.Height;

        var candidateX = FindSnapX(layout, movingItemId, targetX, width, canvasWidth);
        var candidateY = FindSnapY(layout, movingItemId, targetY, height, canvasHeight);

        var finalX = ApplyAxisHysteresis(targetX, candidateX, ref _snapActiveX, ref _snapValueX);
        var finalY = ApplyAxisHysteresis(targetY, candidateY, ref _snapActiveY, ref _snapValueY);

        return (finalX, finalY, _snapActiveX ? _snapValueX : null, _snapActiveY ? _snapValueY : null);
    }

    public void EndDrag()
    {
        _snapActiveX = false;
        _snapActiveY = false;
        _snapValueX = null;
        _snapValueY = null;
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

    private static double? FindSnapX(LayoutDocument layout, Guid movingItemId, double targetX, double width, double canvasWidth)
    {
        var candidates = new List<double>
        {
            0,
            canvasWidth - width,
            canvasWidth / 2 - width / 2
        };

        foreach (var other in layout.Placements.Where(x => x.ItemId != movingItemId))
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

        double? best = null;
        var bestDistance = double.MaxValue;

        foreach (var candidate in candidates)
        {
            var distance = Math.Abs(targetX - candidate);
            if (distance < bestDistance)
            {
                best = candidate;
                bestDistance = distance;
            }
        }

        return best;
    }

    private static double? FindSnapY(LayoutDocument layout, Guid movingItemId, double targetY, double height, double canvasHeight)
    {
        var candidates = new List<double>
        {
            0,
            canvasHeight - height,
            canvasHeight / 2 - height / 2
        };

        foreach (var other in layout.Placements.Where(x => x.ItemId != movingItemId))
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

        double? best = null;
        var bestDistance = double.MaxValue;

        foreach (var candidate in candidates)
        {
            var distance = Math.Abs(targetY - candidate);
            if (distance < bestDistance)
            {
                best = candidate;
                bestDistance = distance;
            }
        }

        return best;
    }
}
