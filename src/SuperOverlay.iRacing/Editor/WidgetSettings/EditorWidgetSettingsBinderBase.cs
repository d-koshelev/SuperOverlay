using System.Windows;
using WpfWindow = System.Windows.Window;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor.WidgetSettings;

internal abstract class EditorWidgetSettingsBinderBase : IEditorWidgetSettingsBinder
{
    public void Refresh(OverlayRuntimeSession session, EditorPropertiesPanelView view)
    {
        if (TryRefreshCore(session, view))
        {
            SetSectionVisible(view, true);
            return;
        }

        Reset(view);
        SetSectionVisible(view, false);
    }

    public bool Apply(OverlayRuntimeSession session, EditorPropertiesPanelView view, WpfWindow owner)
    {
        if (!IsSectionVisible(view))
        {
            return false;
        }

        return ApplyCore(session, view, owner);
    }

    protected abstract bool TryRefreshCore(OverlayRuntimeSession session, EditorPropertiesPanelView view);
    protected abstract bool ApplyCore(OverlayRuntimeSession session, EditorPropertiesPanelView view, WpfWindow owner);
    protected abstract void Reset(EditorPropertiesPanelView view);
    protected abstract bool IsSectionVisible(EditorPropertiesPanelView view);
    protected abstract void SetSectionVisible(EditorPropertiesPanelView view, bool isVisible);

    protected static void SetPanelVisibility(FrameworkElement panel, bool isVisible)
    {
        panel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }
}
