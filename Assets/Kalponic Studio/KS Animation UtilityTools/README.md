# KS UtilityTools

Small collection of editor and runtime utility helpers.

Purpose

Quick Start
1. Open the folder in the Project window to inspect available utilities.
2. Many utilities are used directly from scripts; check each tool's example usage in its source file.
3. For editor tools, look under `Tools` menu or the Editor folder for custom windows.

What to expect

Troubleshooting

License & Contact
Purpose
- A focused set of Unity Editor utilities (under `Editor`) that speed up common AnimationClip and 2D sprite animation tasks.

This README documents each tool, how to run it from the Unity Editor, what it changes, and important safety notes.

**What is included**
- **`AnimationClipDuplicator`**: duplicate AnimationClip assets and clips embedded in FBX files.
	- Menu: Tools → Kalponic Studio → Duplicate Animation Clips
	- Path: [Assets/Kalponic Studio/KS UtilityTools/Editor/AnimationClipDuplicator.cs](Assets/Kalponic Studio/KS UtilityTools/Editor/AnimationClipDuplicator.cs)
	- Behavior: loads clips from a selected folder or selected assets, makes deep copies of each clip (using `EditorUtility.CopySerialized`) and writes new `.anim` assets to a destination folder. Generates unique asset names.
	- Use when: you need editable copies of runtime/FBX-provided animation clips without modifying originals.

- **`AnimationClipRenamer`**: rename AnimationClip assets in-place.
	- Menu: Tools → Kalponic Studio → Rename Animation Clips
	- Path: [Assets/Kalponic Studio/KS UtilityTools/Editor/AnimationClipRenamer.cs](Assets/Kalponic Studio/KS UtilityTools/Editor/AnimationClipRenamer.cs)
	- Behavior: loads selected clips or all clips in a folder and renames each asset by removing a specified suffix (default `_Duplicate_NoRootTz`) via `AssetDatabase.RenameAsset`.
	- Use when: you want consistent naming across many clips. Note: this edits the asset name (references may be affected).

- **`AnimClipGen2D`**: generate starter AnimationClip assets for grouped sprites.
	- Menu: Tools → Kalponic Studio → Anim Clip Gen 2D
	- Path: [Assets/Kalponic Studio/KS UtilityTools/Editor/AnimClipGen2D.cs](Assets/Kalponic Studio/KS UtilityTools/Editor/AnimClipGen2D.cs)
	- Behavior: scans a folder for `Sprite` assets, groups sprites by their `Texture2D`, and creates an empty `.anim` (one per texture) with the chosen frame rate and loop setting. Clips are saved without keyframes so you can add frames manually or with an animator tool.
	- Use when: you have sprite sheets / multiple sprites split from the same texture and want quick clip placeholders.

- **`RemoveRootMotion`**: duplicate AnimationClips and remove root Z-position animation curves.
	- Menu: Tools → Kalponic Studio → Remove Root Motion
	- Path: [Assets/Kalponic Studio/KS UtilityTools/Editor/RemoveRootMotion.cs](Assets/Kalponic Studio/KS UtilityTools/Editor/RemoveRootMotion.cs)
	- Behavior: duplicates selected clips (or all clips found in a folder) into a target folder (default `Assets/NoRMClips`) and removes curves named `RootT.z` or `m_LocalPosition.z` from the duplicated asset via `AnimationUtility.SetEditorCurve(clip, binding, null)`.
	- Use when: you need copies of clips with root-motion Z removed (for 2D workflows or to prevent Z position drift).

**Quick start (Unity Editor)**
1. In Unity, open the Project window and select a folder or select specific `.anim`/FBX assets.
2. Open the tool from the top menu: `Tools → Kalponic Studio → <ToolName>`.
3. Use the window to `Load Selected` or `Load from Source Folder`, set destination path (if applicable) and run the action button.

**Important behavior & safety notes**
- All scripts are Editor-only and live in `Editor` — they will not run in builds.
- `AnimationClipRenamer` renames assets in place; consider duplicating first if you need to preserve original names.
- `AnimationClipDuplicator` and `RemoveRootMotion` create duplicated assets by default (safer for originals). Duplicates are placed in the folder you specify.
- Always check the console for logs/errors after running a tool; failed compilation in Editor scripts prevents menu items from appearing.

**Troubleshooting**
- Tool not visible: ensure files are inside an `Editor` folder and Unity has recompiled without errors.
- Clips not found: confirm you selected the correct Project folder (drag the folder asset into the folder field) and that assets are `AnimationClip` or FBX `Model` assets containing clips.

**Examples / common workflows**
- Make editable copies of FBX clips:
	1. Select the FBX(s) in Project.
	2. Open `Duplicate Animation Clips` and click `Load Selected` → `Duplicate Animation Clips`.
	3. New `.anim` files appear in the chosen destination.

- Remove Z root motion and keep originals:
	1. Select `.anim` assets or a folder.
	2. Open `Remove Root Motion`, specify destination (default `Assets/NoRMClips`), click `Load Selected` → `Remove Root Motion`.

**Where to look for source**
- Editor scripts: [Assets/Kalponic Studio/KS UtilityTools/Editor](Assets/Kalponic Studio/KS UtilityTools/Editor)

License & contact
- These utilities are small single-purpose helpers. Use freely for development; contact the author for commercial licensing or support.

If you want, I can:
- Add example screenshots or GIFs showing the window flows
- Add confirmation dialogs for destructive actions (rename/overwrite)
- Add a small unit-checker to verify clip count before actions

---
Edited to document exact tool behavior and safe usage.
