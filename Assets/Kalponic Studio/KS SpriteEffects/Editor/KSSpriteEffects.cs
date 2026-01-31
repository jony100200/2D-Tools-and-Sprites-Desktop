using UnityEditor;
using UnityEngine;
using KalponicStudio.SpriteEffects;

public class KSSpriteEffects : EditorWindow
{
    private Vector2 scrollPos;
    private const string InputFolderPrefKey = "KSSpriteEffects_InputFolder";
    private const string OutputFolderPrefKey = "KSSpriteEffects_OutputFolder";
    private const string MaterialPrefKey = "KSSpriteEffects_Material";

    private enum ProcessingScope
    {
        SingleTexture,
        Folder
    }

    [MenuItem("Tools/Kalponic Studio/KS Sprite Effects")]
    public static void ShowWindow()
    {
        GetWindow<KSSpriteEffects>("KS Sprite Effects");
    }

    private ProcessingScope scope = ProcessingScope.Folder;
    private Sprite singleSprite;
    private DefaultAsset inputFolder;
    private DefaultAsset outputFolder;
    private Material selectedMaterial;
    private string filePrefix = string.Empty;
    private string fileSuffix = string.Empty;

    private SpriteEffectBatchProcessor batchProcessor;

    private void OnEnable()
    {
        LoadPreferences();
        InitializeServices();
    }

    private void OnDisable()
    {
        if (batchProcessor != null)
        {
            batchProcessor.OnTextureProcessed -= HandleTextureProcessed;
            batchProcessor.OnTextureFailed -= HandleTextureFailed;
        }

        SavePreferences();
        EditorUtility.ClearProgressBar();
    }

    private void InitializeServices()
    {
        var locator = new FolderSpriteLocator();
        var extractor = new SpriteReadableExtractor();
        var processor = new MaterialEffectProcessor();
        var saver = new SpriteTextureSaver();

        batchProcessor = new SpriteEffectBatchProcessor(locator, extractor, processor, saver);
        batchProcessor.OnTextureProcessed += HandleTextureProcessed;
        batchProcessor.OnTextureFailed += HandleTextureFailed;
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("KS Sprite Effects", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Applies the selected material to sprites without stacking.", MessageType.Info);

        scope = (ProcessingScope)EditorGUILayout.EnumPopup("Processing Scope", scope);

        if (scope == ProcessingScope.SingleTexture)
        {
            singleSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", singleSprite, typeof(Sprite), false);
        }
        else
        {
            inputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Input Folder", inputFolder, typeof(DefaultAsset), false);
        }

        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(DefaultAsset), false);
        selectedMaterial = (Material)EditorGUILayout.ObjectField("Effect Material", selectedMaterial, typeof(Material), false);
        filePrefix = EditorGUILayout.TextField("Optional Prefix", filePrefix);
        fileSuffix = EditorGUILayout.TextField("Optional Suffix", fileSuffix);

        using (new EditorGUI.DisabledScope(!CanProcess()))
        {
            string buttonLabel = scope == ProcessingScope.SingleTexture ? "Process Sprite" : "Process Folder";
            if (GUILayout.Button(buttonLabel))
            {
                SavePreferences();
                ProcessTextures();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private bool CanProcess()
    {
        if (batchProcessor == null || outputFolder == null || selectedMaterial == null)
        {
            return false;
        }

        if (scope == ProcessingScope.SingleTexture)
        {
            return singleSprite != null;
        }

        return inputFolder != null;
    }

    private void ProcessTextures()
    {
        string outputPath = AssetDatabase.GetAssetPath(outputFolder);
        if (!AssetDatabase.IsValidFolder(outputPath))
        {
            Debug.LogError("Output folder is invalid.");
            return;
        }

        AssetDatabase.StartAssetEditing();
        try
        {
            if (scope == ProcessingScope.SingleTexture)
            {
                if (singleSprite == null)
                {
                    Debug.LogError("Selected sprite is invalid.");
                    return;
                }

                batchProcessor.ProcessSingleSprite(singleSprite, outputPath, selectedMaterial, filePrefix, fileSuffix);
            }
            else
            {
                string inputPath = AssetDatabase.GetAssetPath(inputFolder);
                if (!AssetDatabase.IsValidFolder(inputPath))
                {
                    Debug.LogError("Input folder is invalid.");
                    return;
                }

                batchProcessor.ProcessFolder(inputPath, outputPath, selectedMaterial, filePrefix, fileSuffix);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }

    private void HandleTextureProcessed(int index, int total, string path)
    {
        float progress = total > 0 ? (float)index / total : 1f;
        EditorUtility.DisplayProgressBar("KS Sprite Effects", $"Processing {index}/{total}: {path}", progress);
    }

    private void HandleTextureFailed(string path)
    {
        Debug.LogWarning($"Failed to process texture: {path}");
    }

    private void SavePreferences()
    {
        string inputPath = inputFolder != null ? AssetDatabase.GetAssetPath(inputFolder) : string.Empty;
        string outputPath = outputFolder != null ? AssetDatabase.GetAssetPath(outputFolder) : string.Empty;
        string materialPath = selectedMaterial != null ? AssetDatabase.GetAssetPath(selectedMaterial) : string.Empty;

        EditorPrefs.SetString(InputFolderPrefKey, inputPath);
        EditorPrefs.SetString(OutputFolderPrefKey, outputPath);
        EditorPrefs.SetString(MaterialPrefKey, materialPath);
    }

    private void LoadPreferences()
    {
        string inputPath = EditorPrefs.GetString(InputFolderPrefKey, string.Empty);
        string outputPath = EditorPrefs.GetString(OutputFolderPrefKey, string.Empty);
        string materialPath = EditorPrefs.GetString(MaterialPrefKey, string.Empty);

        if (!string.IsNullOrEmpty(inputPath))
        {
            inputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(inputPath);
        }

        if (!string.IsNullOrEmpty(outputPath))
        {
            outputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(outputPath);
        }

        if (!string.IsNullOrEmpty(materialPath))
        {
            selectedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        }
    }
}
