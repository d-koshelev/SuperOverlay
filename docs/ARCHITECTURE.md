# SuperOverlay Architecture

## Current project shape

SuperOverlay is currently organized around four active projects:

- `SuperOverlay.Core.Layouts`
- `SuperOverlay.Dashboards`
- `SuperOverlay.LayoutEditor`
- `SuperOverlay.iRacing`

Legacy `LayoutBuilder` is not part of the active architecture and should not be reintroduced into runtime or editor flows.

---

## Responsibility boundaries

### SuperOverlay.Core.Layouts
Responsible for:
- layout document model
- layout runtime host
- item placement and shell composition contracts
- prepared-state application surface between backend and widgets

Does not know:
- iRacing SDK
- sim-specific telemetry semantics
- strategy logic
- widget-specific business rules

### SuperOverlay.Dashboards
Responsible for:
- dashboard item definitions
- widget settings models
- prepared payload models
- presenters and renderers
- dashboard registry

Does not know:
- direct iRacing SDK access
- telemetry connection lifecycle
- raw SDK threading

### SuperOverlay.LayoutEditor
Responsible for:
- editor shell
- selection, marquee, drag, resize, snapping
- properties panel
- editing authoring UX

Does not know:
- live sim connection internals
- telemetry ingestion
- heavy presentation calculations

### SuperOverlay.iRacing
Responsible for:
- iRacing SDK integration
- fast telemetry reader
- slow session info reader
- raw stores
- backend presentation processing
- runtime hosting and shell mode orchestration

May reference all other projects.

---

## Core runtime pipeline

The active direction is a backend-driven pipeline:

`Readers -> Raw Stores -> Presentation Processors -> Prepared State -> Publisher -> Widgets`

### 1. Readers
Two independent ingestion paths are required:

- `Fast telemetry reader`
  - reads fast-changing telemetry values
  - writes into `TelemetryRawStore`
- `Slow session reader`
  - reads session info / metadata / thresholds / mostly static values
  - writes into `SessionInfoStore`

### 2. Raw stores
Raw stores keep source-of-truth values exactly as received from telemetry.

Rules:
- raw data must remain raw
- no smoothing or beautification in raw stores
- raw stores must be safe for concurrent read/update
- state handoff should prefer atomic snapshot replacement over long lock-held mutation

### 3. Presentation processors
Processors read raw stores plus widget config and produce prepared payloads.

Processors are responsible for:
- selecting which field to display
- formatting the final display text when needed
- evaluating rules and thresholds
- producing colors, borders, visibility, and other style outputs
- computing custom widget payloads for domain widgets

Widgets are **not** responsible for evaluating telemetry rules.

### 4. Prepared state
Prepared state is the backend-owned, widget-ready view of the overlay.

Conceptually:
- `WidgetId -> PreparedPayload`
- optionally with version / hash / timestamp / changed flag

This layer is required for:
- publish only on change
- selective updates
- debugging what the user should have seen
- replay/freeze scenarios later

### 5. Publisher
Publisher compares newly prepared payload against previously published payload.

Rules:
- if payload did not change, do not publish
- unchanged widgets must not receive `Apply` calls
- publisher should be change-aware, not tick-aware only

### 6. Widgets
Widgets render prepared payload only.

Widgets must:
- not read telemetry directly
- not evaluate business rules
- not perform heavy calculations on the UI thread
- remain as dumb renderers as possible

---

## Raw data model

### TelemetryRaw
Fast-changing telemetry values.

Examples:
- speed
- rpm
- gear
- throttle
- brake
- clutch
- temperatures
- pressures

### SessionInfo
Slow-changing metadata and session structures.

Examples:
- track info
- driver info
- car metadata
- SDK-provided warning / critical thresholds where available

### Field selection model
Widgets should bind by source plus field path.

Examples:
- `TelemetryRaw:Speed`
- `TelemetryRaw:Gear`
- `SessionInfo:WeekendInfo.TrackDisplayName`

The field catalog is currently code-driven and must remain stable without depending on prior runtime export files.

---

## Widget families

SuperOverlay will contain two major widget families.

### A. Generic widgets
Reusable widgets with configurable bindings and rules.

Examples:
- `One Value`
- `Label : Value List`
- later: bars, indicators, badges, traces

These widgets use shared backend concepts:
- field bindings
- base style
- rule evaluation
- prepared payload output

### B. Custom widgets
Domain widgets with unique logic and unique payloads.

