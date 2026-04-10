using WpfWindow = System.Windows.Window;
using SuperOverlay.iRacing.Hosting;

namespace SuperOverlay.iRacing.Editor.WidgetSettings;

internal interface IEditorWidgetSettingsBinder
{
    void Refresh(OverlayRuntimeSession session, EditorPropertiesPanelView view);
    bool Apply(OverlayRuntimeSession session, EditorPropertiesPanelView view, WpfWindow owner);
}
