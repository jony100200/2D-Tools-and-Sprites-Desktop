using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KalponicStudio
{
    public enum AssetType { Models, Prefabs, Both }

    public class KSPrefabMakerWindow : EditorWindow
    {
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
            inputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Input Folder", inputFolder, typeof(DefaultAsset), false);
            outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(DefaultAsset), false);
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
        }
    }
}