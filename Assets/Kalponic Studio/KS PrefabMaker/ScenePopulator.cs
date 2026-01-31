using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KalponicStudio
{
    public static class ScenePopulator
    {
        public static List<(string path, GameObject instance)> Populate(GameObject root, string[] assetPaths, bool addColliders)
        {
            List<(string, GameObject)> populated = new List<(string, GameObject)>();
            int total = assetPaths.Length;
            for (int i = 0; i < total; i++)
            {
                string path = assetPaths[i];
                if (total > 10) EditorUtility.DisplayProgressBar("Populating Scene", $"Instantiating {path}", (float)i / total);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset != null)
                {
                    GameObject instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
                    if (instance != null)
                    {
                        instance.transform.parent = root.transform;
                        instance.transform.position = Vector3.zero;
                        if (addColliders && instance.GetComponent<Collider>() == null)
                        {
                            instance.AddComponent<BoxCollider>();
                        }
                        populated.Add((path, instance));
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            return populated;
        }
    }
}