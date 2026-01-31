using UnityEditor.SceneManagement;
using UnityEngine;

namespace KalponicStudio
{
    public static class WorkspaceManager
    {
        public static GameObject GetOrCreateRoot()
        {
            GameObject root = GameObject.Find("KS_PrefabWorkspaceRoot");
            if (root == null)
            {
                root = new GameObject("KS_PrefabWorkspaceRoot");
            }
            return root;
        }

        public static void ClearRoot()
        {
            GameObject root = GameObject.Find("KS_PrefabWorkspaceRoot");
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
        }

        public static void CreateWorkspaceScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            GetOrCreateRoot();
        }
    }
}