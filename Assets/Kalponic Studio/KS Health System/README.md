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
3) (Optional) Add `HealthUIController` and UI components from `Runtime/UI/`.
4) (Optional) Create a `HealthProfileSO` and apply it at runtime or in a setup script.

That is enough to be fully functional.

---

## 3) Folder structure (audit)

Current structure is modular and easy to extend:

- `Runtime/Core` - health, shields, status effects, profiles, controllers
- `Runtime/Visuals` - visual feedback system
- `Runtime/UI` - UI components and UI docs
- `Extensions` - optional advanced features (safe to ignore)
- `Resources` - ScriptableObject assets (optional)
- `Tests` - playmode tests

---

## 4) Step-by-step setup (scene)

### Beginner Scene Setup (Hierarchy)
1) Create a Player GameObject
   - Hierarchy: Right-click -> Create Empty
   - Name: Player
   - Add components:
     - SpriteRenderer
     - Rigidbody2D (2D) or Rigidbody (3D)
     - Your movement script (optional)

2) Add the health system
   - Select Player
   - Add Component -> HealthSystem

3) Optional modules (add only what you need)
   - Add Component -> ShieldSystem
   - Add Component -> StatusEffectSystem
   - Add Component -> HealthVisualSystem

4) Create a UI Canvas (optional but common)
   - Hierarchy: Right-click -> UI -> Canvas
   - Unity will also create an EventSystem if missing

5) Add Health UI
   - Select the Canvas
   - Create an empty child: Right-click -> Create Empty (name: HealthUI)
   - Add Component -> HealthUIController on HealthUI
   - Create children under HealthUI:
     - UI -> Slider (add HealthBar script)
     - UI -> Text or TextMeshPro (add HealthText script)
     - UI -> Image (add HealthIcon script)
   - Assign the HealthSystem reference in HealthUIController

6) (Optional) Create an Event Channel
   - Project window: Right-click -> Create -> Kalponic Studio -> Health -> Event Channel
   - Assign it to HealthSystem and HealthVisualSystem if you prefer SO events

### Step 1: Create a unit
1) Create a GameObject (Player or Enemy).
2) Add `HealthSystem`.
3) Optional: Add `ShieldSystem` and `StatusEffectSystem`.

### Step 2: Add visuals (optional)
1) Add `HealthVisualSystem`.
2) Assign:
   - `Main Renderer` (SpriteRenderer)
   - UI Images for damage/heal flash (optional)
   - Particle systems (optional)
   - Audio source + clips (optional)

### Step 3: Create UI (optional)
1) Create a Canvas.
2) Add `HealthUIController` to a parent UI object.
3) Add one or more UI children:
   - `HealthBar`
   - `HealthText`
   - `HealthIcon`
4) Assign the `HealthSystem` to the controller.

### Step 4: Optional event channel
1) Create `HealthEventChannelSO`.
2) Assign it to any systems you want to drive via ScriptableObject events.

---

## 5) Code usage (common patterns)

### A) Subscribe to events (recommended)
```csharp
using UnityEngine;
using KalponicStudio.Health;

public class HealthListener : MonoBehaviour
{
    [SerializeField] private HealthSystem health;

    private void OnEnable()
    {
        health.HealthChanged += OnHealthChanged;
        health.DamageTaken += OnDamageTaken;
        health.Death += OnDeath;
    }

    private void OnDisable()
    {
        health.HealthChanged -= OnHealthChanged;
        health.DamageTaken -= OnDamageTaken;
        health.Death -= OnDeath;
    }

    private void OnHealthChanged(int current, int max) { }
    private void OnDamageTaken(int amount) { }
    private void OnDeath() { }
}
```

### B) Deal damage (simple)
```csharp
health.TakeDamage(25);
```

### C) Deal damage with type and options
```csharp
DamageInfo info = new DamageInfo
{
    Amount = 40,
    Type = DamageType.Fire,
    BypassShield = false,
    IgnoreMitigation = false,
    Source = gameObject,
    SourceTag = "Enemy"
};

health.TakeDamage(info);
```

