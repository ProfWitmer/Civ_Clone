# Civ IV Clone Roadmap

## Project Notes (Stable Decisions)
- Target platform: macOS (Mac Studio).
- Visual style: match Civ IV (2.5D/3D isometric look).
- Data-driven content: JSON/CSV preferred.
- Approach: balanced (keep architecture clean, but prioritize playable milestones).
- Headless code coverage/test runs via `make test-cover` currently fail due to Unity licensing in batch mode. Run tests/coverage in the editor and use the generated report under `CodeCoverage/Report`.

## First Playable Milestone
- Explore a random map with a unit, end turns, and found a city.

## Phase 1: Foundation + First Playable
1. Project structure: `Simulation/`, `Presentation/`, `Infrastructure/`, `Data/`.
2. Data-driven content: JSON/CSV loaders for terrain, units, buildings.
3. Map: random map gen, tile data, tile rendering (Civ IV-style).
4. Camera + Input: pan/zoom/edge scroll, unit select, click-to-move.
5. Turn system: end turn, movement reset, turn counter.
6. Fog of war (basic): explored vs visible tiles.
7. City founding: settler -> city on tile, city view placeholder.
8. Save/load (basic): serialize state to JSON.

## Phase 2: Core Gameplay Loop
- Units: move, combat, basic promotions.
- Cities: production, growth, yields.
- Improvements: workers, basic terrain improvements.
- Combat resolution and UI feedback.

## Phase 3: Economy & Tech
- Tech tree scaffolding.
- Civics system scaffold.
- Resource system and trade routes (simple).

## Phase 4: AI & Diplomacy
- Basic AI turns (move, found, build).
- War/peace and simple diplomacy.

## Phase 5: UI & UX Polish
- HUD, tooltips, menus, minimap.
- End turn flow and alerts.

## Phase 6: Content & Balance
- Expand units/buildings/techs.
- Scenario hooks.

## Phase 7: Release Prep
- Performance pass.
- Bug fixes.
- Packaging.

## Update Log
- 2026-02-05: Initial roadmap created and agreed.
- 2026-02-05: Added JSON/CSV data loading and initial data files.
- 2026-02-05: Added isometric map rendering, camera pan/zoom/edge scroll, placeholder tile textures, and hills elevation.
- 2026-02-05: Added camera drag, isometric tile hover/selection highlight, and terrain pattern swatches.
- 2026-02-05: Added right-mouse camera drag, unit-tile highlight hook, and outline shader for tiles.
- 2026-02-05: Tuned tile outline settings and made selected tile highlight override hover.

- 2026-02-07: Phase 2 complete (units, cities, improvements, combat, promotions, roads, tech selection).
- 2026-02-07: Phase 3 started - tech prerequisites, civics scaffold, resources and simple trade routes.
- 2026-02-07: Phase 4 started - basic AI production/research/worker actions and war declarations on combat.
- 2026-02-07: Added diplomacy panel toggles (G/7-9) and AI war-target movement.
- 2026-02-07: Added diplomacy status HUD line, basic AI peace offers, and city capture on entering enemy cities.
- 2026-02-07: Added city health/defense combat and diplomacy panel paging.
- 2026-02-07: Added city HP badge rendering and AI now prioritizes enemy cities when at war.
- 2026-02-07: Added city defense badge + low-HP outline and siege-style AI behavior (move adjacent before attack).
- 2026-02-07: Added defense shield icon and ranged bombardment vs cities.
- 2026-02-07: Added city hover tooltip and siege debuff (HP regen paused when adjacent enemy present).
- 2026-02-07: Added siege icon and dedicated HUD hover line for cities.
- 2026-02-07: Added unit hover HUD line and animated city HP bars.
- 2026-02-07: Phase 4 complete (AI + diplomacy baseline).
- 2026-02-11: Phase 5 complete (HUD layout, top bar yields, unit/city panels, tooltips, minimap + viewport, end turn flow, alerts, UI polish).
