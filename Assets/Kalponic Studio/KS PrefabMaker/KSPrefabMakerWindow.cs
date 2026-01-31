using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KalponicStudio
{
    public enum AssetType { Models, Prefabs, Both }

    public class KSPrefabMakerWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private const string PREF_PREFAB_INPUT = "KSPM_InputFolder";
        private const string PREF_PREFAB_OUTPUT = "KSPM_OutputFolder";

        private void OnEnable()
        {
            string inPath = EditorPrefs.GetString(PREF_PREFAB_INPUT, string.Empty);
            if (!string.IsNullOrEmpty(inPath))
                inputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(inPath);

            string outPath = EditorPrefs.GetString(PREF_PREFAB_OUTPUT, string.Empty);
            if (!string.IsNullOrEmpty(outPath))
                outputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(outPath);
        }
        [MenuItem("Tools/Kalponic Studio/KS Prefab Maker")]
        static void ShowWindow()
        {
            GetWindow<KSPrefabMakerWindow>("KS Prefab Maker");
        }

        DefaultAsset inputFolder;
        DefaultAsset outputFolder;
        bool includeSubfolders = true;
        bool mirrorStructure = true;
        bool overwriteExisting = false;
        AssetType selectedType = AssetType.Both;
        bool addColliders = true;

        List<(string path, GameObject instance)> populated = new List<(string path, GameObject instance)>();

        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            inputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Input Folder", inputFolder, typeof(DefaultAsset), false);
            outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(DefaultAsset), false);
            // persist fields when user changes them
            if (GUI.changed)
            {
                EditorPrefs.SetString(PREF_PREFAB_INPUT, inputFolder != null ? AssetDatabase.GetAssetPath(inputFolder) : string.Empty);
                EditorPrefs.SetString(PREF_PREFAB_OUTPUT, outputFolder != null ? AssetDatabase.GetAssetPath(outputFolder) : string.Empty);
            }
            selectedType = (AssetType)EditorGUILayout.EnumPopup("Asset Type", selectedType);
            includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);
            addColliders = EditorGUILayout.Toggle("Add Box Colliders", addColliders);
            mirrorStructure = EditorGUILayout.Toggle("Mirror Structure", mirrorStructure);
            overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);

            if (GUILayout.Button("Create Workspace Scene"))
            {
                WorkspaceManager.CreateWorkspaceScene();
            }

            if (GUILayout.Button("Populate From Input"))
            {
                if (inputFolder == null)
                {
                    Debug.LogError("Input folder not set");
                    return;
                }
                string inputPath = AssetDatabase.GetAssetPath(inputFolder);
                string[] assets = AssetScanner.FindInstantiableAssets(inputPath, includeSubfolders, selectedType);
                GameObject root = WorkspaceManager.GetOrCreateRoot();
                populated = ScenePopulator.Populate(root, assets, addColliders);
                Debug.Log($"Populated {populated.Count} assets");
            }

            if (GUILayout.Button("Create Prefabs"))
            {
                if (outputFolder == null)
                {
                    Debug.LogError("Output folder not set");
                    return;
                }
                string outputPath = AssetDatabase.GetAssetPath(outputFolder);
                string inputPath = AssetDatabase.GetAssetPath(inputFolder);
                PrefabCreator.CreatePrefabs(populated, outputPath, mirrorStructure, overwriteExisting, inputPath);
            }

            if (GUILayout.Button("Clear Workspace Objects"))
            {
                WorkspaceManager.ClearRoot();
                populated.Clear();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}