# KS Animation 2D – Feature Guide

This plugin provides Playables-driven sprite animation, Animator support, editor tools, and an animation-focused FSM.

## Playback Options
- **PlayableAnimatorComponent**: Play sprites via Unity Playables (no Animator controller). Use `CharacterTypeSO` + `AnimationProfileSet`/`AnimationProfiles`. Implements `IAnimator`/`IAnimationPlayer`.
- **AnimatorAnimationPlayer**: Play Animator clips via Playables without transitions. Use `AnimationProfileSet`.
- **KSAnimComponent**: Resolves any `IAnimationPlayer` on the object/children and forwards play/loop events.
- **Interfaces**: `IAnimator` (state/time/sequence) and `IAnimationPlayer` (play by id/type) keep gameplay decoupled.

## Data
- **AnimationStateSO**: Sprite frames, FPS, loop, priority, per-frame durations, per-frame UnityEvents, loop-once UnityEvent, per-frame hitboxes/metadata, and attack defaults.
- **CharacterTypeSO**: Maps logical state names to `AnimationStateSO` assets and exposes `AllStates`.
- **AnimationProfile / AnimationProfileSet**: Map `AnimationId/AnimationType` to clips or state names (used by both players).

## Editor Tools (menu: Tools + Kalponic Studio + Animation + KS Animation 2D)
- **Hub**: Launch common tools.
- **Install / Validate / Demo Scene**: Setup utilities.
- **Auto Setup**: Adds SpriteRenderer, nodes/attachments, PlayableAnimatorComponent.
- **Profile Wizard**: Build profiles/profile sets.
- **Sprite Animation Editor**: Preview (zoom + checkerboard), scrubber, per-frame durations, per-frame UnityEvents, loop-once event, frame thumbnails.
- **Live Debugger**: Shows current animation id/state, time, speed, transitions, and state event availability.
- **Character Editor**: Manage CharacterTypeSO animations, attack defaults, hitbox/event summaries; open related tools.

## FSM Utilities
- **AnimationStateMachine & AnimationState**: Class-based FSM with priority-aware transitions; adapter for `AnimationStateSO`.
- **AnimationStateMachineRunner**: MonoBehaviour to tick the FSM against any `IAnimator`.
- **Buffers/Combos**: `InputBuffer`, `ComboChain`, and selectors in `FsmUtilities`.

## Events & Diagnostics
- Per-frame UnityEvents and loop-once callbacks (from `AnimationStateSO`) fired by `PlayableAnimator`.
- `AnimationUnityEvents` and `AnimationEventHub` for start/loop/complete callbacks.
- `IAnimationDiagnostics` snapshots with transition history; surfaced in Live Debugger.
- Hitboxes (per-frame type/offset/size) on `AnimationStateSO`, previewed in the Sprite Animation Editor; runtime `HitboxManager` raises contacts via callbacks.
- Contact helpers: `HitboxContactReceiver` to trigger UnityEvents/audio/VFX on filtered contacts; collision matrix asset for interaction rules.

## Quick Use
1) Add either `PlayableAnimatorComponent` (sprites) or `AnimatorAnimationPlayer` (clips).
2) Assign `CharacterTypeSO` + `AnimationProfileSet` (or profiles).
3) (Optional) Add `KSAnimComponent` to expose a single player reference to code.
4) Open the Hub → Sprite Animation Editor to edit frames/events; use Live Debugger to inspect at runtime.

