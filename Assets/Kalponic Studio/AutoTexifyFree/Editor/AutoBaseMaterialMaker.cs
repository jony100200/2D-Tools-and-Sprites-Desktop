using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System;

namespace KalponicGames
{
#if UNITY_EDITOR
    public class AutoBaseMaterialMaker : EditorWindow
    {
        private string textureFolder = "Assets/Textures";
        private string materialsFolder = "";
        private int gridSize = 5;
        private float spacing = 2.0f;
        private PrimitiveType previewMeshType = PrimitiveType.Plane;
        private Shader materialShader;
        private bool isProcessing = false;
        private bool createPreviews = true;
        private bool recursiveSearch = false;
        private bool dryRun = true;
        private bool overwriteExisting = false;
        private bool cancelRequested = false;
        private object coroutineRef = null; // keep as object for compatibility
        private Vector2 scrollPos = Vector2.zero;

        [MenuItem("Tools/Kalponic Studio/Auto Base Material Maker")]
        public static void ShowWindow()
        {
            GetWindow<AutoBaseMaterialMaker>("Auto Base Material Maker");
        }

        private void OnEnable()
        {
            materialShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        }

        private void OnGUI()
        {
            if (!EnsureCoroutinePackageAvailable())
                return;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            textureFolder = DrawFolderSelector(
                "Texture Folder",
                "Folder containing your source textures (PNG, JPG, JPEG, TGA). Drag & drop a folder from the Project window or browse.",
                textureFolder,
                "Select Texture Folder",
                "Pick Folder"
            );

            EditorGUILayout.Space();

            materialsFolder = DrawFolderSelector(
                "Materials Save Folder",
                "Folder to save generated materials. Leave empty to use a 'Materials' subfolder next to the textures.",
                materialsFolder,
                "Select Materials Save Folder",
                "Pick Save Folder",
                "(default: Materials subfolder next to the texture folder)"
            );
            if (GUILayout.Button(new GUIContent("Reset", "Reset to default materials save folder."), GUILayout.MaxWidth(60)))
            {
                materialsFolder = "";
            }

            gridSize = EditorGUILayout.IntField(
                new GUIContent("Grid Size", "Number of preview meshes per row in the scene."),
                gridSize
            );
            spacing = EditorGUILayout.FloatField(
                new GUIContent("Spacing", "Distance between each preview mesh in the grid."),
                spacing
            );
            previewMeshType = (PrimitiveType)EditorGUILayout.EnumPopup(
                new GUIContent("Preview Mesh", "Primitive mesh type (plane, cube, sphere, etc.) used for material previews."),
                previewMeshType
            );
            materialShader = EditorGUILayout.ObjectField(
                new GUIContent("Material Shader", "Shader used for the generated materials. Pick the correct shader for your pipeline."),
                materialShader, typeof(Shader), false
            ) as Shader;

            EditorGUILayout.Space();
            createPreviews = EditorGUILayout.Toggle(
                new GUIContent("Create Preview Meshes", "If checked, will spawn preview meshes in the scene for each material."),
                createPreviews
            );
            recursiveSearch = EditorGUILayout.Toggle(
                new GUIContent("Recursive Search (include subfolders)", "If checked, searches for textures in all subfolders (not just the main folder)."),
                recursiveSearch
            );

            EditorGUILayout.Space();
            dryRun = EditorGUILayout.Toggle(new GUIContent("Dry Run (no asset writes)", "If checked, simulates actions without writing assets."), dryRun);
            overwriteExisting = EditorGUILayout.Toggle(new GUIContent("Overwrite Existing Materials", "If checked, existing materials with the same name will be overwritten."), overwriteExisting);

            EditorGUILayout.Space();

            GUI.enabled = !isProcessing;
            if (GUILayout.Button(new GUIContent("Generate Materials & Preview", "Create materials from all textures and optionally preview them in the scene.")))
            {
                StartCoroutine();
            }
            GUI.enabled = isProcessing;
            if (isProcessing)
            {
                if (GUILayout.Button(new GUIContent("Cancel", "Stop the current material generation process.")))
                {
                    cancelRequested = true;
                }
            }
            GUI.enabled = true;

            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Delete All Previews In Scene", "Deletes all preview meshes created by this tool from the scene. Does NOT delete your materials or textures.")))
            {
                DeleteAllPreviewObjects();
            }

            EditorGUILayout.EndScrollView();
        }

        // This dynamic call avoids compile error if package is missing
        private void StartCoroutine()
        {
            if (isProcessing) return;
            cancelRequested = false;

            if (!ValidateFolders())
                return;

            // Use reflection to start coroutine so script compiles even if package missing
            Type coroutineType = Type.GetType("Unity.EditorCoroutines.Editor.EditorCoroutineUtility, Unity.EditorCoroutines.Editor");
            if (coroutineType != null)
            {
                var startMethod = coroutineType.GetMethod("StartCoroutineOwnerless", new Type[] { typeof(IEnumerator) });
                coroutineRef = startMethod.Invoke(null, new object[] { GenerateMaterialsCoroutine() });
            }
        }

