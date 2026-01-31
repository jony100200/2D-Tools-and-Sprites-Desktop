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

    private void OnGUI()
    {
        GUILayout.Label("Anim Clip Gen 2D", EditorStyles.boldLabel);

        // Source folder field
        GUILayout.Label("Source Folder", EditorStyles.boldLabel);
        sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField(new GUIContent("Source Folder", "Drag a folder containing sprite sheets to generate animation clips from."), sourceFolder, typeof(DefaultAsset), false);

        // Output folder field
        GUILayout.Label("Output Folder", EditorStyles.boldLabel);
        outputFolderAsset = (DefaultAsset)EditorGUILayout.ObjectField(new GUIContent("Output Folder", "Drag a folder to save generated animation clips."), outputFolderAsset, typeof(DefaultAsset), false);

        // Frame rate
        GUILayout.Label("Frame Rate", EditorStyles.boldLabel);
        frameRate = EditorGUILayout.FloatField(new GUIContent("Frame Rate", "Frames per second for the animations."), frameRate);

        // Loop
        loop = EditorGUILayout.Toggle(new GUIContent("Loop Animation", "Whether the generated animations should loop."), loop);

        if (GUILayout.Button(new GUIContent("Generate Animation Clips", "Generate empty animation clips for each sprite sheet.")))
        {
            GenerateClips();
        }
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
    }
}