using UnityEditor;
using UnityEngine;

namespace KalponicStudio.Editor
{
    public static class HitboxMenuItems
    {
        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Create Hitbox Collision Matrix", false, 300)]
        private static void CreateCollisionMatrix()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Hitbox Collision Matrix", "HitboxCollisionMatrix", "asset", "Choose where to save the collision matrix asset.");
            if (string.IsNullOrEmpty(path)) return;

            var asset = ScriptableObject.CreateInstance<HitboxCollisionMatrix>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(asset);
        }
    }
}
