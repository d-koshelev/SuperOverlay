# SuperOverlay Vision

## Purpose

SuperOverlay is a sim racing overlay platform built around a composable layout system.

The goal is not to ship a fixed set of monolithic widgets, but to provide a system where the user can build a personal HUD from independent dashboard items such as gear, speed, RPM, pedals, fuel, relative, and other racing-related UI elements.

The project should remain clean, extensible, and understandable as it grows.

---

## Core Idea

An overlay is a layout composed of independent dashboard items.

Each item is a self-contained visual unit with its own settings and rendering behavior.  
The user should be able to combine these items into a custom interface that matches personal preferences, car type, sim type, or race situation.

Examples:

- gear + speed + RPM
- pedals next to the central dashboard
- relative at the top
- fuel and strategy on the side

The layout should be flexible rather than predefined.

---

## Product Direction

SuperOverlay is designed as a reusable overlay system, not as a single-sim hardcoded app.

iRacing is the first telemetry source and host application, but the architecture should allow future support for other sims without rewriting the dashboard layer or the layout system.

The long-term direction is:

- reusable layout builder
- reusable dashboard item library
- replaceable telemetry providers
- user-shareable layouts
- clean separation between UI and data source

---

## Architectural Principles

### 1. Layout is independent from telemetry

The layout system must not know anything about iRacing-specific APIs or sim-specific SDK concepts.

A layout only describes:

- which items exist
- where they are placed
- how they are linked
- what settings they use

### 2. Dashboard items are independent from telemetry source

Dashboard items must consume a unified runtime state, not raw sim SDK data.

This allows the same dashboard library to be reused by different hosts and telemetry sources.

### 3. Telemetry is an adapter layer

Telemetry integration is a separate concern.

A sim-specific project is responsible for:

- connecting to the sim
- reading data
- mapping it into a unified runtime state
- passing that state into the overlay system

### 4. The system must be composable

New dashboard items must be addable without changing the layout engine.

New telemetry sources must be addable without changing the dashboard library.

### 5. The system must remain understandable

The project should avoid architecture that looks impressive but becomes hard to maintain.

The structure should stay small, explicit, and easy to reason about.

---

## Solution Shape

The system is built as three projects:

### SuperOverlay.LayoutBuilder
Responsible for layout structure, runtime hosting of layout items, and later editing features such as drag, resize, snapping, and glue.

### SuperOverlay.Dashboards
Responsible for the dashboard item library, dashboard runtime contracts, and item registry.

### SuperOverlay.iRacing
Responsible for iRacing telemetry, mapping, startup wiring, and hosting the overlay application.

This split exists to protect the architecture from sim-specific coupling.

---

## User Experience Goal

The user experience should eventually allow a driver to:

- choose available dashboard items
- place them freely on the overlay
- resize and align them
- attach items to each other
- save and load layouts
- reuse layouts across sessions
- potentially share layouts with other users

The overlay should feel modular, fast, and purpose-built for sim racing.

---

## Long-Term Goal

The long-term goal is to create an open and extensible overlay platform that is:

- modular
- sim-agnostic at the dashboard/layout level
- easy to expand
- easy to maintain
- suitable for open-source development

SuperOverlay should grow as a system, not as a pile of special cases.