### D) Heal or revive
```csharp
health.Heal(20);
health.Revive(50);
```

### E) Apply status effects
```csharp
statusEffectSystem.ApplyPoison(5f, 3);
statusEffectSystem.ApplyRegeneration(6f, 4);
statusEffectSystem.ApplySpeedBoost(8f, 1.5f);
```

### F) Apply a Health Profile
```csharp
public class HealthProfileApplier : MonoBehaviour
{
    [SerializeField] private HealthProfileSO profile;
    [SerializeField] private HealthSystem health;
    [SerializeField] private ShieldSystem shield;

    private void Awake()
    {
        profile.ApplyTo(health);
        profile.ApplyTo(shield);
    }
}
```

---

## 6) UI setup (step-by-step)

### Health bar + text + icon
1) Create a Canvas.
2) Add `HealthUIController` to a parent UI object.
3) Add children:
   - `HealthBar` (Slider/Image based)
   - `HealthText` (TextMeshPro or Text)
   - `HealthIcon` (Image)
4) Assign the `HealthSystem` to `HealthUIController`.

Tips:
- `HealthUIController` can auto-find children if enabled.
- Use `HealthUIManager` for multiple units.

---

## 7) Extensions (optional, Pro glue)

These live in `Extensions/` and can be added only when you need them.

Combat
- `TeamComponent` (team + faction + friendly fire flag)
- `DamageRouter` (team-aware damage application)
- `DamageSourceInfo` + `DamageRules`

Tip: Leave `FactionId` empty if you only want team-based checks.

Persistence
- `HealthSnapshotComponent` + `IHealthSerializable`
- `HealthSnapshot` (capture/restore health, shield, statuses)

UI
- `WorldSpaceHealthBar`
- `BossHealthBar`

---

## 8) Recommended setup by game type

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

## 9) How to use events

Code (C# events)
- HealthSystem: `HealthChanged`, `DamageTaken`, `Healed`, `Death`, `Downed`, `Revived`
- ShieldSystem: `ShieldChanged`, `ShieldAbsorbed`, `ShieldDepleted`, `ShieldRestored`
- StatusEffectSystem: `EffectApplied`, `EffectExpired`

Inspector (UnityEvents)
- UnityEvents are still available in each system for drag-and-drop wiring.

Event Channel (optional)
- `HealthEventChannelSO` can be assigned if you prefer ScriptableObject-based events.

---

## 10) Damage types and mitigation

- `DamageType` includes Generic, Physical, Fire, Ice, Poison, Electric, True.
- Flat and percent mitigation are applied unless damage is True or IgnoreMitigation is set.
- Damage can optionally bypass shields.

---

## 11) Status effects and stacking

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

## 12) Downed / Revive

Optional flow:
- On lethal damage: enter Downed state instead of Death
- Revive restores health and exits Downed
- If timer expires: Death fires

---

## 13) Health Profiles

`HealthProfileSO` can configure:
- Health settings
- Mitigation and resistances
- Downed / revive settings
- Invulnerability settings
- Shield settings (if present)

Use this for fast setup across many enemies/units.

---

## 14) Common issues

- If no events fire, check if you subscribed to C# events or assigned UnityEvents.
- If a shield exists but damage hits health, ensure `ShieldSystem` is on the same object.
- If low health warning does not show, ensure `HealthVisualSystem` has a low health overlay image.

---

## 15) Notes on modularity

You can use any subset:
- Health only
- Health + UI
- Health + Shield + UI
- Health + Status + Visuals
- Full setup

This is designed to remain clear, simple, and easy to debug.

---

## 16) Separation of Function vs Visuals (recommended)

Keep gameplay logic separate from visuals:
- Core logic: `HealthSystem`, `ShieldSystem`, `StatusEffectSystem`
- Visuals/feedback: `HealthVisualSystem`
- UI display: `HealthUIController` + UI components

Core systems should never depend on visuals or UI.
Visuals and UI should only listen to events.
