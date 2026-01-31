using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;
using System;

namespace KalponicStudio.Editor
{
    /// <summary>
    /// UIToolkit-based sprite animation editor with preview, scrubber, and frame list management.
    /// </summary>
    public sealed class SpriteAnimationEditorWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Kalponic Studio/Animation/KS Animation 2D/Sprite Animation Editor";

        internal AnimationStateSO animationState;
        internal SerializedObject serializedState;
        internal SerializedProperty spritesProp;
        internal SerializedProperty eventsProp;
        internal SerializedProperty frameDurationsProp;
        internal SerializedProperty frameEventsProp;
        internal SerializedProperty frameHitboxesProp;
        internal SerializedProperty onLoopOnceProp;
        internal SerializedProperty invokeOnFirstLoopProp;
        private ListView frameListView;
        private ListView eventListView;
        private TimelineElement timelineElement;
        private readonly List<int> frameIndices = new List<int>();
        private readonly List<int> eventIndices = new List<int>();

        private IMGUIContainer previewContainer;
        private Slider scrubSlider;
        private Label timeLabel;
        private Toggle loopToggle;
        private Slider speedSlider;
        private Slider zoomSlider;
        private Slider timelineZoomSlider;
        private Label clipInfoLabel;
        private FloatField fpsField;
        private int selectedHitboxIndex = -1;
        private bool isDraggingHitbox;
        private int draggingFrameIndex = -1;
        private Vector2 dragStartMouse;
        private Vector2 dragStartOffset;
        private Vector2 dragStartSize;
        private bool showHitboxes = true;
        private Label statusLabel;

        private static AnimationStateSO.FrameHitboxSet copyBuffer;

        internal float currentTime;
        internal bool isPlaying;
        private float lastEditorTime;
        private bool scrubbingTimeline;
        private float previewZoom = 1.1f;

        private VisualElement root;

        [MenuItem(MenuPath)]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteAnimationEditorWindow>();
            window.titleContent = new GUIContent("KS Animation 2D");
            window.minSize = new Vector2(900, 600);
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            lastEditorTime = (float)EditorApplication.timeSinceStartup;
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnEditorUpdate()
        {
            if (!isPlaying || animationState == null || animationState.sprites == null || animationState.sprites.Length == 0)
            {
                return;
            }

            float now = (float)EditorApplication.timeSinceStartup;
            currentTime += (now - lastEditorTime) * speedSlider.value;
            lastEditorTime = now;
            float duration = GetDuration();
            if (loopToggle.value && duration > 0f)
            {
                currentTime %= duration;
            }
            else
            {
                currentTime = Mathf.Min(currentTime, duration);
                if (currentTime >= duration) isPlaying = false;
            }

            UpdateScrubUI();
        }

        public void CreateGUI()
        {
            root = rootVisualElement;
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/KS Animation 2D/Editor/UI/SpriteAnimationEditor.uss");
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            root.AddToClassList("sae-root");

            BuildToolbar(root);
            BuildPreview(root);
            BuildScrubber(root);
            BuildLists(root);

            statusLabel = new Label("Space: Play/Pause | Ctrl+F: Add Frame | Ctrl+E: Add Event | Drag handles/gizmo to edit hitboxes/projectiles");
            statusLabel.style.marginTop = 4;
            statusLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            root.Add(statusLabel);
        }

        private void BuildToolbar(VisualElement root)
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList("sae-toolbar");

            var stateField = new ObjectField("Animation State")
            {
                objectType = typeof(AnimationStateSO),
                allowSceneObjects = false
            };
            stateField.RegisterValueChangedCallback(evt =>
            {
                animationState = evt.newValue as AnimationStateSO;
                BindState();
            });
            toolbar.Add(stateField);

            var playButton = new Button(() =>
            {
                if (animationState == null) return;
                isPlaying = !isPlaying;
            })
            {
                text = "Play/Pause"
            };
            toolbar.Add(playButton);

            loopToggle = new Toggle("Loop") { value = true };
            loopToggle.style.marginLeft = 10;
            toolbar.Add(loopToggle);

            var hitboxToggle = new Toggle("Show Hitboxes") { value = showHitboxes };
            hitboxToggle.style.marginLeft = 10;
            hitboxToggle.RegisterValueChangedCallback(evt =>
            {
                showHitboxes = evt.newValue;
                Repaint();
            });
            toolbar.Add(hitboxToggle);

