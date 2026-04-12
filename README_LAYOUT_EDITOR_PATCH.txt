STEP 1 ARCHIVE CONTENTS

This archive performs the first aggressive isolation step:
- creates a new permanent project: src/SuperOverlay.Core.Layouts
- copies the current LayoutBuilder domain/persistence/runtime/contracts into that project
- switches project references from SuperOverlay.LayoutBuilder to SuperOverlay.Core.Layouts
- updates namespace usages in dependent projects to SuperOverlay.Core.Layouts
- updates the solution to include SuperOverlay.Core.Layouts instead of SuperOverlay.LayoutBuilder

Notes:
- SuperOverlay.LayoutBuilder is still present on disk as a donor snapshot, but it is no longer part of the solution.
- No editor mechanics were moved yet from SuperOverlay.iRacing/Hosting; that is the next isolation step.
- This patch was prepared statically in the container and was not compiled here because dotnet SDK is unavailable in the environment.

STEP 4
- Deleted src/SuperOverlay.LayoutBuilder from the tree.
- Removed Hosting/LayoutEditorLegacyEngineAdapter.cs.
- Added Core.Layouts.Persistence/JsonFileStore.cs as a shared persistence primitive.
- Switched LayoutEditor preset/layout stores to delegate file IO to Core persistence.
