New files added:
- src/SuperOverlay.LayoutEditor/SuperOverlay.LayoutEditor.csproj
- src/SuperOverlay.LayoutEditor/LayoutEditorWindow.xaml
- src/SuperOverlay.LayoutEditor/LayoutEditorWindow.xaml.cs
- src/SuperOverlay.LayoutEditor/LayoutEditorVisibilityOptions.cs

Modified:
- SuperOverlay.slnx
- src/SuperOverlay.iRacing/SuperOverlay.iRacing.csproj
- src/SuperOverlay.iRacing/App.xaml.cs

Entry point:
- dotnet run --project src/SuperOverlay.iRacing -- --layout-editor

Behavior:
- Opens a new standalone LayoutEditor shell with floating menu.
- Includes placeholder properties panel.
- Includes ShowInRaceLayout visibility option placeholder for widgets that remain visible in editor but hidden in production layout.
