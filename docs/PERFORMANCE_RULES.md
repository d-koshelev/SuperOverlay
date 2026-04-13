# SuperOverlay Performance Rules

## Required rules

1. Widgets render prepared payload only.
2. Widgets never read telemetry directly.
3. Widgets never evaluate business rules.
4. UI thread only applies prepared state and renders.
5. Raw telemetry stays raw; do not smooth values for cosmetic reasons.
6. If prepared payload is unchanged, do not publish and do not re-apply.
7. Heavy processors must never delay fast widgets.
8. Fast telemetry reading and slow session reading remain isolated.
9. Prefer atomic snapshot replacement to long shared-state locks.
10. Hot paths should minimize allocations and reflection.

## Execution lane model

Use workload lanes, not one thread per widget.

- `Fast`
  - speed, gear, shift LEDs, fast indicators
- `Standard`
  - one-value, label/value lists, thresholds, status blocks
- `Heavy`
  - pit strategy, radar, relative, predictive widgets

## Publish model

Publisher should compare the newly prepared payload to the last published payload.

If equal:
- skip publish
- skip widget apply
- skip avoidable UI invalidation

## Graph model

Fast graphs should use:
- fixed-size history buffers
- short rolling windows
- direct lightweight drawing
- no heavy chart controls
