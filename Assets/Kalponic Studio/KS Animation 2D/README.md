# KS Animation 2D (Kalponic Studio)

KS Animation 2D is a Playables-driven sprite animation toolkit. It lets you play animations without Animator controllers, while still supporting Animator workflows when needed.

## Core Components
- `PlayableAnimatorComponent`: MonoBehaviour wrapper around `PlayableAnimator` for Playables-first sprite playback.
- `AnimatorAnimationPlayer`: Animator-based player that runs clips via Playables (no controller transitions required).
- `KSAnimComponent`: Resolver that finds an `IAnimationPlayer` on the object/children and forwards play/loop events.
- `AnimationStateSO`: Sprite-frame state assets (name, FPS, loop, priority, events) for direct sprite playback.
- `AnimationProfile` / `AnimationProfileSet`: Map `AnimationId/AnimationType` to clips/states for both Animator and Playables.
- `IAnimator` / `IAnimationPlayer`: Shared interfaces for gameplay code to stay decoupled from the concrete player.

## Editor Tools (menu: `Tools/Kalponic Studio/Animation/KS Animation 2D`)
- Install / Validate / Demo Scene: installer utilities.
- Auto Setup: adds required components (SpriteRenderer, nodes/attachments, PlayableAnimatorComponent).
- Profile Wizard: generates profiles/profile sets.
- Sprite Animation Editor: frame/event editor for `AnimationStateSO`.
- Live Debugger: shows current animation, time, speed, and history for any `IAnimationDiagnostics`.
- Hub: UIToolkit launcher that links to the above tools.

## Data & Events
- Sprite states: `AnimationStateSO` (sprites, frame durations, loop, priority, UnityEvents, per-frame hitboxes + metadata).
- Clips: `AnimationProfile` entries (clip, fade, layer) bundled via `AnimationProfileSet`.
- Hitboxes: Optional per-frame hitboxes (type/offset/size/metadata) on `AnimationStateSO` for 2D combat triggers; collision matrix asset to filter interactions.
- Events: Per-frame UnityEvents + loop-once callbacks on `AnimationStateSO`, plus `AnimationUnityEvents`/`AnimationEventHub` for start/loop/complete callbacks.

## FSM (animation-focused)
- Class-based FSM: `AnimationStateMachine` + `AnimationState` base, with a ScriptableObject adapter (`ScriptableAnimationState`).
- Runner: `AnimationStateMachineRunner` ticks the FSM against any `IAnimator`.
- Priority-aware transitions; use it to drive animation state switches, not gameplay logic.

## Quick Start
1) Choose a player:
   - Clips: add `AnimatorAnimationPlayer`, assign `AnimationProfileSet`.
   - Sprites: add `PlayableAnimatorComponent`, assign `CharacterTypeSO` + profiles.
2) (Optional) Add `KSAnimComponent` to expose a single player reference to scripts.
3) Use `Play(AnimationType/AnimationId/stateName)` from gameplay code; keep logic outside the animation plugin.
4) Open the Hub (`Tools/Kalponic Studio/Animation/KS Animation 2D/Hub`) to reach installers, auto-setup, editors, hitbox collision matrix. Use Live Debugger to inspect playback (events/hitboxes info).
5) Per-frame/loop events: author them on `AnimationStateSO`; the Sprite Animation Editor shows per-frame UnityEvents and hitboxes, and the Live Debugger reports event availability.
6) Hitboxes/combat: see `Guides/CombatQuickStart.md`. Author hitboxes, add `HitboxManager` + helpers (`HitboxContactReceiver`, `ProjectileSpawner`), and use a collision matrix to filter interactions.

## Guides
- Setup: `Guides/KSAnimation2D_SetupGuide.md`
- Playables quick start: `Guides/PlayableAnimatorQuickStart.md`
- Progress & TODO: see `Documents/3.Suggestions and Improvements/KS2DAnimProgressTracker.md` and `KSAnimation2D_TODO.md`

## Notes
- PlayableSpriteAnimator has been removed; use `PlayableAnimatorComponent`.
- Legacy editor windows (timeline/node/about/attachment toggles) were trimmed; rely on the Hub + Sprite Animation Editor + Live Debugger.

