# KS Health System (Pro) - Modular Vitality Framework

Updated: January 31, 2026

This system provides modular health, shields, status effects, visuals, and UI for many game types.
Use only what you need. Wire with C# events or UnityEvents.

---

## 1) What is included

Core
- Health (damage, heal, regen, invulnerability, mitigation)
- Damage types and resistances
- Shield absorption (optional)
- Downed / revive state (optional)

Status
- Poison, regeneration, speed boost
- Stacking modes: refresh, extend, stack

Events
- C# events for code
- UnityEvents for Inspector
- Optional ScriptableObject event channel

UI and Visuals (optional)
- Health bar, text, icon, controller, manager
- Screen flash, tint, shake, particles, audio, low-health warning

Profiles
- HealthProfileSO for plug-and-play presets

---

## 1.5) Recent updates

- Damage types + mitigation (flat/percent + resistances)
- Downed / revive state with events
- Status effect stacking rules
- HealthProfileSO presets
- C# events as primary API (UnityEvents still available)

---

## 2) Quick Start (minimal setup)

1) Add `HealthSystem` to your player/enemy.
2) (Optional) Add `ShieldSystem`, `StatusEffectSystem`, `HealthVisualSystem`.
3) (Optional) Add `HealthUIController` and UI components from `UI/`.
4) (Optional) Create a `HealthProfileSO` and apply it at runtime or in a setup script.

That is enough to be fully functional.

---

## 3) Recommended setup by game type

Shmup (Sky Force style)
- HealthSystem + ShieldSystem + HealthVisualSystem
- UI: simple health bar + boss bar (if needed)

Metroidvania
- HealthSystem + StatusEffectSystem + optional downed/revive
- UI: health bar + text

Tower Defense
- HealthSystem + damage types + resistances
- UI: world-space bars (many units)

RTS
- HealthSystem + damage types + resistances + upgrades (external)
- UI: world-space bars + selection UI

Action RPG
- HealthSystem + StatusEffectSystem + downed/revive
- UI: health + status icons

---

## 4) How to use events

Code (C# events)
- HealthSystem: `HealthChanged`, `DamageTaken`, `Healed`, `Death`, `Downed`, `Revived`
- ShieldSystem: `ShieldChanged`, `ShieldAbsorbed`, `ShieldDepleted`, `ShieldRestored`
- StatusEffectSystem: `EffectApplied`, `EffectExpired`

Inspector (UnityEvents)
- UnityEvents are still available in each system for drag-and-drop wiring.

Event Channel (optional)
- `HealthEventChannelSO` can be assigned if you prefer ScriptableObject-based events.

---

## 5) Damage types and mitigation

- `DamageType` includes Generic, Physical, Fire, Ice, Poison, Electric, True.
- Flat and percent mitigation are applied unless damage is True or IgnoreMitigation is set.
- Damage can optionally bypass shields.

---

## 6) Status effects and stacking

Each effect supports:
- Duration and tick interval
- Amount per tick
- Stacking mode (Refresh, Extend, Stack)
- Max stacks

Default behavior:
- Poison: stacks (up to 5)
- Regeneration: refresh
- Speed Boost: refresh

---

## 7) Downed / Revive

Optional flow:
- On lethal damage: enter Downed state instead of Death
- Revive restores health and exits Downed
- If timer expires: Death fires

---

## 8) Health Profiles

`HealthProfileSO` can configure:
- Health settings
- Mitigation and resistances
- Downed / revive settings
- Invulnerability settings
- Shield settings (if present)

Use this for fast setup across many enemies/units.

---

## 9) Common issues

- If no events fire, check if you subscribed to C# events or assigned UnityEvents.
- If a shield exists but damage hits health, ensure `ShieldSystem` is on the same object.
- If low health warning does not show, ensure `HealthVisualSystem` has a low health overlay image.

---

## 10) Notes on modularity

You can use any subset:
- Health only
- Health + UI
- Health + Shield + UI
- Health + Status + Visuals
- Full setup

This is designed to remain clear, simple, and easy to debug.
