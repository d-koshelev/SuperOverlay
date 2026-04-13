# SuperOverlay Implementation Plan

## Goal
Build a high-performance overlay platform with:
- raw data as source of truth
- backend-prepared presentation payloads
- dumb widgets
- change-aware publishing
- clean separation between generic widgets and custom widgets

## Core Principles
- No smoothing of telemetry values.
- Widgets never read telemetry directly.
- Widgets never evaluate business rules.
- UI thread only applies prepared payload and renders.
- Different workload classes must not share the same execution lane.
- If a widget payload did not change, do not apply it again.

## Execution Lanes
### Fast
Use for:
- speed
- gear
- shift lights
- pedal indicators
- other highly reactive widgets

### Standard
Use for:
- one value
- label:value list
- temperatures
- ABS/TC blocks
- warning/status blocks

### Heavy
Use for:
- pit strategy
- fuel strategy
- relative
- radar
- predictive widgets

## Delivery Sequence

### Phase 1 — Performance Foundation
#### 1. Prepared State Store
Build a prepared state layer:
- `WidgetId -> PreparedPayload`
- version or timestamp per entry
- optional hash/equality cache

Done when:
- runtime produces prepared payloads outside widgets
- prepared payloads are addressable by widget id

#### 2. Change-Aware Publisher
Publisher must compare current payload vs last published payload.
If equal, skip apply.

Done when:
- unchanged widgets do not receive `Apply`
- logs/debugging can show skipped vs published updates

#### 3. Processing Lane Metadata
Add workload metadata for processors/widgets:
- `Fast`
- `Standard`
- `Heavy`

Done when:
- each processor declares its lane
- runtime scheduling can isolate heavy widgets from fast ones

---

### Phase 2 — Generic Widget Family
#### 4. One Value Widget
Complete `One Value` as the first fully backend-driven generic widget.

Must support:
- display binding
- background color
- border color
- border thickness
- per-corner radius
- foreground color
- backend-evaluated rules

Done when:
- widget receives only prepared payload
- color/state logic is fully outside widget UI

#### 5. Label:Value List Widget
Build a list widget for blocks like:
- temperatures
- ABS / TC
- status lists

Row model:
- `Label`
- `Value`
- `Severity = Normal | Warning | Critical`

Severity source modes:
- manual rules
- threshold bindings
- SDK-driven thresholds where available

Done when:
- backend prepares row payloads
- widget renders rows only
- WATER/OIL can use iRacing-provided warning/critical thresholds when available

---

### Phase 3 — Lightweight Trace Widgets
#### 6. History Buffer Infrastructure
Add fixed-size history buffers for fast trace widgets.

Rules:
- fixed-size storage only
- no heavy chart libraries
- no large per-frame allocations

Done when:
- short rolling history can be stored for selected channels

#### 7. Fast Graph Widget Family
Build lightweight graph widgets using direct drawing.
Start with pedal traces:
- throttle
- brake
- clutch

Done when:
- graph uses fixed-size history
- rendering uses direct drawing/geometry
- graph path stays outside heavy widget logic

---

### Phase 4 — First Custom Widget
#### 8. Shift LED Panel
Build as the first full custom backend-driven widget.

Processor responsibilities:
- led activation
- color state
- blink state
- thresholds source selection

Widget responsibilities:
- render prepared led payload only

Done when:
- shift panel logic lives in processor, not widget UI
- widget updates only on payload change

---

### Phase 5 — Heavy Domain Widgets
#### 9. Pit Strategy
Build as a heavy-lane widget.

Processor responsibilities:
- pit windows
- fuel needed
- strategy options
- caution/stage logic where applicable

Done when:
- processor runs outside fast lanes
- fast widgets are unaffected by strategy calculations

#### 10. Fuel / Relative / Radar Family
Add other heavy widgets only after heavy-lane scheduling is stable.

Done when:
- heavy widgets do not delay fast widget updates

---

## Editor Roadmap
### Short-Term
- continue using current stable `RACE / EDIT / EXIT` flow
- keep authoring focused on generic widgets first

### Next Editor Features
1. full editing UI for `One Value` appearance
2. rule authoring UI for `One Value`
3. row editor for `Label:Value List`
4. threshold binding editor
5. preview/debug tools for prepared payload

Done when:
- generic widgets can be configured without manual JSON edits

---

## Non-Goals for Now
Do not add yet:
- smoothing / averaging telemetry values
- scriptable expression language
- per-widget threads
- chart libraries for fast traces
- business logic in widget controls
- direct telemetry access from widgets

---

## Acceptance Criteria for the Platform
The architecture is on track when all of the following are true:
- raw stores are the only source of truth
- widgets receive only prepared payload
- unchanged widgets are not re-applied
- heavy widgets cannot stall fast widgets
- runtime and editor operate on the same active layout
- generic widgets cover common cases
- custom widgets use the same backend processor/payload pattern
