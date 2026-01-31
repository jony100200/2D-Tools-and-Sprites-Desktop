# KS SpriteEffects

Quick Start — apply a Material to sprites (single or folder)

Purpose
- Apply shader/material effects to 2D sprite assets and export the processed results as individual sprite PNGs.
- Designed for editor workflows: batch-process a folder of sprites or process a single sprite.

Requirements
- Unity Editor (the tool is editor-only and lives under the `Tools` menu)
- Works with Sprite assets (not raw Texture2D sheets). Use sprites that have been sliced/imported.

How to Use
1. Open the tool: `Tools → Kalponic Studio → KS Sprite Effects`.
2. Choose the `Processing Scope`: `SingleTexture` (single sprite) or `Folder` (all sprites in a folder).
3. For a single sprite: assign the `Sprite` field. For folder mode: pick the input folder containing sprite assets.
4. Select an `Output Folder` where processed sprites will be saved.
5. Assign the `Effect Material` (the material/shader that will be applied).
6. (Optional) Enter a file `Prefix` and/or `Suffix`. Leave blank for no extra text.
7. Click `Process Sprite` or `Process Folder`.

What it does
- Extracts the exact sprite rectangle from the source texture, applies the material using a RenderTexture + Graphics.Blit pipeline, and saves each processed sprite as its own PNG.
- Does NOT create atlases by default — each result is a separate sprite asset to avoid stacking artifacts.

Limitations
- Currently only supports processing Sprites (not raw texture sheets). Slicing is planned for the future.
- Editor-only: runs in Unity Editor (menu tool) and will not be included in builds.

Troubleshooting
- If no output appears, ensure sprites are valid and `Output Folder` is set to a folder inside `Assets/`.
- If material effects look wrong, check shader properties and ensure the material works with 2D textures.

Contact & License
- Free to use; please retain author attribution when redistributing.
- Report issues or request features via the project repo or the contact method in project metadata.
