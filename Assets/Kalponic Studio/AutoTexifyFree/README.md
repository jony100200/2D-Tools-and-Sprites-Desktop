# AutoTexify Free

AutoTexify Free streamlines material creation for texture and sprite assets directly inside the Unity Editor.

## Features
- Drag-and-drop folder selection for source textures and output locations
- Batch material generation with optional recursive search
- Preview mesh grid with configurable spacing, grid size, and primitive type
- Works with URP Lit or any shader you assign (falls back to Standard)
- Saves to a custom folder or an auto-created `Materials` subfolder next to your textures

## Requirements
- Unity Editor Coroutines package (`com.unity.editorcoroutines`) installed via Window > Package Manager > Add package by name...
- Editor-only utility; supports PNG, JPG, JPEG, and TGA textures

## Quick Start
1. Open `Tools > Kalponic Studio > Auto Base Material Maker`.
2. Drag a texture folder into **Texture Folder** (or click **Pick Folder**).
3. Optionally drag a save folder into **Materials Save Folder**, or leave empty to use the default `Materials` subfolder.
4. Choose shader and preview settings, then click **Generate Materials & Preview**.
5. Use **Delete All Previews In Scene** to clean up preview meshes when finished.

## Tips
- Enable **Recursive Search** to include subfolders.
- Cancelling the process keeps already-created materials intact.
- Assign custom shaders by dragging them into the **Material Shader** field before generating.

## Support & License
- Uses only Unity packages; no third-party plugins required.
- Free to use unless otherwise noted in your project. For support or feedback, please reach out via the publisher contact information.
