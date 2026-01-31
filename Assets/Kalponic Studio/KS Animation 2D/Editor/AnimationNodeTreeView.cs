using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Collections.Generic;

namespace KalponicStudio.Editor
{
    /// <summary>
    /// Tree view for animation nodes hierarchy
    /// Provides organized view of node relationships and properties
    /// </summary>
    public class AnimationNodeTreeView : TreeView<int>
    {
        #region Constants
        private const int ROOT_ID = 0;
        private const int NODE_ID_OFFSET = 1000;

        private static readonly string[] NODE_NAMES = new string[]
        {
            "Head", "Hand_R", "Hand_L", "Foot_R", "Foot_L",
            "Weapon", "Shield", "Effect_1", "Effect_2", "Custom"
        };

        private static readonly string[] NODE_CATEGORIES = new string[]
        {
            "Body", "Body", "Body", "Body", "Body",
            "Equipment", "Equipment", "Effects", "Effects", "Custom"
        };
        #endregion

        #region Fields
        private SpriteAnimationNodes targetNodes;
        private Dictionary<int, AnimationNodeTreeElement> nodeElements;
        #endregion

        #region Constructor
        public AnimationNodeTreeView(TreeViewState<int> state, SpriteAnimationNodes nodes)
            : base(state)
        {
            targetNodes = nodes;
            nodeElements = new Dictionary<int, AnimationNodeTreeElement>();
            Reload();
        }
        #endregion

        #region TreeView Overrides
        protected override TreeViewItem<int> BuildRoot()
        {
            var root = new TreeViewItem<int> { id = ROOT_ID, depth = -1, displayName = "Animation Nodes" };

            var allItems = new List<TreeViewItem<int>>();
            nodeElements.Clear();

            // Create category folders
            var categories = new Dictionary<string, TreeViewItem<int>>();

            for (int i = 0; i < 10; i++)
            {
                string category = NODE_CATEGORIES[i];
                if (!categories.ContainsKey(category))
                {
                    var categoryItem = new TreeViewItem<int>
                    {
                        id = GetCategoryId(category),
                        displayName = category,
                        depth = 0
                    };
                    categories[category] = categoryItem;
                    allItems.Add(categoryItem);
                }

                // Create node item
                var nodeItem = new AnimationNodeTreeItem(i, NODE_NAMES[i], 1);
                nodeElements[i] = new AnimationNodeTreeElement
                {
                    nodeIndex = i,
                    position = targetNodes.GetNodePosition(i),
                    angle = targetNodes.GetNodeAngle(i),
                    treeItem = nodeItem
                };

                categories[category].AddChild(nodeItem);
                allItems.Add(nodeItem);
            }

            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as AnimationNodeTreeItem;
            if (item == null)
            {
                base.RowGUI(args);
                return;
            }

            // Draw node info
            Rect labelRect = args.rowRect;
            labelRect.x += GetContentIndent(item);
            labelRect.width -= GetContentIndent(item);

            // Node name
            GUI.Label(labelRect, item.displayName);

            // Position info
            if (nodeElements.ContainsKey(item.nodeIndex))
            {
                var element = nodeElements[item.nodeIndex];
                string posText = $"({element.position.x:F2}, {element.position.y:F2})";
                float angleText = element.angle;

                Rect posRect = labelRect;
                posRect.x = labelRect.xMax - 120;
                posRect.width = 60;
                GUI.Label(posRect, posText, EditorStyles.miniLabel);

                Rect angleRect = posRect;
                angleRect.x = posRect.xMax + 5;
                angleRect.width = 40;
                GUI.Label(angleRect, $"{angleText:F1}°", EditorStyles.miniLabel);
            }
        }

        protected override bool CanRename(TreeViewItem<int> item)
        {
            return false; // Nodes have fixed names
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            // Since CanRename returns false, renaming is not allowed
            // This method is required by Unity when CanRename is overridden
        }

