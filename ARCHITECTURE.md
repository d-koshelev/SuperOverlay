# SuperOverlay Architecture

## Overview

SuperOverlay is organized as three projects with clear and strict responsibility boundaries:

- `SuperOverlay.LayoutBuilder`
- `SuperOverlay.Dashboards`
- `SuperOverlay.iRacing`

This separation exists to ensure that layout logic, dashboard rendering logic, and telemetry integration do not collapse into one mixed codebase.

---

## Project Responsibilities

### SuperOverlay.LayoutBuilder

Responsible for:

- layout data structures
- runtime hosting of layout items
- layout item lifecycle
- later: edit mode, drag, resize, snapping, glue
- later: layout persistence

Does not know:

- iRacing SDK
- sim-specific telemetry concepts
- dashboard-specific business meaning such as gear, fuel, relative, etc.

---

### SuperOverlay.Dashboards

Responsible for:

- dashboard item definitions
- dashboard presenters/views
- dashboard item settings
- unified dashboard runtime state
- dashboard registry

Does not know:

- iRacing SDK
- layout persistence internals
- sim connection details

---

### SuperOverlay.iRacing

Responsible for:

- telemetry integration with iRacing
- telemetry mocking for development
- mapping iRacing data into dashboard runtime state
- startup composition
- overlay application hosting

May reference both `LayoutBuilder` and `Dashboards`.

---

## Project Dependencies

Dependencies are intentionally one-directional:

- `SuperOverlay.Dashboards -> SuperOverlay.LayoutBuilder`
- `SuperOverlay.iRacing -> SuperOverlay.LayoutBuilder`
- `SuperOverlay.iRacing -> SuperOverlay.Dashboards`

`SuperOverlay.LayoutBuilder` must not depend on `Dashboards` or `iRacing`.

---

## Internal Structure

### SuperOverlay.LayoutBuilder

Current intended structure:

- `Contracts/`
- `Layout/`
- `Runtime/`

Planned future structure:

- `Editing/`
- `Interaction/`
- `Persistence/`

#### Contracts
External contracts required by layout items.

#### Layout
Pure layout data model.

#### Runtime
Runtime hosting and management of live layout items.

---

### SuperOverlay.Dashboards

Current intended structure:

- `Contracts/`
- `Runtime/`
- `Registry/`
- `Items/`

#### Contracts
Dashboard-specific contracts such as dashboard definitions.

#### Runtime
Unified runtime state used by all dashboard items.

#### Registry
Catalog of available dashboard item definitions.

#### Items
Concrete dashboard item implementations such as Gear, Speed, Pedals, Fuel, Relative, etc.

---

### SuperOverlay.iRacing

Current intended structure:

- `Telemetry/`
  - `Mock/`
  - `Live/`
- `Mapping/`
- `Hosting/`

Planned future structure:

- `Configuration/`
- `Bootstrap/`

#### Telemetry
Raw data providers.

#### Mapping
Maps raw iRacing data into `DashboardRuntimeState`.

#### Hosting
Application orchestration and runtime update flow.

---

## Layout Model

The layout model is data-only.

It does not contain controls, presenters, services, or runtime references.

### LayoutDocument

Represents a full layout.

Contains:

- `Version`
- `Name`
- `Canvas`
- `Items`
- `Placements`
- `Links`

---

### LayoutCanvas

Represents the base canvas for which the layout is defined.

Contains:

- `Width`
- `Height`

This exists so the layout can later support scaling and resolution-aware behavior.

---

### LayoutItemInstance

Represents a dashboard item instance inside a layout.

Contains:

- `Id`
- `TypeId`
- `Settings`

Notes:

- `Id` is the stable identifier of the instance inside the layout
- `TypeId` is a stable string such as `dashboard.gear`
- `Settings` contains serialized item-specific configuration

---

### LayoutItemPlacement

Represents where an item is placed on the layout.

Contains:

- `ItemId`
- `X`
- `Y`
- `Width`
- `Height`
- `ZIndex`

Placement is intentionally separate from the item instance.

---

### LayoutItemLink

Represents a structural relationship between items.

Contains:

- `SourceItemId`
- `TargetItemId`
- `SourceSide`
- `TargetSide`
- `Gap`

Links are the foundation for future glue/attach behavior and grouped movement.

---

## Dashboard Model

### IDashboardDefinition

Represents a dashboard item type.

A definition must provide:

- `TypeId`
- `DisplayName`
- `SettingsType`
- default settings creation
- presenter creation

A dashboard definition is metadata plus a factory for a dashboard item type.

It is not a runtime instance.

---

### DashboardRegistry

Maps dashboard type identifiers to dashboard definitions.

Responsibilities:

- register definitions
- resolve definitions by `TypeId`
- expose available definitions

The registry is a catalog of types, not a holder of runtime instances.

---

### DashboardRuntimeState

Represents the unified runtime state consumed by dashboard items.

This is the only runtime data shape that dashboard items should depend on.

Dashboard items must not depend on raw iRacing SDK objects.

---

## Runtime Model

### RuntimeLayoutItem

Represents one live runtime item on the overlay.

It binds together:

- `LayoutItemInstance`
- `LayoutItemPlacement`
- `IDashboardDefinition`
- `ILayoutItemPresenter`
- `View`

This object is the bridge between layout data and live UI.

---

### LayoutHost

Responsible for turning layout data into live runtime UI.

Responsibilities:

- accept a layout document
- resolve dashboard definitions via registry
- create presenters
- create runtime layout items
- add views to the render surface
- update all runtime items with runtime state

Does not know:

- concrete dashboard classes such as Gear or Speed
- iRacing telemetry
- how layouts are authored

---

## Runtime Flow

### Data Flow

```text
iRacing telemetry
-> mapping
-> DashboardRuntimeState
-> LayoutHost.Update(state)
-> RuntimeLayoutItems
-> Presenters
-> UI