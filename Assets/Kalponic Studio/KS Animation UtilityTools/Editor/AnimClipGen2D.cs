using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AnimClipGen2D : EditorWindow
{
    [MenuItem("Tools/Kalponic Studio/Anim Clip Gen 2D")]
    public static void ShowWindow()
    {
        GetWindow<AnimClipGen2D>("Anim Clip Gen 2D");
    }

    private DefaultAsset sourceFolder;
    private DefaultAsset outputFolderAsset;
    private string outputFolder = "Assets/GeneratedAnimations";
    private float frameRate = 12f;
    private bool loop = true;
    private Vector2 scrollPos;
    private const string PREF_GEN2D_SOURCE = "KSAG_Gen2D_SourceFolder";
    private const string PREF_GEN2D_OUTPUT = "KSAG_Gen2D_OutputFolder";

    private void OnEnable()
    {
        string src = EditorPrefs.GetString(PREF_GEN2D_SOURCE, string.Empty);
        if (!string.IsNullOrEmpty(src))
            sourceFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(src);

        string outp = EditorPrefs.GetString(PREF_GEN2D_OUTPUT, string.Empty);
        if (!string.IsNullOrEmpty(outp))
        {
            outputFolder = outp;
            outputFolderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(outp);
        }
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Anim Clip Gen 2D", EditorStyles.boldLabel);

        // Source folder field
        GUILayout.Label("Source Folder", EditorStyles.boldLabel);
        var newSource = (DefaultAsset)EditorGUILayout.ObjectField(new GUIContent("Source Folder", "Drag a folder containing sprite sheets to generate animation clips from."), sourceFolder, typeof(DefaultAsset), false);
        if (newSource != sourceFolder)
        {
            sourceFolder = newSource;
            EditorPrefs.SetString(PREF_GEN2D_SOURCE, sourceFolder != null ? AssetDatabase.GetAssetPath(sourceFolder) : string.Empty);
        }

        // Output folder field
        GUILayout.Label("Output Folder", EditorStyles.boldLabel);
        var newOutput = (DefaultAsset)EditorGUILayout.ObjectField(new GUIContent("Output Folder", "Drag a folder to save generated animation clips."), outputFolderAsset, typeof(DefaultAsset), false);
        if (newOutput != outputFolderAsset)
        {
            outputFolderAsset = newOutput;
            outputFolder = outputFolderAsset != null ? AssetDatabase.GetAssetPath(outputFolderAsset) : outputFolder;
            EditorPrefs.SetString(PREF_GEN2D_OUTPUT, outputFolder);
        }

        // Frame rate
        GUILayout.Label("Frame Rate", EditorStyles.boldLabel);
        frameRate = EditorGUILayout.FloatField(new GUIContent("Frame Rate", "Frames per second for the animations."), frameRate);

        // Loop
        loop = EditorGUILayout.Toggle(new GUIContent("Loop Animation", "Whether the generated animations should loop."), loop);

        if (GUILayout.Button(new GUIContent("Generate Animation Clips", "Generate empty animation clips for each sprite sheet.")))
        {
            GenerateClips();
        }

        EditorGUILayout.EndScrollView();
    }

    private void GenerateClips()
    {
        if (sourceFolder == null)
        {
            Debug.LogError("No source folder selected.");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(sourceFolder);
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Selected object is not a valid folder.");
            return;
        }

        // Ensure output folder exists
        if (outputFolderAsset != null)
        {
            outputFolder = AssetDatabase.GetAssetPath(outputFolderAsset);
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                Debug.LogError("Selected output is not a valid folder.");
                return;
            }
        }
        else
        {
            // Fallback to default
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                string parentFolder = System.IO.Path.GetDirectoryName(outputFolder).Replace("\\", "/");
                string folderName = System.IO.Path.GetFileName(outputFolder);
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }

        // Find all sprite assets in the folder
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
        Dictionary<Texture2D, List<Sprite>> spriteGroups = new Dictionary<Texture2D, List<Sprite>>();

        foreach (string guid in spriteGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null && sprite.texture != null)
            {
                if (!spriteGroups.ContainsKey(sprite.texture))
                {
                    spriteGroups[sprite.texture] = new List<Sprite>();
                }
                spriteGroups[sprite.texture].Add(sprite);
            }
        }

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var group in spriteGroups)
            {
                Texture2D texture = group.Key;
                List<Sprite> sprites = group.Value.OrderBy(s => s.rect.y).ThenBy(s => s.rect.x).ToList();

                if (sprites.Count > 0)
                {
                    string animName = texture.name;

                    AnimationClip clip = new AnimationClip();
                    clip.frameRate = frameRate;

                    if (loop)
                    {
                        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                        settings.loopTime = true;
                        AnimationUtility.SetAnimationClipSettings(clip, settings);
                    }

                    // Save the clip (without keyframes, for manual editing)
                    string clipPath = Path.Combine(outputFolder, animName + ".anim").Replace("\\", "/");
                    clipPath = AssetDatabase.GenerateUniqueAssetPath(clipPath);
                    AssetDatabase.CreateAsset(clip, clipPath);

                    Debug.Log("Generated empty animation clip: " + clipPath + " for " + sprites.Count + " sprites (add frames manually).");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Generated all animation clips.");
        // persist folders
        EditorPrefs.SetString(PREF_GEN2D_SOURCE, sourceFolder != null ? AssetDatabase.GetAssetPath(sourceFolder) : string.Empty);
        EditorPrefs.SetString(PREF_GEN2D_OUTPUT, outputFolder);
    }
}