            var speedContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, marginLeft = 10, alignItems = Align.Center } };
            speedContainer.Add(new Label("Speed"));
            speedSlider = new Slider(0.1f, 3f) { value = 1f, style = { width = 150, marginLeft = 4 } };
            speedContainer.Add(speedSlider);
            toolbar.Add(speedContainer);

            var zoomContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, marginLeft = 10, alignItems = Align.Center } };
            zoomContainer.Add(new Label("Zoom"));
            zoomSlider = new Slider(0.5f, 3f) { value = previewZoom, style = { width = 120, marginLeft = 4 } };
            zoomSlider.RegisterValueChangedCallback(evt =>
            {
                previewZoom = evt.newValue;
                Repaint();
            });
            zoomContainer.Add(zoomSlider);
            toolbar.Add(zoomContainer);

            timelineZoomSlider = new Slider("T Zoom", 0.5f, 3f) { value = 1f, style = { width = 140, marginLeft = 6 } };
            timelineZoomSlider.RegisterValueChangedCallback(evt =>
            {
                timelineElement?.SetZoom(evt.newValue);
                Repaint();
            });
            toolbar.Add(timelineZoomSlider);

            fpsField = new FloatField("FPS") { value = 12f, style = { width = 120, marginLeft = 10 } };
            fpsField.RegisterValueChangedCallback(evt =>
            {
                if (animationState == null) return;
                Undo.RecordObject(animationState, "Change FPS");
                animationState.frameRate = Mathf.Max(0.01f, evt.newValue);
                EditorUtility.SetDirty(animationState);
                UpdateScrubUI();
            });
            toolbar.Add(fpsField);

            loopToggle.RegisterValueChangedCallback(evt =>
            {
                if (animationState == null) return;
                Undo.RecordObject(animationState, "Toggle Loop");
                animationState.loop = evt.newValue;
                EditorUtility.SetDirty(animationState);
            });

            var pingButton = new Button(() =>
            {
                if (animationState != null)
                {
                    EditorGUIUtility.PingObject(animationState);
                }
            })
            {
                text = "Ping Asset"
            };
            pingButton.style.marginLeft = 10;
            toolbar.Add(pingButton);

            root.Add(toolbar);
        }

        private void BuildPreview(VisualElement root)
        {
            var previewContainer = new VisualElement();
            previewContainer.AddToClassList("sae-preview");

            previewContainer.style.height = 260;
            this.previewContainer = new IMGUIContainer(DrawPreviewArea);
            this.previewContainer.style.flexGrow = 1f;
            previewContainer.Add(this.previewContainer);
            root.Add(previewContainer);
        }

        private void DrawPreviewArea()
        {
            Rect area = GUILayoutUtility.GetRect(position.width, 240, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawCheckerboard(area, 12f);

            if (animationState == null || animationState.sprites == null || animationState.sprites.Length == 0)
            {
                GUI.Label(area, "No sprite selected", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var sprite = animationState.sprites[Mathf.Clamp(GetCurrentFrameIndex(), 0, animationState.sprites.Length - 1)];
            if (sprite == null)
            {
                GUI.Label(area, "Empty frame", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var tex = sprite.texture;
            var rect = sprite.textureRect;
            Rect uv = new Rect(rect.x / tex.width, rect.y / tex.height, rect.width / tex.width, rect.height / tex.height);

            float scale = previewZoom;
            float width = rect.width * scale;
            float height = rect.height * scale;
            var dst = new Rect(area.center.x - width / 2f, area.center.y - height / 2f, width, height);
            GUI.DrawTextureWithTexCoords(dst, tex, uv, true);

            // Draw hitboxes overlay
            if (showHitboxes && animationState.frameHitboxes != null && animationState.frameHitboxes.Count > 0)
            {
                int frame = Mathf.Clamp(GetCurrentFrameIndex(), 0, animationState.frameHitboxes.Count - 1);
                var frameSet = animationState.frameHitboxes[frame];
                float ppu = sprite.pixelsPerUnit > 0 ? sprite.pixelsPerUnit : 100f;
                for (int i = 0; i < frameSet.hitboxes.Count; i++)
                {
                    var hb = frameSet.hitboxes[i];
                    Vector2 sizePx = hb.size * ppu * scale;
                    Vector2 offsetPx = hb.offset * ppu * scale;
                    var hbRect = new Rect(
                        dst.center.x + offsetPx.x - sizePx.x / 2f,
                        dst.center.y - offsetPx.y - sizePx.y / 2f,
                        sizePx.x,
                        sizePx.y);
                    var fill = i == selectedHitboxIndex ? new Color(1f, 0.5f, 0f, 0.12f) : new Color(1f, 0f, 0f, 0.08f);
                    var line = i == selectedHitboxIndex ? new Color(1f, 0.5f, 0f, 0.9f) : Color.red;
                    Handles.DrawSolidRectangleWithOutline(hbRect, fill, line);

                    // Handle drag to move selected hitbox
                    var evt = Event.current;
                    if (i == selectedHitboxIndex)
                    {
                        if (evt.type == EventType.MouseDown && evt.button == 0 && hbRect.Contains(evt.mousePosition))
                        {
                            isDraggingHitbox = true;
                            draggingFrameIndex = frame;
                            dragStartMouse = evt.mousePosition;
                            dragStartOffset = hb.offset;
                            dragStartSize = hb.size;
                            evt.Use();
                        }
                        else if (evt.type == EventType.MouseDrag && isDraggingHitbox && draggingFrameIndex == frame)
                        {
                            var delta = evt.mousePosition - dragStartMouse;
                            float pxToUnits = 1f / (ppu * scale);
                            Vector2 newOffset = dragStartOffset + new Vector2(delta.x, -delta.y) * pxToUnits;
                            Undo.RecordObject(animationState, "Move Hitbox");
                            hb.offset = newOffset;
                            EditorUtility.SetDirty(animationState);
                            evt.Use();
                            Repaint();
                        }
                        else if (evt.type == EventType.MouseUp && isDraggingHitbox)
                        {
                            isDraggingHitbox = false;
                            draggingFrameIndex = -1;
                            evt.Use();
                        }

                        // Resize handles (corners)
                        Vector2[] corners =
                        {
                            new Vector2(hbRect.xMin, hbRect.yMin),
                            new Vector2(hbRect.xMax, hbRect.yMin),
                            new Vector2(hbRect.xMax, hbRect.yMax),
                            new Vector2(hbRect.xMin, hbRect.yMax)
                        };
                        int[] opposite = { 2, 3, 0, 1 };
                        float handleSize = 6f;
                        for (int c = 0; c < 4; c++)
                        {
                            EditorGUI.BeginChangeCheck();
                            var fmh_302_81_638990436389561648 = Quaternion.identity; Vector2 newPos = Handles.FreeMoveHandle(corners[c], handleSize, Vector3.zero, Handles.DotHandleCap);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Vector2 opp = corners[opposite[c]];
                                Vector2 newCenter = (newPos + opp) * 0.5f;
                                Vector2 newSizePx = new Vector2(Mathf.Abs(newPos.x - opp.x), Mathf.Abs(newPos.y - opp.y));
                                newSizePx.x = Mathf.Max(2f, newSizePx.x);
                                newSizePx.y = Mathf.Max(2f, newSizePx.y);

                                float pxToUnits = 1f / (ppu * scale);
                                Vector2 offsetPxNew = new Vector2(newCenter.x - dst.center.x, -(newCenter.y - dst.center.y));
                                Vector2 sizeUnits = newSizePx * pxToUnits;
                                Vector2 offsetUnits = offsetPxNew * pxToUnits;

                                Undo.RecordObject(animationState, "Resize Hitbox");
                                hb.size = sizeUnits;
                                hb.offset = offsetUnits;
                                EditorUtility.SetDirty(animationState);
                                Repaint();
                            }
                        }

                        // Projectile origin gizmo
                        Vector2 projPosPx = hb.projectileOrigin * ppu * scale;
                        Vector2 projScreen = new Vector2(
                            dst.center.x + projPosPx.x,
                            dst.center.y - projPosPx.y);
                        EditorGUI.BeginChangeCheck();
                        var fmh_359_78_638990499184863637 = Quaternion.identity; Vector2 newProj = Handles.FreeMoveHandle(projScreen, 5f, Vector3.zero, Handles.CircleHandleCap);
                        if (EditorGUI.EndChangeCheck())
                        {
                            float pxToUnits = 1f / (ppu * scale);
                            Vector2 delta = newProj - projScreen;
                            Undo.RecordObject(animationState, "Move Projectile Origin");
                            hb.projectileOrigin += new Vector2(delta.x, -delta.y) * pxToUnits;
                            EditorUtility.SetDirty(animationState);
                            Repaint();
                        }
                        Handles.Label(projScreen + new Vector2(6, -10), "Proj");
                    }
                }
            }
        }

        private void DrawCheckerboard(Rect rect, float size)
        {
            Color c1 = new Color(0.22f, 0.22f, 0.22f);
            Color c2 = new Color(0.26f, 0.26f, 0.26f);
            int xCount = Mathf.CeilToInt(rect.width / size);
            int yCount = Mathf.CeilToInt(rect.height / size);
            for (int y = 0; y < yCount; y++)
            {
                for (int x = 0; x < xCount; x++)
                {
                    var r = new Rect(rect.x + x * size, rect.y + y * size, size, size);
                    EditorGUI.DrawRect(r, ((x + y) % 2 == 0) ? c1 : c2);
                }
            }
        }

        private void BuildScrubber(VisualElement root)
        {
            var scrubContainer = new VisualElement();
            scrubContainer.AddToClassList("sae-scrubber");
            scrubSlider = new Slider(0f, 1f)
            {
                style = { flexGrow = 1, marginRight = 8 }
            };
            scrubSlider.RegisterValueChangedCallback(evt =>
            {
                if (animationState == null || animationState.sprites == null) return;
                currentTime = evt.newValue;
                isPlaying = false;
                UpdatePreviewImage();
            });

            timeLabel = new Label("0.00s")
            {
                style =
                {
                    width = 80,
                    unityTextAlign = TextAnchor.MiddleRight
                }
            };

            scrubContainer.Add(scrubSlider);
            scrubContainer.Add(timeLabel);
            root.Add(scrubContainer);

            clipInfoLabel = new Label("No clip selected");
            clipInfoLabel.AddToClassList("sae-clipinfo");
            root.Add(clipInfoLabel);
        }

        private void BuildLists(VisualElement root)
        {
            var split = new TwoPaneSplitView(0, 320, TwoPaneSplitViewOrientation.Horizontal);
            split.AddToClassList("sae-split");

            var leftPanel = new VisualElement();
            leftPanel.style.flexGrow = 1f;
            leftPanel.Add(new Label("Frames") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 } });

            frameListView = new ListView
            {
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBorder = true,
                reorderable = false,
                fixedItemHeight = 52,
                itemsSource = frameIndices
            };
            frameListView.makeItem = MakeFrameItem;
            frameListView.bindItem = BindFrameItem;
            frameListView.style.flexGrow = 1;
            leftPanel.Add(frameListView);

            var frameButtons = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4 } };
            frameButtons.Add(new Button(AddFrame) { text = "Add" });
            frameButtons.Add(new Button(RemoveSelectedFrames) { text = "Delete Sel", style = { marginLeft = 4 } });
            frameButtons.Add(new Button(() => MoveSelectedFrames(-1)) { text = "↑", style = { width = 28, marginLeft = 4 } });
            frameButtons.Add(new Button(() => MoveSelectedFrames(1)) { text = "↓", style = { width = 28, marginLeft = 2 } });
            leftPanel.Add(frameButtons);

            var durationRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4, alignItems = Align.Center } };
            durationRow.Add(new Label("Set Dur") { style = { width = 52 } });
            var durField = new FloatField { value = 0.1f, style = { width = 70 } };
            durationRow.Add(durField);
            var applyDur = new Button(() => ApplyDurationToSelection(durField.value)) { text = "Apply", style = { marginLeft = 4 } };
            durationRow.Add(applyDur);
            leftPanel.Add(durationRow);

            split.Add(leftPanel);

            // Event timeline / info
            var rightPanel = new VisualElement { style = { paddingLeft = 8, paddingRight = 8 } };
            var eventsFoldout = new Foldout { text = "Events / Timeline", value = true };
            eventsFoldout.Add(new Label("Add/edit/remove events; drag markers to move; double-click to add.") { style = { whiteSpace = WhiteSpace.Normal, marginBottom = 6 } });

            eventListView = new ListView
            {
                selectionType = SelectionType.Single,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBorder = true,
                reorderable = false,
                fixedItemHeight = 26,
                itemsSource = eventIndices
            };
            eventListView.makeItem = MakeEventItem;
            eventListView.bindItem = BindEventItem;
            eventsFoldout.Add(eventListView);

            var eventButtons = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4 } };
            eventButtons.Add(new Button(() => AddEventAt(0f)) { text = "Add Event" });
            eventButtons.Add(new Button(RemoveEvent) { text = "Remove Event", style = { marginLeft = 4 } });
            eventsFoldout.Add(eventButtons);

            timelineElement = new TimelineElement(this)
            {
                style = { height = 140, marginTop = 6 }
            };
            eventsFoldout.Add(timelineElement);
            rightPanel.Add(eventsFoldout);

            var hitboxFoldout = new Foldout { text = "Loop / Hitboxes", value = true };
            hitboxFoldout.style.marginTop = 6;
            var eventSettings = new IMGUIContainer(() =>
            {
                if (serializedState == null) return;
                serializedState.Update();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Loop Events", EditorStyles.boldLabel);
                if (invokeOnFirstLoopProp != null) EditorGUILayout.PropertyField(invokeOnFirstLoopProp, new GUIContent("Invoke On First Loop"));
                if (onLoopOnceProp != null) EditorGUILayout.PropertyField(onLoopOnceProp, new GUIContent("On Loop Once"));

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Per-Frame UnityEvents", EditorStyles.boldLabel);
                EnsureFrameEventCount();
                if (frameEventsProp != null)
                {
                    EditorGUILayout.PropertyField(frameEventsProp, GUIContent.none, true);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Hitboxes (Current Frame)", EditorStyles.boldLabel);
                EnsureFrameHitboxCount();
                if (frameHitboxesProp != null && animationState != null && animationState.sprites != null && animationState.sprites.Length > 0)
                {
                    int frame = GetCurrentFrameIndex();
                    frame = Mathf.Clamp(frame, 0, frameHitboxesProp.arraySize - 1);
                    var frameSetProp = frameHitboxesProp.GetArrayElementAtIndex(frame).FindPropertyRelative("hitboxes");
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Add Hitbox"))
                        {
                            Undo.RecordObject(animationState, "Add Hitbox");
                            frameSetProp.arraySize++;
                            serializedState.ApplyModifiedProperties();
                            selectedHitboxIndex = frameSetProp.arraySize - 1;
                        }
                        using (new EditorGUI.DisabledScope(selectedHitboxIndex < 0 || selectedHitboxIndex >= frameSetProp.arraySize))
                        {
                            if (GUILayout.Button("Remove Hitbox"))
                            {
                                if (selectedHitboxIndex >= 0 && selectedHitboxIndex < frameSetProp.arraySize)
                                {
                                    Undo.RecordObject(animationState, "Remove Hitbox");
                                    frameSetProp.DeleteArrayElementAtIndex(selectedHitboxIndex);
                                    selectedHitboxIndex = Mathf.Min(selectedHitboxIndex, frameSetProp.arraySize - 1);
                                    serializedState.ApplyModifiedProperties();
                                }
                            }
                            if (GUILayout.Button("Copy"))
                            {
                                copyBuffer = DeepCopyFrameSet(frameSetProp);
                            }
                            using (new EditorGUI.DisabledScope(copyBuffer == null))
                            {
                                if (GUILayout.Button("Paste"))
                                {
                                    Undo.RecordObject(animationState, "Paste Hitboxes");
                                    PasteFrameSet(frameSetProp, copyBuffer);
                                    serializedState.ApplyModifiedProperties();
                                    selectedHitboxIndex = Mathf.Clamp(selectedHitboxIndex, 0, frameSetProp.arraySize - 1);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < frameSetProp.arraySize; i++)
                    {
                        var hbProp = frameSetProp.GetArrayElementAtIndex(i);
                        var typeProp = hbProp.FindPropertyRelative("type");
                        string label = $"{i + 1}: {((HitboxType)typeProp.enumValueIndex).ToString()}";
                        bool selected = i == selectedHitboxIndex;
                        if (GUILayout.Toggle(selected, label, "Button"))
                        {
                            selectedHitboxIndex = i;
                        }
                    }

                    if (selectedHitboxIndex >= 0 && selectedHitboxIndex < frameSetProp.arraySize)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Selected Hitbox", EditorStyles.boldLabel);
                        var hbProp = frameSetProp.GetArrayElementAtIndex(selectedHitboxIndex);
                        EditorGUILayout.PropertyField(hbProp.FindPropertyRelative("type"));
                        EditorGUILayout.PropertyField(hbProp.FindPropertyRelative("offset"));
                        EditorGUILayout.PropertyField(hbProp.FindPropertyRelative("size"));
                        EditorGUILayout.PropertyField(hbProp.FindPropertyRelative("damage"));
                        EditorGUILayout.PropertyField(hbProp.FindPropertyRelative("knockback"));
                        EditorGUILayout.PropertyField(hbProp.FindPropertyRelative("hitstun"));
                        EditorGUILayout.PropertyField(hbProp.FindPropertyRelative("projectileOrigin"));
                        EditorGUILayout.PropertyField(hbProp.FindPropertyRelative("fxId"));
                    }
                }
                serializedState.ApplyModifiedProperties();
            });
            eventSettings.style.marginTop = 2;
            hitboxFoldout.Add(eventSettings);
            rightPanel.Add(hitboxFoldout);

            split.Add(rightPanel);
            root.Add(split);
        }

        private void BindState()
        {
            if (animationState == null)
            {
                serializedState = null;
                spritesProp = null;
                eventsProp = null;
                frameIndices.Clear();
                eventIndices.Clear();
                currentTime = 0f;
                UpdateScrubUI();
                UpdatePreviewImage();
                RefreshLists();
                return;
            }

            serializedState = new SerializedObject(animationState);
            spritesProp = serializedState.FindProperty("sprites");
            eventsProp = serializedState.FindProperty("events");
            frameDurationsProp = serializedState.FindProperty("frameDurations");
            frameEventsProp = serializedState.FindProperty("frameEvents");
            frameHitboxesProp = serializedState.FindProperty("frameHitboxes");
            onLoopOnceProp = serializedState.FindProperty("onLoopOnce");
            invokeOnFirstLoopProp = serializedState.FindProperty("invokeOnFirstLoop");
            EnsureDurationCount();
            EnsureFrameEventCount();
            EnsureFrameHitboxCount();
            RefreshLists();
            currentTime = 0f;
            UpdateScrubUI();
            UpdatePreviewImage();
            selectedHitboxIndex = -1;
        }

        private void UpdatePreviewImage()
        {
            if (animationState == null || animationState.sprites == null || animationState.sprites.Length == 0)
            {
                clipInfoLabel.text = "No clip selected";
                return;
            }

            int frame = GetCurrentFrameIndex();
            clipInfoLabel.text = $"{animationState.stateName} | FPS: {animationState.frameRate:0.0} | Frames: {animationState.sprites.Length} | Frame: {frame+1}";
        }

        private void UpdateScrubUI()
        {
            float duration = GetDuration();
            scrubSlider.lowValue = 0f;
            scrubSlider.highValue = Mathf.Max(0.001f, duration);
            scrubSlider.SetValueWithoutNotify(Mathf.Clamp(currentTime, 0f, duration));
            timeLabel.text = $"{currentTime:0.00}s / {duration:0.00}s";
            UpdatePreviewImage();
            Repaint();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Space)
            {
                isPlaying = !isPlaying;
                evt.StopImmediatePropagation();
            }
            else if (evt.ctrlKey && evt.keyCode == KeyCode.F)
            {
                AddFrame();
                evt.StopImmediatePropagation();
            }
            else if (evt.ctrlKey && evt.keyCode == KeyCode.E)
            {
                AddEventAt(currentTime / Mathf.Max(0.0001f, GetDuration()));
                evt.StopImmediatePropagation();
            }
        }

        internal float GetDuration()
        {
            if (animationState == null || animationState.sprites == null || animationState.frameRate <= 0f)
                return 0f;
            if (frameDurationsProp != null && frameDurationsProp.arraySize == animationState.sprites.Length && frameDurationsProp.arraySize > 0)
            {
                float sum = 0f;
                for (int i = 0; i < frameDurationsProp.arraySize; i++)
                {
                    sum += Mathf.Max(0.0001f, frameDurationsProp.GetArrayElementAtIndex(i).floatValue);
                }
                return sum;
            }
            return animationState.sprites.Length / animationState.frameRate;
        }

        private int GetCurrentFrameIndex()
        {
            if (animationState == null || animationState.sprites == null || animationState.frameRate <= 0f)
                return 0;

            if (frameDurationsProp != null && frameDurationsProp.arraySize == animationState.sprites.Length && frameDurationsProp.arraySize > 0)
            {
                float time = currentTime;
                for (int i = 0; i < frameDurationsProp.arraySize; i++)
                {
                    float d = Mathf.Max(0.0001f, frameDurationsProp.GetArrayElementAtIndex(i).floatValue);
                    if (time < d) return i;
                    time -= d;
                }
                return animationState.sprites.Length - 1;
            }

            float frameDuration = 1f / animationState.frameRate;
            int index = Mathf.FloorToInt(currentTime / frameDuration);
            if (loopToggle.value && animationState.sprites.Length > 0)
            {
                index %= animationState.sprites.Length;
            }
            return Mathf.Clamp(index, 0, animationState.sprites.Length - 1);
        }

        internal IReadOnlyList<float> GetFrameBoundaries()
        {
            if (animationState == null || animationState.sprites == null || animationState.sprites.Length == 0) return Array.Empty<float>();
            if (frameDurationsProp != null && frameDurationsProp.arraySize == animationState.sprites.Length)
            {
                float t = 0f;
                var list = new List<float>(frameDurationsProp.arraySize);
                for (int i = 0; i < frameDurationsProp.arraySize; i++)
                {
                    list.Add(t);
                    t += Mathf.Max(0.0001f, frameDurationsProp.GetArrayElementAtIndex(i).floatValue);
                }
                return list;
            }
            float uniform = animationState.frameRate > 0f ? 1f / animationState.frameRate : 0.1f;
            return Enumerable.Range(0, animationState.sprites.Length).Select(i => i * uniform).ToArray();
        }

        private void DrawEventList()
        {
            // Deprecated IMGUI path removed
        }

        private void DrawEventTimeline()
        {
            // Deprecated IMGUI path removed
        }

        private void EnsureDurationCount()
        {
            if (animationState == null || frameDurationsProp == null || spritesProp == null) return;
            if (frameDurationsProp.arraySize != spritesProp.arraySize)
            {
                frameDurationsProp.arraySize = spritesProp.arraySize;
                float defaultDur = animationState.frameRate > 0f ? 1f / animationState.frameRate : 0.1f;
                for (int i = 0; i < frameDurationsProp.arraySize; i++)
                {
                    var durProp = frameDurationsProp.GetArrayElementAtIndex(i);
                    if (durProp.floatValue <= 0f)
                    {
                        durProp.floatValue = defaultDur;
                    }
                }
                serializedState.ApplyModifiedProperties();
            }
        }

        private void EnsureFrameEventCount()
        {
            if (animationState == null || frameEventsProp == null || spritesProp == null) return;
            if (frameEventsProp.arraySize != spritesProp.arraySize)
            {
                frameEventsProp.arraySize = spritesProp.arraySize;
                for (int i = 0; i < frameEventsProp.arraySize; i++)
                {
                    var evtProp = frameEventsProp.GetArrayElementAtIndex(i);
                    if (evtProp != null && evtProp.managedReferenceId == 0)
                    {
                        // Ensure list slots exist; UnityEvent serialized refs are created automatically
                    }
                }
                serializedState.ApplyModifiedProperties();
            }
        }

        private void EnsureFrameHitboxCount()
        {
            if (animationState == null || frameHitboxesProp == null || spritesProp == null) return;
            if (frameHitboxesProp.arraySize != spritesProp.arraySize)
            {
                frameHitboxesProp.arraySize = spritesProp.arraySize;
                serializedState.ApplyModifiedProperties();
            }
        }

        private AnimationStateSO.FrameHitboxSet DeepCopyFrameSet(SerializedProperty frameSetProp)
        {
            var copy = new AnimationStateSO.FrameHitboxSet();
            var listProp = frameSetProp.FindPropertyRelative("hitboxes");
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var hbProp = listProp.GetArrayElementAtIndex(i);
                var hb = new AnimationStateSO.HitboxFrameData
                {
                    type = (HitboxType)hbProp.FindPropertyRelative("type").enumValueIndex,
                    offset = hbProp.FindPropertyRelative("offset").vector2Value,
                    size = hbProp.FindPropertyRelative("size").vector2Value,
                    damage = hbProp.FindPropertyRelative("damage").floatValue,
                    knockback = hbProp.FindPropertyRelative("knockback").vector2Value,
                    hitstun = hbProp.FindPropertyRelative("hitstun").floatValue,
                    projectileOrigin = hbProp.FindPropertyRelative("projectileOrigin").vector2Value,
                    fxId = hbProp.FindPropertyRelative("fxId").stringValue
                };
                copy.hitboxes.Add(hb);
            }
            return copy;
        }

        private void PasteFrameSet(SerializedProperty frameSetProp, AnimationStateSO.FrameHitboxSet buffer)
        {
            if (buffer == null) return;
            var listProp = frameSetProp.FindPropertyRelative("hitboxes");
            listProp.arraySize = buffer.hitboxes.Count;
            for (int i = 0; i < buffer.hitboxes.Count; i++)
            {
                var hbProp = listProp.GetArrayElementAtIndex(i);
                hbProp.FindPropertyRelative("type").enumValueIndex = (int)buffer.hitboxes[i].type;
                hbProp.FindPropertyRelative("offset").vector2Value = buffer.hitboxes[i].offset;
                hbProp.FindPropertyRelative("size").vector2Value = buffer.hitboxes[i].size;
                hbProp.FindPropertyRelative("damage").floatValue = buffer.hitboxes[i].damage;
                hbProp.FindPropertyRelative("knockback").vector2Value = buffer.hitboxes[i].knockback;
                hbProp.FindPropertyRelative("hitstun").floatValue = buffer.hitboxes[i].hitstun;
                hbProp.FindPropertyRelative("projectileOrigin").vector2Value = buffer.hitboxes[i].projectileOrigin;
                hbProp.FindPropertyRelative("fxId").stringValue = buffer.hitboxes[i].fxId;
            }
        }
        internal void SetTimeFromTimeline(float norm)
        {
            float duration = GetDuration();
            currentTime = Mathf.Clamp(norm * duration, 0f, duration);
            UpdateScrubUI();
        }

        internal void AddEventAt(float normalizedTime)
        {
            if (eventsProp == null) return;
            Undo.RecordObject(animationState, "Add Event");
            serializedState.Update();
            int index = eventsProp.arraySize;
            eventsProp.arraySize++;
            var element = eventsProp.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("normalizedTime").floatValue = Mathf.Clamp01(normalizedTime);
            element.FindPropertyRelative("functionName").stringValue = "OnAnimationEvent";
            element.FindPropertyRelative("stringParameter").stringValue = string.Empty;
            serializedState.ApplyModifiedProperties();
            RefreshLists();
            eventListView.selectedIndex = index;
            Repaint();
        }

        internal void RefreshLists()
        {
            frameIndices.Clear();
            eventIndices.Clear();
            if (spritesProp != null)
            {
                for (int i = 0; i < spritesProp.arraySize; i++) frameIndices.Add(i);
            }
            if (eventsProp != null)
            {
                for (int i = 0; i < eventsProp.arraySize; i++) eventIndices.Add(i);
            }
            frameListView?.Rebuild();
            eventListView?.Rebuild();
            timelineElement?.MarkDirtyRepaint();
        }

        private VisualElement MakeFrameItem()
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, paddingLeft = 2, paddingRight = 2 } };
            var thumb = new Image { scaleMode = ScaleMode.ScaleToFit, style = { width = 46, height = 46, marginRight = 6 } };
            var spriteField = new ObjectField { objectType = typeof(Sprite), style = { flexGrow = 1 } };
            var durField = new FloatField { label = "Dur", style = { width = 80, marginLeft = 4 } };
            var upBtn = new Button { text = "▲", style = { width = 20, marginLeft = 4 } };
            var downBtn = new Button { text = "▼", style = { width = 20, marginLeft = 2 } };
            row.Add(thumb);
            row.Add(spriteField);
            row.Add(durField);
            row.Add(upBtn);
            row.Add(downBtn);
            row.userData = new FrameRow { thumb = thumb, spriteField = spriteField, durField = durField, upButton = upBtn, downButton = downBtn };
            return row;
        }

        private void BindFrameItem(VisualElement element, int index)
        {
            if (spritesProp == null || frameDurationsProp == null || index >= frameIndices.Count) return;
            int actual = frameIndices[index];
            var row = (FrameRow)element.userData;
            var spriteProp = spritesProp.GetArrayElementAtIndex(actual);
            var durProp = frameDurationsProp.GetArrayElementAtIndex(actual);
            row.spriteField.BindProperty(spriteProp);
            row.durField.BindProperty(durProp);
            var sprite = spriteProp.objectReferenceValue as Sprite;
            if (row.thumb != null)
            {
                row.thumb.image = sprite != null ? sprite.texture : Texture2D.grayTexture;
            }
            row.upButton.clicked -= row.UpHandler;
            row.downButton.clicked -= row.DownHandler;
            row.UpHandler = () => MoveFrame(actual, -1);
            row.DownHandler = () => MoveFrame(actual, 1);
            row.upButton.clicked += row.UpHandler;
            row.downButton.clicked += row.DownHandler;
        }

        private void MoveFrame(int index, int delta)
        {
            if (spritesProp == null || frameDurationsProp == null) return;
            int target = index + delta;
            if (target < 0 || target >= spritesProp.arraySize) return;
            Undo.RecordObject(animationState, "Reorder Frame");
            serializedState.Update();
            spritesProp.MoveArrayElement(index, target);
            frameDurationsProp.MoveArrayElement(index, target);
            frameEventsProp?.MoveArrayElement(index, target);
            frameHitboxesProp?.MoveArrayElement(index, target);
            serializedState.ApplyModifiedProperties();
            RefreshLists();
        }

        private void AddFrame()
        {
            if (serializedState == null || spritesProp == null || frameDurationsProp == null) return;
            Undo.RecordObject(animationState, "Add Frame");
            serializedState.Update();
            spritesProp.arraySize++;
            frameDurationsProp.arraySize = spritesProp.arraySize;
            var durProp = frameDurationsProp.GetArrayElementAtIndex(frameDurationsProp.arraySize - 1);
            durProp.floatValue = animationState != null && animationState.frameRate > 0 ? 1f / animationState.frameRate : 0.1f;
            if (frameEventsProp != null)
            {
                frameEventsProp.arraySize = spritesProp.arraySize;
            }
            if (frameHitboxesProp != null)
            {
                frameHitboxesProp.arraySize = spritesProp.arraySize;
            }
            serializedState.ApplyModifiedProperties();
            RefreshLists();
        }

        private void RemoveFrame()
        {
            if (serializedState == null || spritesProp == null || frameDurationsProp == null || frameListView.selectedIndex < 0) return;
            Undo.RecordObject(animationState, "Remove Frame");
            serializedState.Update();
            int idx = frameListView.selectedIndex;
            spritesProp.DeleteArrayElementAtIndex(idx);
            if (idx < frameDurationsProp.arraySize) frameDurationsProp.DeleteArrayElementAtIndex(idx);
            if (frameEventsProp != null && idx < frameEventsProp.arraySize) frameEventsProp.DeleteArrayElementAtIndex(idx);
            if (frameHitboxesProp != null && idx < frameHitboxesProp.arraySize) frameHitboxesProp.DeleteArrayElementAtIndex(idx);
            serializedState.ApplyModifiedProperties();
            RefreshLists();
        }

        private void RemoveSelectedFrames()
        {
            if (serializedState == null || spritesProp == null || frameDurationsProp == null) return;
            var sel = frameListView.selectedIndices.ToList();
            if (sel.Count == 0) return;
            sel.Sort();
            Undo.RecordObject(animationState, "Remove Selected Frames");
            serializedState.Update();
            for (int i = sel.Count - 1; i >= 0; i--)
            {
                int idx = sel[i];
                if (idx < spritesProp.arraySize) spritesProp.DeleteArrayElementAtIndex(idx);
                if (idx < frameDurationsProp.arraySize) frameDurationsProp.DeleteArrayElementAtIndex(idx);
                if (frameEventsProp != null && idx < frameEventsProp.arraySize) frameEventsProp.DeleteArrayElementAtIndex(idx);
                if (frameHitboxesProp != null && idx < frameHitboxesProp.arraySize) frameHitboxesProp.DeleteArrayElementAtIndex(idx);
            }
            serializedState.ApplyModifiedProperties();
            RefreshLists();
        }

        private void MoveSelectedFrames(int delta)
        {
            if (serializedState == null || spritesProp == null || frameDurationsProp == null) return;
            var sel = frameListView.selectedIndices.ToList();
            if (sel.Count == 0) return;
            sel.Sort();
            Undo.RecordObject(animationState, "Reorder Frames");
            serializedState.Update();
            if (delta < 0)
            {
                foreach (var idx in sel)
                {
                    int target = idx + delta;
                    if (target >= 0)
                    {
                        spritesProp.MoveArrayElement(idx, target);
                        frameDurationsProp.MoveArrayElement(idx, target);
                        frameEventsProp?.MoveArrayElement(idx, target);
                        frameHitboxesProp?.MoveArrayElement(idx, target);
                    }
                }
            }
            else if (delta > 0)
            {
                for (int i = sel.Count - 1; i >= 0; i--)
                {
                    int idx = sel[i];
                    int target = idx + delta;
                    if (target < spritesProp.arraySize)
                    {
                        spritesProp.MoveArrayElement(idx, target);
                        frameDurationsProp.MoveArrayElement(idx, target);
                        frameEventsProp?.MoveArrayElement(idx, target);
                        frameHitboxesProp?.MoveArrayElement(idx, target);
                    }
                }
            }
            serializedState.ApplyModifiedProperties();
            RefreshLists();
        }

        private void ApplyDurationToSelection(float duration)
        {
            if (serializedState == null || spritesProp == null || frameDurationsProp == null) return;
            var sel = frameListView.selectedIndices.ToList();
            if (sel.Count == 0) return;
            Undo.RecordObject(animationState, "Set Frame Duration");
            duration = Mathf.Max(0.0001f, duration);
            serializedState.Update();
            foreach (var idx in sel)
            {
                if (idx < frameDurationsProp.arraySize)
                {
                    frameDurationsProp.GetArrayElementAtIndex(idx).floatValue = duration;
                }
            }
            serializedState.ApplyModifiedProperties();
            RefreshLists();
        }

        private VisualElement MakeEventItem()
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, paddingLeft = 2, paddingRight = 2 } };
            var timeField = new FloatField { style = { width = 70 } };
            var funcField = new TextField { style = { flexGrow = 1, marginLeft = 4 } };
            var paramField = new TextField { style = { flexGrow = 1, marginLeft = 4 } };
            var upBtn = new Button { text = "↑", style = { width = 20, marginLeft = 4 } };
            var downBtn = new Button { text = "↓", style = { width = 20, marginLeft = 2 } };
            row.Add(timeField);
            row.Add(funcField);
            row.Add(paramField);
            row.Add(upBtn);
            row.Add(downBtn);
            row.userData = new EventRow { timeField = timeField, funcField = funcField, paramField = paramField, upButton = upBtn, downButton = downBtn };
            return row;
        }

        private void BindEventItem(VisualElement element, int index)
        {
            if (eventsProp == null || index >= eventIndices.Count) return;
            int actual = eventIndices[index];
            var row = (EventRow)element.userData;
            var evtProp = eventsProp.GetArrayElementAtIndex(actual);
            row.timeField.BindProperty(evtProp.FindPropertyRelative("normalizedTime"));
            row.funcField.BindProperty(evtProp.FindPropertyRelative("functionName"));
            row.paramField.BindProperty(evtProp.FindPropertyRelative("stringParameter"));
            row.upButton.clicked -= row.UpHandler;
            row.downButton.clicked -= row.DownHandler;
            row.UpHandler = () => MoveEvent(actual, -1);
            row.DownHandler = () => MoveEvent(actual, 1);
            row.upButton.clicked += row.UpHandler;
            row.downButton.clicked += row.DownHandler;
        }

        private void MoveEvent(int index, int delta)
        {
            if (eventsProp == null) return;
            int target = index + delta;
            if (target < 0 || target >= eventsProp.arraySize) return;
            serializedState.Update();
            eventsProp.MoveArrayElement(index, target);
            serializedState.ApplyModifiedProperties();
            RefreshLists();
        }

        private void RemoveEvent()
        {
            if (eventsProp == null || eventListView.selectedIndex < 0) return;
            Undo.RecordObject(animationState, "Remove Event");
            serializedState.Update();
            eventsProp.DeleteArrayElementAtIndex(eventListView.selectedIndex);
            serializedState.ApplyModifiedProperties();
            RefreshLists();
        }

        private struct FrameRow
        {
            public Image thumb;
            public ObjectField spriteField;
            public FloatField durField;
            public Button upButton;
            public Button downButton;
            public Action UpHandler;
            public Action DownHandler;
            public AnimationStateSO.FrameHitboxSet hitboxSetCopy;
        }

        private struct EventRow
        {
            public FloatField timeField;
            public TextField funcField;
            public TextField paramField;
            public Button upButton;
            public Button downButton;
            public Action UpHandler;
            public Action DownHandler;
        }
    }

    internal sealed class TimelineElement : VisualElement
    {
        private readonly SpriteAnimationEditorWindow window;
        private int draggingEvent = -1;
        private bool draggingPlayhead;
        private float zoom = 1f;

        public TimelineElement(SpriteAnimationEditorWindow window)
        {
            this.window = window;
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            tooltip = "Drag playhead, drag events to move, double-click to add (snaps to frames)";
        }

        public void SetZoom(float value)
        {
            zoom = Mathf.Max(0.1f, value);
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            Rect rect = contentRect;
            painter.strokeColor = new Color(0.2f, 0.2f, 0.2f);
            painter.lineWidth = 1.5f;
            painter.BeginPath();
            painter.MoveTo(rect.min);
            painter.LineTo(new Vector2(rect.xMax, rect.yMin));
            painter.LineTo(rect.max);
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.ClosePath();
            painter.Fill(FillRule.NonZero);
            painter.Stroke();

            var state = window.animationState;
            if (state == null || state.sprites == null || state.sprites.Length == 0) return;
            float duration = window.GetDuration();
            if (duration <= 0f) duration = 0.001f;
            duration /= Mathf.Max(0.1f, zoom);

            // Axis
            painter.strokeColor = new Color(0.6f, 0.6f, 0.6f);
            painter.lineWidth = 1f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.x, rect.yMax - 20));
            painter.LineTo(new Vector2(rect.xMax, rect.yMax - 20));
            painter.Stroke();

            // Markers every 0.25s
            int markerCount = Mathf.CeilToInt(duration / 0.25f);
            for (int i = 0; i <= markerCount; i++)
            {
                float t = i * 0.25f;
                float x = Mathf.Lerp(rect.x, rect.xMax, t / duration);
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, rect.yMax - 25));
                painter.LineTo(new Vector2(x, rect.yMax - 15));
                painter.Stroke();
            }

            // Events
            var eventsProp = window.eventsProp;
            if (eventsProp != null)
            {
                for (int i = 0; i < eventsProp.arraySize; i++)
                {
                    var element = eventsProp.GetArrayElementAtIndex(i);
                    float norm = Mathf.Clamp01(element.FindPropertyRelative("normalizedTime").floatValue);
                    float x = Mathf.Lerp(rect.x, rect.xMax, norm);
                    float y = rect.y + 10;
                    painter.BeginPath();
                    painter.strokeColor = new Color(1f, 0.6f, 0.2f);
                    painter.fillColor = new Color(1f, 0.6f, 0.2f, 0.6f);
                    painter.MoveTo(new Vector2(x - 4, y));
                    painter.LineTo(new Vector2(x + 4, y));
                    painter.LineTo(new Vector2(x, y + rect.height - 40));
                    painter.ClosePath();
                    painter.Fill();
                    painter.Stroke();
                }
            }

            // Frame markers (hitboxes/events counts)
            var frameBounds = window.GetFrameBoundaries();
            if (frameBounds != null && frameBounds.Count > 0 && window.animationState != null)
            {
                float total = duration;
                for (int i = 0; i < frameBounds.Count; i++)
                {
                    float t = frameBounds[i];
                    float x = Mathf.Lerp(rect.x, rect.xMax, t / total);
                    int hitboxCount = 0;
                    if (window.animationState.frameHitboxes != null && i < window.animationState.frameHitboxes.Count)
                    {
                        var fh = window.animationState.frameHitboxes[i];
                        hitboxCount = fh != null && fh.hitboxes != null ? fh.hitboxes.Count : 0;
                    }
                    // draw small marker
                    painter.strokeColor = hitboxCount > 0 ? new Color(0f, 0.8f, 1f) : new Color(0.4f, 0.4f, 0.4f);
                    painter.lineWidth = 2f;
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(x, rect.y));
                    painter.LineTo(new Vector2(x, rect.y + 8));
                    painter.Stroke();
                }
            }

            // Playhead
            float playX = Mathf.Lerp(rect.x, rect.xMax, duration > 0f ? window.currentTime / duration : 0f);
            painter.strokeColor = Color.red;
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(playX, rect.y));
            painter.LineTo(new Vector2(playX, rect.yMax));
            painter.Stroke();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (window.animationState == null) return;
            Rect rect = contentRect;
            float duration = Mathf.Max(0.001f, window.GetDuration() / Mathf.Max(0.1f, zoom));
            float norm = Mathf.InverseLerp(rect.x, rect.xMax, evt.localPosition.x) / Mathf.Max(0.1f, zoom);

            int hitEvent = HitTestEvent(norm, rect, duration);
            if (hitEvent >= 0)
            {
                Undo.RecordObject(window.animationState, "Move Event");
                draggingEvent = hitEvent;
            }
            else
            {
                draggingPlayhead = true;
                window.SetTimeFromTimeline(norm);
                window.isPlaying = false;
            }
            evt.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (window.animationState == null) return;
            Rect rect = contentRect;
            float duration = Mathf.Max(0.001f, window.GetDuration() / Mathf.Max(0.1f, zoom));
            float norm = Mathf.InverseLerp(rect.x, rect.xMax, evt.localPosition.x) / Mathf.Max(0.1f, zoom);
            norm = SnapToFrames(norm);
            if (draggingEvent >= 0)
            {
                norm = Mathf.Clamp01(norm);
                window.eventsProp.GetArrayElementAtIndex(draggingEvent).FindPropertyRelative("normalizedTime").floatValue = norm;
                window.serializedState.ApplyModifiedProperties();
                window.RefreshLists();
            }
            else if (draggingPlayhead)
            {
                window.SetTimeFromTimeline(norm);
                window.isPlaying = false;
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (window.animationState == null) return;
            Rect rect = contentRect;
            float duration = Mathf.Max(0.001f, window.GetDuration() / Mathf.Max(0.1f, zoom));
            float norm = SnapToFrames(Mathf.InverseLerp(rect.x, rect.xMax, evt.localPosition.x) / Mathf.Max(0.1f, zoom));
            if (evt.clickCount == 2 && draggingEvent < 0)
            {
                window.AddEventAt(norm);
            }
            draggingEvent = -1;
            draggingPlayhead = false;
        }

        private int HitTestEvent(float norm, Rect rect, float duration)
        {
            var eventsProp = window.eventsProp;
            if (eventsProp == null) return -1;
            for (int i = 0; i < eventsProp.arraySize; i++)
            {
                float evNorm = Mathf.Clamp01(eventsProp.GetArrayElementAtIndex(i).FindPropertyRelative("normalizedTime").floatValue);
                float evX = Mathf.Lerp(rect.x, rect.xMax, evNorm);
                if (Mathf.Abs(evX - Mathf.Lerp(rect.x, rect.xMax, norm)) < 8f)
                {
                    return i;
                }
            }
            return -1;
        }

        private float SnapToFrames(float norm)
        {
            var frames = window.GetFrameBoundaries();
            float duration = Mathf.Max(0.0001f, window.GetDuration());
            if (frames == null || frames.Count == 0) return norm;
            float targetTime = norm * duration;
            float closest = frames.Aggregate((x, y) => Mathf.Abs(x - targetTime) < Mathf.Abs(y - targetTime) ? x : y);
            return Mathf.Clamp01(closest / duration);
        }
    }
}



