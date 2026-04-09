using System.Windows.Threading;

namespace SuperOverlay.iRacing.Hosting;

public sealed class LayoutSaveCoordinator
{
    private readonly DispatcherTimer _timer;
    private readonly Action _saveAction;

    public LayoutSaveCoordinator(TimeSpan debounceInterval, Action saveAction)
    {
        ArgumentNullException.ThrowIfNull(saveAction);

        _saveAction = saveAction;
        _timer = new DispatcherTimer
        {
            Interval = debounceInterval
        };

        _timer.Tick += OnTimerTick;
    }

    public void QueueSave()
    {
        _timer.Stop();
        _timer.Start();
    }

    public void SaveNow()
    {
        _timer.Stop();
        _saveAction();
    }

    public void CancelPendingSave()
    {
        _timer.Stop();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _timer.Stop();
        _saveAction();
    }
}
