using System.Windows;
using System.Windows.Input;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor;

public sealed class MainWindowInteractionController
{
    private readonly Func<OverlayRuntimeSession?> _getSession;
    private readonly LayoutEditorInteractionController _interactionController;
    private readonly MainWindowCanvasController _canvasController;
    private readonly LayoutGuidePresenter _guidePresenter;
    private readonly Action _refreshProperties;
    private readonly Action _queueSave;
    private readonly Func<double> _getCanvasWidth;
    private readonly Func<double> _getCanvasHeight;

    public MainWindowInteractionController(
        Func<OverlayRuntimeSession?> getSession,
        LayoutEditorInteractionController interactionController,
        MainWindowCanvasController canvasController,
        LayoutGuidePresenter guidePresenter,
        Action refreshProperties,
        Action queueSave,
        Func<double> getCanvasWidth,
        Func<double> getCanvasHeight)
    {
        _getSession = getSession ?? throw new ArgumentNullException(nameof(getSession));
        _interactionController = interactionController ?? throw new ArgumentNullException(nameof(interactionController));
        _canvasController = canvasController ?? throw new ArgumentNullException(nameof(canvasController));
        _guidePresenter = guidePresenter ?? throw new ArgumentNullException(nameof(guidePresenter));
        _refreshProperties = refreshProperties ?? throw new ArgumentNullException(nameof(refreshProperties));
        _queueSave = queueSave ?? throw new ArgumentNullException(nameof(queueSave));
        _getCanvasWidth = getCanvasWidth ?? throw new ArgumentNullException(nameof(getCanvasWidth));
        _getCanvasHeight = getCanvasHeight ?? throw new ArgumentNullException(nameof(getCanvasHeight));
    }

    public void SetSnappingEnabled(bool enabled)
    {
        var session = _getSession();
        if (session is null)
        {
            return;
        }

        session.SetSnappingEnabled(enabled);
        if (!enabled)
        {
            _guidePresenter.Hide();
            _canvasController.HideSelectionMarquee();
        }
    }

    public bool BeginInteraction(DependencyObject? originalSource, Point canvasPoint, ModifierKeys modifiers)
    {
        var session = _getSession();
        if (session is null)
        {
            return false;
        }

        var startResult = _interactionController.BeginInteraction(originalSource, canvasPoint, modifiers);
        if (startResult.ShowMarquee && startResult.MarqueeRect is Rect startRect)
        {
            _canvasController.ShowSelectionMarquee(startRect);
            _guidePresenter.Hide();
        }

        _refreshProperties();

        if (startResult.ClearedSelection)
        {
            _guidePresenter.Hide();
        }

        return startResult.CaptureMouse;
    }

    public void MoveInteraction(Point canvasPoint, ModifierKeys modifiers, MouseButtonState leftButton)
    {
        if (!_interactionController.IsInteracting || leftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var result = _interactionController.MoveInteraction(
            canvasPoint,
            _getCanvasWidth(),
            _getCanvasHeight(),
            modifiers.HasFlag(ModifierKeys.Alt));

        if (_interactionController.IsMarqueeSelecting)
        {
            if (result.MarqueeRect is Rect rect)
            {
                _canvasController.ShowSelectionMarquee(rect);
            }

            return;
        }

        if (!result.Changed)
        {
            return;
        }

        _guidePresenter.Show(new LayoutMoveResult(result.Changed, result.SnapX, result.SnapY));
    }

    public bool EndInteraction(ModifierKeys modifiers)
    {
        var endResult = _interactionController.EndInteraction(modifiers);
        if (!endResult.Ended)
        {
            return false;
        }

        _canvasController.HideSelectionMarquee();
        _guidePresenter.Hide();
        _refreshProperties();
        _queueSave();
        return true;
    }
}
