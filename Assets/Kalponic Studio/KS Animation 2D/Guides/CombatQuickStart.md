# KS Animation 2D – Combat Quick Start

This walkthrough wires a sprite-based character with per-frame hitboxes, runtime detection, and simple SFX/VFX/projectile reactions.

## 1) Author Data
1. Create/ensure a `CharacterTypeSO` and `AnimationStateSO` assets for your character.
2. Open **Sprite Animation Editor** (Hub → Sprite Animation Editor).
3. Select your `AnimationStateSO` (e.g., `Attack_01`).
4. In the hitbox foldout:
   - Add hitboxes on the active frames (type = `Hit` for attacks; `Hurt` for damageable areas).
   - Set size/offset; fill metadata (damage/knockback/hitstun/fxId/projectileOrigin if needed).
   - Per-frame UnityEvents can also trigger SFX/VFX if preferred.
5. (Optional) Set per-animation `attackDefaults` on the state for baseline damage/poise/knockback.
6. Save the asset.

## 2) Set Up the Character Prefab
On your character GameObject:
1. Add `SpriteRenderer` (and Animator if you also use rigged clips).
2. Add `PlayableAnimatorComponent`.
   - Assign `CharacterTypeSO`.
   - Assign profiles/profile set.
3. Add `HitboxManager`.
   - Optionally assign a `HitboxCollisionMatrix`.
   - Tune `updateInterval` / `cullDistance` for perf.
4. (Optional) Add helpers:
   - `HitboxContactReceiver` (filters + fires UnityEvents/audio/VFX on contacts; can also use a collision matrix).
   - `ProjectileSpawner` (spawns prefabs using hitbox metadata/projectile origin).

## 3) Runtime Wiring
When you play an attack state:
- `HitboxManager` spawns trigger colliders based on the current frame’s hitboxes.
- On overlap (Hit vs Hurt), a `HitboxContact` is raised containing: source/target types, point, damage, knockback, hitstun, fxId, projectileOrigin.

Handle contacts in one of two ways:
- Via `HitboxContactReceiver` UnityEvent: hook to methods that apply damage/knockback, play SFX/VFX, or call `ProjectileSpawner.SpawnFromContact`.
- Via code: subscribe to `HitboxManager.Contact` and apply your combat logic there.

## 4) Quick Test Checklist
- In Play Mode, run an attack animation and verify:
  - Hitboxes render in the editor overlay.
  - Contacts fire (log or UI).
  - SFX/VFX/projectiles trigger as expected.
  - Collision matrix filters unwanted interactions.

## 5) Tips
- Keep movement physics separate: hitboxes are triggers for combat, not movement colliders.
- Use `attackDefaults` for common values, override per-hitbox when needed.
- Use per-frame UnityEvents for precise SFX/VFX; use contacts for damage/knockback logic.
- For performance, enable `updateInterval` and `cullDistance` on `HitboxManager` for off-screen characters.