Examples:
- `Shift LED Panel`
- `Pit Strategy`
- `Fuel Strategy`
- `Radar`
- `Relative`

Custom widgets still follow the same architecture pattern:

`Config -> Processor -> Prepared Payload -> Renderer`

The difference is only in widget-specific processor logic and payload shape.

---

## One Value widget

`One Value` is the first generic backend-driven widget.

It displays one bound value and receives already prepared visual state from backend processors.

### Core properties
- value binding
- background color
- border color
- border thickness
- foreground color
- corner radius per corner

### Rule model
Rules belong to backend processing, not the widget.

Rules may depend on:
- the displayed field
- another telemetry field
- SDK-provided warning / critical threshold fields

Example:
- display `Gear`
- evaluate severity using `RPM`
- backend returns text plus background color

---

## Label : Value List widget

Planned generic widget for rows like:

- `WATER: 118`
- `OIL: 126`
- `ABS: 3`
- `TC: 5`

Each row has a severity state:
- `Normal`
- `Warning`
- `Critical`

Severity may be computed from:
- manual rules
- SDK-provided warning / critical thresholds

For values like `WATER` and `OIL`, iRacing-provided warning and critical thresholds should be preferred when available.

---

## Editor and runtime modes

The product now has a strict mode split.

### RACE mode
- launches the active overlay in runtime mode
- overlay widgets remain topmost as required for racing use
- the master control widget itself is **not** required to stay topmost and may sit under the game window
- runtime mode must not expose editor commands in the overlay itself

### EDIT mode
- stops the active runtime overlay
- opens the editor for the current active layout
- the master control widget becomes topmost so editing controls remain reachable
- all overlay changes happen only in edit mode

### Mode intent
- `RACE` means: save current layout and launch the current overlay
- `EDIT` means: stop race overlay and enter layout editing
- `EXIT` means: close both editor/runtime and terminate the app cleanly

The active layout used by `RACE` and `EDIT` must be the same current layout document.

---

## Performance rules

Performance is a first-class architectural requirement.

### Non-negotiable principles
1. Show real telemetry values, not smoothed approximations.
2. Different workload classes must not share one execution path if they can block each other.
3. Heavy processors must never delay fast visual widgets.
4. Widgets must not re-render when prepared payload is unchanged.
5. UI thread must only apply prepared state and render.

### Threading / execution lanes
Do **not** create one thread per widget.

Instead, separate work by execution lane:
- `Fast`
- `Standard`
- `Heavy`

#### Fast lane
Examples:
- speed
- gear
- shift lights
- simple one-value widgets
- fast indicators

#### Standard lane
Examples:
- label/value lists
- threshold-driven status blocks
- temperatures
- ABS/TC displays

#### Heavy lane
Examples:
- pit strategy
- fuel strategy
- radar
- relative
- predictive/domain-heavy widgets

### UI thread rules
The UI thread must not do:
- SDK reads
- rule evaluation
- heavy layout calculations
- strategy calculations
- history computation
- file IO
- reflection-heavy hot path binding resolution

### Allocation rules
In hot paths, avoid:
- unnecessary LINQ in per-tick loops
- rebuilding visual trees per tick
- repeated reflection lookups
- new transient collections every update when avoidable
- repeated formatting work when output has not changed

### State handoff rules
- prefer atomic snapshot replacement
- avoid one giant global mutable lock
- keep raw ingestion isolated from processing lanes
- keep processing isolated from rendering

### Publish rules
- publish prepared payload only when changed
- unchanged widgets must not receive update calls
- payload equality should be the primary gate for UI work

### Graph rules
Fast graphs should use:
- fixed-size history buffers
- direct lightweight drawing
- short rolling history
- no heavyweight chart libraries

---

## Explicit anti-rules

The following are architectural anti-patterns and should be avoided:
- business logic inside widget controls
- direct telemetry access from widgets
- smoothing telemetry values just to make them look nicer
- one monolithic processor loop for all widget classes
- one heavy lock around all state
- thread-per-widget design
- chart-library-based fast telemetry traces
- re-applying unchanged widget state every tick

---

## Near-term roadmap

### Foundation
- formalize processing lanes
- formalize prepared state store
- formalize change-aware publisher

### Generic widgets
- finish backend-driven `One Value`
- add backend-driven `Label : Value List`

### Custom widgets
- `Shift LED Panel`
- `Pit Strategy`
- further custom racing widgets after the backend pipeline is stable

The architecture goal remains consistent:

**Readers and processors do the thinking. Widgets only render prepared truth.**
