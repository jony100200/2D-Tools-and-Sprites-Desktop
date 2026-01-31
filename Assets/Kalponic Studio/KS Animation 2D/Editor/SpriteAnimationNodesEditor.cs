using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace KalponicStudio.Editor
{
    /// <summary>
    /// Custom editor for SpriteAnimationNodes component
    /// Provides visual node editing tools in the scene view
    /// </summary>
    [CustomEditor(typeof(SpriteAnimationNodes))]
    public class SpriteAnimationNodesEditor : UnityEditor.Editor
    {
        #region Constants
        private const float NODE_HANDLE_SIZE = 0.1f;
        private const float NODE_LABEL_OFFSET = 0.15f;
        private const float ROTATION_HANDLE_SIZE = 0.3f;
        private const float ATTACHMENT_LINE_THICKNESS = 2f;

        private static readonly Color[] NODE_COLORS = new Color[]
        {
            Color.red,      // Node 0
            Color.green,    // Node 1
            Color.blue,     // Node 2
            Color.yellow,   // Node 3
            Color.cyan,     // Node 4
            Color.magenta,  // Node 5
            Color.white,    // Node 6
            Color.gray,     // Node 7
            Color.black,    // Node 8
            Color.red       // Node 9 (duplicate for visibility)
        };

        private static readonly string[] NODE_NAMES = new string[]
        {
            "Head", "Hand_R", "Hand_L", "Foot_R", "Foot_L",
            "Weapon", "Shield", "Effect_1", "Effect_2", "Custom"
        };
        #endregion

        #region Fields
        private SpriteAnimationNodes targetNodes;
        private Transform targetTransform;
        private SpriteRenderer spriteRenderer;

        // Editor state
        private int selectedNodeIndex = -1;
        private bool showNodeLabels = true;
        private bool showAttachmentPreview = true;
        private bool editMode = false;

        // GUI
        private GUIStyle nodeLabelStyle;
        #endregion

        #region Unity Messages
        private void OnEnable()
        {
            targetNodes = (SpriteAnimationNodes)target;
            targetTransform = targetNodes.transform;
            spriteRenderer = targetNodes.GetComponent<SpriteRenderer>();

            InitializeStyles();

            // Subscribe to scene GUI
            SceneView.duringSceneGui += OnSceneGUIDelegate;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUIDelegate;
        }

        /// <summary>
        /// Delegate method for SceneView.duringSceneGui event
        /// </summary>
        private void OnSceneGUIDelegate(SceneView sceneView)
        {
            OnSceneGUI();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visual Node Editor", EditorStyles.boldLabel);

            // Edit mode toggle
            EditorGUI.BeginChangeCheck();
            editMode = EditorGUILayout.Toggle("Edit Mode", editMode);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            // Display options
            showNodeLabels = EditorGUILayout.Toggle("Show Node Labels", showNodeLabels);
            showAttachmentPreview = EditorGUILayout.Toggle("Show Attachment Preview", showAttachmentPreview);

            EditorGUILayout.Space();

            // Node list
            EditorGUILayout.LabelField("Animation Nodes", EditorStyles.boldLabel);
            for (int i = 0; i < 10; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Node color indicator
                Rect colorRect = EditorGUILayout.GetControlRect(GUILayout.Width(20), GUILayout.Height(20));
                EditorGUI.DrawRect(colorRect, NODE_COLORS[i]);

                // Node name and position
                Vector2 nodePos = targetNodes.GetNodePosition(i);
                EditorGUI.BeginChangeCheck();
                nodePos = EditorGUILayout.Vector2Field($"{NODE_NAMES[i]} (Node {i})", nodePos);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(targetNodes, $"Move Node {i}");
                    targetNodes.SetNodePosition(i, nodePos);
                    EditorUtility.SetDirty(targetNodes);
                }

                // Select button
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    selectedNodeIndex = i;
                    SceneView.RepaintAll();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // Reset buttons
            if (GUILayout.Button("Reset All Nodes"))
            {
                Undo.RecordObject(targetNodes, "Reset All Nodes");
                targetNodes.ResetAllNodes();
                EditorUtility.SetDirty(targetNodes);
            }

            if (GUILayout.Button("Mirror Nodes (Left <-> Right)"))
            {
                Undo.RecordObject(targetNodes, "Mirror Nodes");
                MirrorNodes();
                EditorUtility.SetDirty(targetNodes);
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Scene View GUI
        private void OnSceneGUI()
        {
            if (!editMode || targetNodes == null) return;

            // Draw node handles
            for (int i = 0; i < 10; i++)
            {
                DrawNodeHandle(i);
            }

            // Draw attachment preview
            if (showAttachmentPreview)
            {
                DrawAttachmentPreview();
            }

            // Handle input
            HandleSceneInput();
        }

        private void DrawNodeHandle(int nodeIndex)
        {
            Vector3 worldPos = targetNodes.GetNodeWorldPosition(nodeIndex);

            // Draw handle
            float handleSize = HandleUtility.GetHandleSize(worldPos) * NODE_HANDLE_SIZE;
            Handles.color = selectedNodeIndex == nodeIndex ? Color.white : NODE_COLORS[nodeIndex];

            EditorGUI.BeginChangeCheck();
            var fmh_179_68_638990255723319007 = Quaternion.identity; Vector3 newWorldPos = Handles.FreeMoveHandle(worldPos, handleSize, Vector3.zero, Handles.CircleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetNodes, $"Move Node {nodeIndex}");
                Vector3 localPos = targetTransform.InverseTransformPoint(newWorldPos);
                targetNodes.SetNodePosition(nodeIndex, localPos);
                EditorUtility.SetDirty(targetNodes);
            }

            // Draw rotation handle if selected
            if (selectedNodeIndex == nodeIndex)
            {
                DrawRotationHandle(nodeIndex, worldPos);
            }

            // Draw label
            if (showNodeLabels)
            {
                DrawNodeLabel(nodeIndex, worldPos);
            }
        }

        private void DrawRotationHandle(int nodeIndex, Vector3 worldPos)
        {
            float rotation = targetNodes.GetNodeAngle(nodeIndex);
            Quaternion currentRotation = Quaternion.Euler(0, 0, rotation);

            EditorGUI.BeginChangeCheck();
            Quaternion newRotation = Handles.RotationHandle(currentRotation, worldPos);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetNodes, $"Rotate Node {nodeIndex}");
                float newAngle = newRotation.eulerAngles.z;
                targetNodes.SetNodeAngle(nodeIndex, newAngle);
                EditorUtility.SetDirty(targetNodes);
            }
        }

        private void DrawNodeLabel(int nodeIndex, Vector3 worldPos)
        {
            if (nodeLabelStyle == null) return;

            string label = $"{NODE_NAMES[nodeIndex]}\n({nodeIndex})";
            Vector3 labelPos = worldPos + Vector3.up * NODE_LABEL_OFFSET;

            Handles.Label(labelPos, label, nodeLabelStyle);
        }

        private void DrawAttachmentPreview()
        {
            var attachments = targetNodes.GetComponent<SpriteAnimationAttachments>();
            if (attachments == null || attachments.m_attachments == null) return;

            Handles.color = Color.green;

            for (int i = 0; i < attachments.m_attachments.Length && i < 10; i++)
            {
                Transform attachment = attachments.m_attachments[i];
                if (attachment != null)
                {
                    Vector3 nodePos = targetNodes.GetNodeWorldPosition(i);
                    Vector3 attachmentPos = attachment.position;

                    // Draw line from node to attachment
                    Handles.DrawLine(nodePos, attachmentPos, ATTACHMENT_LINE_THICKNESS);

                    // Draw attachment point
                    Handles.SphereHandleCap(0, attachmentPos, Quaternion.identity,
                        HandleUtility.GetHandleSize(attachmentPos) * 0.05f, EventType.Repaint);
                }
            }
        }

        private void HandleSceneInput()
        {
            Event e = Event.current;

            // Handle node selection
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Vector2 mousePos = e.mousePosition;
                mousePos.y = Screen.height - mousePos.y; // Flip Y coordinate

                selectedNodeIndex = -1;

                // Check if clicked on a node
                for (int i = 0; i < 10; i++)
                {
                    Vector3 nodeWorldPos = targetNodes.GetNodeWorldPosition(i);
                    Vector2 nodeScreenPos = HandleUtility.WorldToGUIPoint(nodeWorldPos);

                    float distance = Vector2.Distance(mousePos, nodeScreenPos);
                    float handleSize = HandleUtility.GetHandleSize(nodeWorldPos) * NODE_HANDLE_SIZE * 50; // Screen space size

                    if (distance < handleSize)
                    {
                        selectedNodeIndex = i;
                        e.Use();
                        break;
                    }
                }

                SceneView.RepaintAll();
            }

            // Handle keyboard shortcuts
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    selectedNodeIndex = -1;
                    SceneView.RepaintAll();
                    e.Use();
                }
                else if (e.keyCode >= KeyCode.Alpha0 && e.keyCode <= KeyCode.Alpha9)
                {
                    int nodeIndex = e.keyCode - KeyCode.Alpha0;
                    if (nodeIndex >= 0 && nodeIndex < 10)
                    {
                        selectedNodeIndex = nodeIndex;
                        SceneView.RepaintAll();
                        e.Use();
                    }
                }
            }
        }
        #endregion

        #region Utility Methods
        private void InitializeStyles()
        {
            nodeLabelStyle = new GUIStyle();
            nodeLabelStyle.normal.textColor = Color.white;
            nodeLabelStyle.fontSize = 10;
            nodeLabelStyle.fontStyle = FontStyle.Bold;
            nodeLabelStyle.alignment = TextAnchor.MiddleCenter;
            nodeLabelStyle.normal.background = CreateTexture(1, 1, new Color(0, 0, 0, 0.5f));
        }

        private Texture2D CreateTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height);
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void MirrorNodes()
        {
            // Mirror left/right nodes
            // Assuming nodes 1&2 are hands, 3&4 are feet
            Vector2 temp;

            // Mirror hands (1 <-> 2)
            temp = targetNodes.GetNodePosition(1);
            targetNodes.SetNodePosition(1, new Vector2(-targetNodes.GetNodePosition(2).x, targetNodes.GetNodePosition(2).y));
            targetNodes.SetNodePosition(2, new Vector2(-temp.x, temp.y));

            // Mirror feet (3 <-> 4)
            temp = targetNodes.GetNodePosition(3);
            targetNodes.SetNodePosition(3, new Vector2(-targetNodes.GetNodePosition(4).x, targetNodes.GetNodePosition(4).y));
            targetNodes.SetNodePosition(4, new Vector2(-temp.x, temp.y));
        }
        #endregion
    }
}
