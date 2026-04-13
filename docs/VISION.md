# SuperOverlay Vision

## Product direction

SuperOverlay is a sim-racing overlay platform built around:
- a reusable layout system
- a reusable widget platform
- strict backend-driven rendering
- high-performance runtime behavior

The project is not intended to become a pile of custom controls with logic embedded in UI.

The long-term direction is:
- reusable generic widgets
- reusable custom domain widgets
- sim-specific reader/host layers
- shared layout editing and runtime launching
- predictable performance under live telemetry load

---

## Core product idea

A user builds an overlay from widgets.

Some widgets are generic:
- one value
- label : value list
- status indicators
- later small traces and bars

Some widgets are domain-specific:
- shift LED panel
- pit strategy
- fuel strategy
- radar
- relative

All of them must fit the same backend model:

`Config -> Backend Processor -> Prepared Payload -> Renderer`

---

## UX direction

The top-level user workflow is mode-based:

- `RACE`
  - launches the current overlay in runtime mode
- `EDIT`
  - stops runtime and opens the editor for the current layout
- `EXIT`
  - closes the application cleanly

Overlay changes happen only in `EDIT` mode.
`RACE` always uses the current layout and starts the live overlay from it.

---

## Truthfulness principle

Displayed values should reflect the telemetry truth that was actually received.

The project should prefer:
- correct raw values
- backend-calculated severity
- SDK-provided thresholds where available

The project should avoid:
- decorative smoothing that changes what the driver really saw in telemetry
- UI-side logic that diverges from backend truth

---

## Performance principle

The platform is intended for live sim-racing use, so performance is not an optimization pass to add later. It is part of the design.

Target characteristics:
- cheap widgets
- cheap publishes
- isolated workload classes
- no unnecessary UI churn
- no heavy logic on the UI thread
- no re-render when payload did not change

---

## Architectural direction

The long-term architecture remains:

`Fast reader + Slow reader -> Raw stores -> Presentation processors -> Prepared state -> Change-aware publisher -> Widgets`

This makes it possible to support:
- generic widgets
- custom widgets
- debugging of expected display state
- replay/freeze later
- performance isolation by workload class

SuperOverlay should grow as a platform, not as a collection of unrelated hardcoded widgets.