        private IEnumerator GenerateMaterialsCoroutine()
        {
            isProcessing = true;

            string finalMaterialsFolder = GetMaterialsFolder();
            string absoluteTextureRoot = GetAbsolutePath(textureFolder);
            if (!Directory.Exists(absoluteTextureRoot))
            {
                EditorUtility.DisplayDialog("Texture Folder Missing", $"Could not find the folder:\n{textureFolder}", "OK");
                isProcessing = false;
                coroutineRef = null;
                yield break;
            }

            // Gather texture files (recursive if checked)
            var searchOption = recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] files = Directory.GetFiles(absoluteTextureRoot, "*.*", searchOption);
            string[] allowedExt = new[] { ".png", ".jpg", ".jpeg", ".tga" };
            var textures = Array.FindAll(files, path => Array.Exists(allowedExt, a => a == Path.GetExtension(path).ToLower()));

            int row = 0, col = 0;
            int total = textures.Length;
            int created = 0, skipped = 0;
            // Batch asset editing to reduce database overhead
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < total; i++)
                {

                if (cancelRequested)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Cancelled", $"Operation cancelled.\nMaterials created: {created}\nSkipped: {skipped}", "OK");
                    isProcessing = false;
                    coroutineRef = null;
                    yield break;
                }

                EditorUtility.DisplayProgressBar("Generating Materials", $"Processing texture {i + 1}/{total}", (float)i / total);

                string texPath = ToAssetPath(textures[i]);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

                if (tex == null)
                {
                    skipped++;
                    yield return null;
                    continue;
                }

                string matName = tex.name + "_Mat.mat";
                string matPath = Path.Combine(finalMaterialsFolder, matName).Replace("\\", "/");

                var existingMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (existingMat != null)
                {
                    if (!overwriteExisting)
                    {
                        skipped++;
                        yield return null;
                        continue;
                    }
                    else
                    {
                        if (!dryRun)
                        {
                            AssetDatabase.DeleteAsset(matPath);
                        }
                    }
                }

                if (dryRun)
                {
                    Debug.Log($"[AutoTexify DryRun] Would create material: {matPath}");
                    created++;
                    yield return null;
                    continue;
                }

                var mat = CreateMaterial(tex, materialShader);
                bool createFailed = false;
                try
                {
                    AssetDatabase.CreateAsset(mat, matPath);
                    // assign albedo safely after creation
                    AssignAlbedoSafely(mat, tex);
                    EditorUtility.SetDirty(mat);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to create material at '{matPath}': {ex.Message}");
                    skipped++;
                    createFailed = true;
                }

                if (createFailed)
                {
                    yield return null;
                    continue;
                }

                if (createPreviews)
                {
                    CreatePreviewObject(tex.name, mat, row, col, spacing, previewMeshType);

                    col++;
                    if (col >= gridSize)
                    {
                        col = 0;
                        row++;
                    }
                }

