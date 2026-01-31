# KS Prefab Maker

A Unity Editor tool for batch creating prefabs from assets in a folder.

## Quick Start

1. Open the tool via `Tools > Kalponic Studio > KS Prefab Maker`.
2. Set the **Input Folder** to the folder containing your models or prefabs.
3. Set the **Output Folder** where prefabs will be saved.
4. Choose **Asset Type** (Models, Prefabs, or Both).
5. Enable **Add Box Colliders** if needed for 3D objects.
6. Adjust other options: Include Subfolders, Mirror Structure, Overwrite Existing.
7. Click **Create Workspace Scene** to prepare a new scene.
8. Click **Populate From Input** to instantiate assets in the scene (with progress for large batches).
9. Click **Create Prefabs** to save them as prefabs (validates and shows progress).
10. Use **Clear Workspace Objects** to clean up.

## Notes

- Supports FBX models and existing prefabs.
- Instances are placed at (0,0,0) for standard prefab placement.
- Automatically adds Box Colliders if enabled and none exist.
- Validates prefabs for mesh components before saving.
- Progress bars for population and creation; console logs summary and warnings.
- Filters assets by type for targeted processing.