        protected override void ContextClickedItem(int id)
        {
            if (id >= NODE_ID_OFFSET)
            {
                int nodeIndex = id - NODE_ID_OFFSET;
                ShowNodeContextMenu(nodeIndex);
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            if (id >= NODE_ID_OFFSET)
            {
                int nodeIndex = id - NODE_ID_OFFSET;
                // Focus on node in scene view
                if (SceneView.lastActiveSceneView != null)
                {
                    Vector3 worldPos = targetNodes.GetNodeWorldPosition(nodeIndex);
                    SceneView.lastActiveSceneView.LookAt(worldPos);
                }
            }
        }
        #endregion

        #region Context Menu
        private void ShowNodeContextMenu(int nodeIndex)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Reset Position"), false, () => ResetNodePosition(nodeIndex));
            menu.AddItem(new GUIContent("Center Node"), false, () => CenterNode(nodeIndex));
            menu.AddItem(new GUIContent("Copy Position"), false, () => CopyNodePosition(nodeIndex));
            menu.AddItem(new GUIContent("Paste Position"), false, () => PasteNodePosition(nodeIndex));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Mirror to Opposite Side"), false, () => MirrorNode(nodeIndex));

            menu.ShowAsContext();
        }

        private void ResetNodePosition(int nodeIndex)
        {
            Undo.RecordObject(targetNodes, $"Reset Node {nodeIndex}");
            targetNodes.ResetNode(nodeIndex);
            EditorUtility.SetDirty(targetNodes);
            Reload();
        }

        private void CenterNode(int nodeIndex)
        {
            Undo.RecordObject(targetNodes, $"Center Node {nodeIndex}");
            targetNodes.SetNodePosition(nodeIndex, Vector2.zero);
            EditorUtility.SetDirty(targetNodes);
            Reload();
        }

        private void CopyNodePosition(int nodeIndex)
        {
            Vector2 pos = targetNodes.GetNodePosition(nodeIndex);
            float angle = targetNodes.GetNodeAngle(nodeIndex);
            EditorGUIUtility.systemCopyBuffer = $"{pos.x},{pos.y},{angle}";
        }

        private void PasteNodePosition(int nodeIndex)
        {
            string buffer = EditorGUIUtility.systemCopyBuffer;
            string[] parts = buffer.Split(',');
            if (parts.Length >= 3)
            {
                if (float.TryParse(parts[0], out float x) &&
                    float.TryParse(parts[1], out float y) &&
                    float.TryParse(parts[2], out float angle))
                {
                    Undo.RecordObject(targetNodes, $"Paste Node {nodeIndex} Position");
                    targetNodes.SetNodePosition(nodeIndex, new Vector2(x, y));
                    targetNodes.SetNodeAngle(nodeIndex, angle);
                    EditorUtility.SetDirty(targetNodes);
                    Reload();
                }
            }
        }

        private void MirrorNode(int nodeIndex)
        {
            int oppositeIndex = GetOppositeNodeIndex(nodeIndex);
            if (oppositeIndex != -1)
            {
                Undo.RecordObject(targetNodes, $"Mirror Node {nodeIndex}");

                Vector2 oppositePos = targetNodes.GetNodePosition(oppositeIndex);
                float oppositeAngle = targetNodes.GetNodeAngle(oppositeIndex);

                // Mirror position (flip X)
                targetNodes.SetNodePosition(nodeIndex, new Vector2(-oppositePos.x, oppositePos.y));
                // Mirror angle (negate for opposite side)
                targetNodes.SetNodeAngle(nodeIndex, -oppositeAngle);

                EditorUtility.SetDirty(targetNodes);
                Reload();
            }
        }
        #endregion

        #region Utility Methods
        private int GetCategoryId(string category)
        {
            return category.GetHashCode();
        }

        private int GetOppositeNodeIndex(int nodeIndex)
        {
            switch (nodeIndex)
            {
                case 1: return 2; // Hand_R -> Hand_L
                case 2: return 1; // Hand_L -> Hand_R
                case 3: return 4; // Foot_R -> Foot_L
                case 4: return 3; // Foot_L -> Foot_R
                default: return -1; // No opposite
            }
        }

        public void Refresh()
        {
            Reload();
        }

        public void UpdateNodeData(int nodeIndex)
        {
            if (nodeElements.ContainsKey(nodeIndex))
            {
                nodeElements[nodeIndex].position = targetNodes.GetNodePosition(nodeIndex);
                nodeElements[nodeIndex].angle = targetNodes.GetNodeAngle(nodeIndex);
            }
        }
        #endregion

        #region Data Classes
        private class AnimationNodeTreeItem : TreeViewItem<int>
        {
            public int nodeIndex;

            public AnimationNodeTreeItem(int index, string name, int depth)
            {
                this.nodeIndex = index;
                this.displayName = name;
                this.depth = depth;
                this.id = NODE_ID_OFFSET + index;
            }
        }

        private class AnimationNodeTreeElement
        {
            public int nodeIndex;
            public Vector2 position;
            public float angle;
            public AnimationNodeTreeItem treeItem;
        }
        #endregion
    }
}