                created++;
                yield return null;
            }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Done!", $"Materials created: {created}\nSkipped: {skipped}\nSaved to: {finalMaterialsFolder}", "OK");
            Debug.Log($"All base materials created in '{finalMaterialsFolder}' and preview meshes added! Created: {created}, Skipped: {skipped}");

            isProcessing = false;
            coroutineRef = null;
        }

        private bool EnsureCoroutinePackageAvailable()
        {
            Type coroutineType = Type.GetType("Unity.EditorCoroutines.Editor.EditorCoroutineUtility, Unity.EditorCoroutines.Editor");
            if (coroutineType != null)
                return true;

            EditorGUILayout.HelpBox(
                "Requires Unity's Editor Coroutines package.\n" +
                "Open Window > Package Manager > Add package by name... and enter:\n" +
                "com.unity.editorcoroutines",
                MessageType.Error
            );
            return false;
        }

        private string DrawFolderSelector(string label, string tooltip, string currentPath, string browseTitle, string browseButtonLabel, string defaultHint = "")
        {
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel);
            bool invalidDrop = false;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var folderObj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(currentPath);
                    var droppedObj = EditorGUILayout.ObjectField(new GUIContent("Folder", "Drag & drop a folder from the Project window."), folderObj, typeof(DefaultAsset), false) as DefaultAsset;
                    if (droppedObj != null)
                    {
                        string droppedPath = AssetDatabase.GetAssetPath(droppedObj);
                        if (AssetDatabase.IsValidFolder(droppedPath))
                            currentPath = droppedPath;
                        else
                            invalidDrop = true;
                    }

                    string newPath = EditorGUILayout.TextField(currentPath);
                    if (!string.Equals(newPath, currentPath, StringComparison.Ordinal))
                        currentPath = newPath.Replace("\\", "/");

                    if (GUILayout.Button(new GUIContent(browseButtonLabel, browseTitle), GUILayout.MaxWidth(120)))
                    {
                        string selectedPath = EditorUtility.OpenFolderPanel(browseTitle, Application.dataPath, "");
                        if (!string.IsNullOrEmpty(selectedPath) && selectedPath.StartsWith(Application.dataPath))
                            currentPath = "Assets" + selectedPath.Substring(Application.dataPath.Length).Replace("\\", "/");
                    }
                }

                if (invalidDrop)
                    EditorGUILayout.HelpBox("Please drop a folder asset from inside the project.", MessageType.Warning);

                if (!string.IsNullOrEmpty(defaultHint))
                    EditorGUILayout.LabelField(defaultHint, EditorStyles.miniLabel);
            }

            return currentPath;
        }

        private bool ValidateFolders()
        {
            if (!AssetDatabase.IsValidFolder(textureFolder))
            {
                EditorUtility.DisplayDialog("Texture Folder Missing", "Please assign a valid texture folder inside the project. You can drag & drop it from the Project window.", "OK");
                return false;
            }

            if (!string.IsNullOrEmpty(materialsFolder) && !AssetDatabase.IsValidFolder(materialsFolder))
            {
                try
                {
                    EnsureFolderExists(materialsFolder);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to create materials folder at '{materialsFolder}': {ex.Message}");
                    EditorUtility.DisplayDialog("Materials Folder Invalid", $"Could not create or find the folder:\n{materialsFolder}\n\nPlease choose a valid folder inside Assets.", "OK");
                    return false;
                }
            }

            return true;
        }

        private string GetMaterialsFolder()
        {
            if (!string.IsNullOrEmpty(materialsFolder))
                return EnsureFolderExists(materialsFolder);

            string defaultFolder = Path.Combine(textureFolder, "Materials").Replace("\\", "/");
            return EnsureFolderExists(defaultFolder);
        }

        private string EnsureFolderExists(string folderPath)
        {
            string normalized = folderPath.Replace("\\", "/");
            if (!normalized.StartsWith("Assets"))
                throw new ArgumentException("Folder must be inside the project's Assets folder.", nameof(folderPath));

            if (AssetDatabase.IsValidFolder(normalized))
                return normalized;

            string[] segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string current = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string next = $"{current}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, segments[i]);
                current = next;
            }
            return current;
        }

        private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "..")).Replace("\\", "/");

        private string GetAbsolutePath(string assetPath)
        {
            string normalized = assetPath.Replace("\\", "/");
            if (!normalized.StartsWith("Assets"))
                throw new ArgumentException("Path must start with 'Assets'.", nameof(assetPath));

            string relative = normalized.Length > "Assets".Length ? normalized.Substring("Assets".Length).TrimStart('/') : string.Empty;
            return Path.Combine(Application.dataPath, relative);
        }

        private string ToAssetPath(string absolutePath)
        {
            string normalized = Path.GetFullPath(absolutePath).Replace("\\", "/");
            if (!normalized.StartsWith(ProjectRoot))
                throw new ArgumentException($"Path '{absolutePath}' is outside the project.", nameof(absolutePath));

            return normalized.Substring(ProjectRoot.Length + 1);
        }

        private Material CreateMaterial(Texture2D texture, Shader shader)
        {
            var mat = new Material(shader ?? Shader.Find("Standard"));
            mat.name = texture.name + "_Mat";
            return mat;
        }

        private void AssignAlbedoSafely(Material mat, Texture2D tex)
        {
            if (mat == null || tex == null) return;
            string[] albedoProps = new[] { "_BaseMap", "_MainTex", "_BaseColorMap", "_Albedo" };
            foreach (var p in albedoProps)
            {
                if (p == "_MainTex")
                {
                    try { mat.mainTexture = tex; return; } catch { }
                }
                else if (mat.HasProperty(p))
                {
                    try { mat.SetTexture(p, tex); return; } catch { }
                }
            }
            Debug.LogWarning($"AutoTexify: No known albedo property found on shader '{mat.shader.name}' for material '{mat.name}'. Texture not assigned.");
        }

        private void CreatePreviewObject(string baseName, Material material, int row, int col, float objSpacing, PrimitiveType meshType)
        {
            var go = GameObject.CreatePrimitive(meshType);
            go.transform.position = new Vector3(col * objSpacing, 0, -row * objSpacing);
            var rend = go.GetComponent<Renderer>();
            if (rend != null)
                rend.sharedMaterial = material;
            go.name = baseName + "_Preview";
            go.hideFlags = HideFlags.DontSaveInBuild;
            Undo.RegisterCreatedObjectUndo(go, "AutoTexify Create Preview");
            var colc = go.GetComponent<Collider>();
            if (colc != null)
                DestroyImmediate(colc);
        }

        private void DeleteAllPreviewObjects()
        {
            int deleted = 0;
            foreach (var obj in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj.name.EndsWith("_Preview"))
                {
                    DestroyImmediate(obj);
                    deleted++;
                }
            }
            Debug.Log($"All preview objects deleted. Count: {deleted}");
            EditorUtility.DisplayDialog("Cleanup Complete", $"Deleted {deleted} preview objects.", "OK");
        }
    }
#endif
}
