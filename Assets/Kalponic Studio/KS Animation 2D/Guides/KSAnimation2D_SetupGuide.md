# KS Animation 2D – Integration Guide (v2.1)

This guide shows how to integrate the KS Animation 2D plugin into your Unity project. It covers the different ways to play animations (using Unity's Animator, Playables, or direct sprites), the supporting scripts, and how to debug the system.

---

## Overview of Plugin Modules

The plugin includes several components to handle animations in different ways:

- **AnimatorAnimationPlayer**: Plays animation clips on a Unity Animator without needing transitions. Uses AnimationProfile assets.
- **PlayableAnimatorComponent**: A Playables-driven sprite animator that uses the same profiles as the Animator version, so you can skip Animator controllers entirely.
- **KSAnimComponent**: A helper that automatically finds and provides the right animation player to your scripts.
- **FSM (Finite State Machine)**: Scripts like PlayerStateMachine and various state classes that decide which animation to play based on game events.
- **Signal System**: Scripts like PlayerAnimationSignalBus, InputHandler, and GroundChecker that send events to trigger animations.
- **Combat/Hitboxes**: Author per-frame hitboxes in `AnimationStateSO`; runtime `HitboxManager` + helpers (ContactReceiver/ProjectileSpawner) handle overlap events. See `Guides/CombatQuickStart.md`.

---

## Quick Start

1. **Choose your animation method**:
   - For clips: Use AnimatorAnimationPlayer with AnimationProfileSet.
   - For sprites: Use PlayableAnimatorComponent with CharacterTypeSO and AnimationProfileSet/AnimationProfiles.

2. **Set up the scene hierarchy**:
   ```
   Player (root GameObject)
   ├── PlayerAnimationSignalBus
   ├── InputHandler
   ├── GroundChecker (with Collider2D)
   ├── MovementController
   ├── PlayerController
   ├── PlayerStateMachine
   ├── KSAnimComponent (optional but recommended)
   └── Visual (child GameObject with SpriteRenderer and animation component)
   ```

3. **Connect the scripts**:
   - PlayerController needs references to InputHandler, GroundChecker, MovementController, and PlayerAnimationSignalBus.
   - PlayerStateMachine needs the signal bus; it will automatically find the animation player.

4. **Test it**:
   - Enter Play mode.
   - Check the Unity Animator window or console logs to see if animations change when you move or jump.

---

## Animation Methods

### Method 1: Using Unity Animator (Clip-Based)

**Best for**: Projects with existing Animator controllers.

1. **Prepare the Animator Controller**:
   - Create one animation clip per state (Idle, Walk, Jump, etc.).
   - Set the Entry state to Idle.
   - No transitions needed – the plugin handles switching.

2. **Create Animation Profiles**:
   - Make AnimationProfile or AnimationProfileSet assets.
   - For each animation ID, set the clip, fade time, and layer.

3. **Add AnimatorAnimationPlayer**:
   - Attach to the GameObject with the Animator.
   - Assign your profile set.

4. **How it works**:
   - PlayerStateMachine calls `Play(AnimationId.X)`.
   - AnimatorAnimationPlayer plays the clip using Unity's Playables.

**Troubleshooting**:
- Sprite not changing? Check if the clip is bound to the SpriteRenderer.
- Missing profile warning? Add an entry for that animation ID.
- State name mismatch? Copy the exact name from the Animator.

### Method 2: Using Playables (Sprite-Based)

**Best for**: New projects or when you want to avoid Animator controllers.

1. Attach PlayableAnimatorComponent to your sprite GameObject.
2. Assign a CharacterTypeSO and AnimationProfileSet (or individual AnimationProfiles).
3. Drive it via AnimationIds from your FSM, or call `Play("stateName")` directly for quick sprite previews—no Animator controller needed.

**When to use**: For enum-based FSMs or direct sprite control without setting up Animator controllers.

**Combat/Hitboxes**: If you need melee/ranged detection, add `HitboxManager` + helpers to the same object and author hitboxes in your `AnimationStateSO` assets. Walkthrough: `Guides/CombatQuickStart.md`.

---

## Script Roles (Single Responsibility Principle)

Each script has a clear job:

- **InputHandler**: Reads player input and sends movement events.
- **GroundChecker**: Checks if the player is on the ground and sends grounded/fall events.
- **MovementController**: Handles physics like jumping and velocity.
- **PlayerController**: Combines input and ground info to send jump/attack events.
- **PlayerAnimationSignalBus**: Acts as a hub for all animation-related events.
- **PlayerAnimationContext**: Stores data like movement direction and attack requests.
- **PlayerStateMachine & States**: Pure animation logic – decides which animation to play based on events.

---

## Debugging Tips

1. **Check the Animator window**: When an animation plays, you should see the highlight move to that clip.

2. **Watch console logs**: The plugin logs warnings for missing profiles or disabled components. Fix these first.

3. **Add temporary logs**: If animations don't trigger, log events in PlayerController or state classes.

4. **Test step-by-step**:
   - Move: Should switch to Walk.
   - Jump: Should play Jump, then Fall if not landing.
   - Land: Should return to Idle or Walk.

---

## Adding New Features

- **New states**: Create subclasses of PlayerAnimationState or PlayerAirState (e.g., PlayerWallSlideState).
- **Combos**: Modify PlayerAttackState to chain animations.
- **Multiple characters**: Swap profile sets or prefabs.
- **Advanced**: Use PlayableAnimatorComponent for Playables-only workflows.

---

## Future Improvements

See `Documents/3.Suggestions and Improvements/KSAnimation2D_TODO.md` for planned features like setup wizards, better inspectors, and sample scenes.

This setup keeps your animation system clean, code-driven, and easy to extend!
