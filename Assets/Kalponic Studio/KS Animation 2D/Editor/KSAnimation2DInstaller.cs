using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace KalponicStudio.Editor
{
    /// <summary>
    /// Drag-and-drop installer for KS Animation 2D
    /// Automatically sets up the system in any Unity project
    /// Creates demo scene, example assets, and validates installation
    /// </summary>
    public class KSAnimation2DInstaller : EditorWindow
    {
        private Vector2 scrollPos;
        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Install System", false, 0)]
        static void ShowInstaller()
        {
            KSAnimation2DInstaller window = GetWindow<KSAnimation2DInstaller>();
            window.titleContent = new GUIContent("KS Animation 2D Installer");
            window.minSize = new Vector2(400, 300);
        }

        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Create Demo Scene", false, 1)]
        static void CreateDemoScene()
        {
            CreateDemoSceneInternal();
        }

        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Validate Installation", false, 2)]
        static void ValidateInstallation()
        {
            ValidateInstallationInternal();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("KS Animation 2D - Modular 2D Animation System", EditorStyles.boldLabel);
            GUILayout.Label("Version 2.0.0 - Code-first animation that solves Unity Animator Controller shortfalls");
            GUILayout.Space(20);

            if (GUILayout.Button("🚀 Quick Install (Recommended)", GUILayout.Height(40)))
            {
                QuickInstall();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("🎮 Create Demo Scene", GUILayout.Height(30)))
            {
                CreateDemoSceneInternal();
            }

            if (GUILayout.Button("✅ Validate Installation", GUILayout.Height(30)))
            {
                ValidateInstallationInternal();
            }

            if (GUILayout.Button("📖 Open Documentation", GUILayout.Height(30)))
            {
                OpenDocumentation();
            }

            GUILayout.Space(20);
            GUILayout.Label("Features:", EditorStyles.boldLabel);
            GUILayout.Label("• Code-first state machines (no graph complexity)");
            GUILayout.Label("• Priority-based state interruptions");
            GUILayout.Label("• Frame-perfect animation events");
            GUILayout.Label("• Smooth crossfading and blending");
            GUILayout.Label("• Modular and reusable across projects");
            GUILayout.Label("• No external dependencies");

            GUILayout.Space(20);
            GUILayout.Label("Installation Status:", EditorStyles.boldLabel);

            bool isInstalled = CheckInstallation();
            if (isInstalled)
            {
                GUILayout.Label("✅ System is properly installed", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.green } });
            }
            else
            {
                GUILayout.Label("❌ System not found - run Quick Install", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } });
            }

            EditorGUILayout.EndScrollView();
        }

        private static void QuickInstall()
        {
            if (EditorUtility.DisplayDialog("KS Animation 2D Quick Install",
                "This will:\n• Create demo scene\n• Generate example assets\n• Validate installation\n\nContinue?",
                "Install", "Cancel"))
            {
                CreateDemoSceneInternal();
                CreateExampleAssets();
                ValidateInstallationInternal();

                EditorUtility.DisplayDialog("Installation Complete!",
                    "KS Animation 2D has been installed successfully!\n\n" +
                    "Next steps:\n1. Open the demo scene\n2. Press Play to test\n3. Check the documentation\n\n" +
                    "The system is now ready for use in your game!",
                    "Got it!");
            }
        }

        private static void CreateDemoSceneInternal()
        {
            // Create a new scene
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            // Create demo character
            GameObject character = new GameObject("DemoCharacter");
            character.transform.position = Vector3.zero;

            // Add required components
            var spriteRenderer = character.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateDefaultSprite();

            var animator = character.AddComponent<KalponicStudio.PlayableAnimatorComponent>();
            var rb = character.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // Top-down

            var controller = character.AddComponent<DemoCharacterController>();

            // Create camera
            GameObject cameraObj = new GameObject("Main Camera");
            var camera = cameraObj.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.transform.position = new Vector3(0, 0, -10);

            // Save scene
            string scenePath = "Assets/Scenes/KSAnimation2DDemo.unity";
            EnsureDirectoryExists(Path.GetDirectoryName(scenePath));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log("Demo scene created at: " + scenePath);
        }

        private static void CreateExampleAssets()
        {
            // Create example CharacterTypeSO
            string assetPath = "Assets/KS Animation 2D Examples/ExampleCharacterType.asset";
            EnsureDirectoryExists(Path.GetDirectoryName(assetPath));

            var characterType = ScriptableObject.CreateInstance<KalponicStudio.CharacterTypeSO>();

            // Add example animations (placeholder sprites)
            var idleState = ScriptableObject.CreateInstance<KalponicStudio.AnimationStateSO>();
            idleState.sprites = new Sprite[] { CreateDefaultSprite() };
            idleState.frameRate = 12f;
            idleState.name = "Idle";

            var walkState = ScriptableObject.CreateInstance<KalponicStudio.AnimationStateSO>();
            walkState.sprites = new Sprite[] { CreateDefaultSprite(), CreateDefaultSprite() };
            walkState.frameRate = 8f;
            walkState.name = "Walk";

            // Use reflection to set states
            var statesField = characterType.GetType().GetField("states",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (statesField != null)
            {
                var stateDict = new System.Collections.Generic.Dictionary<string, KalponicStudio.AnimationStateSO>();
                stateDict["Idle"] = idleState;
                stateDict["Walk"] = walkState;
                statesField.SetValue(characterType, stateDict);
            }

            AssetDatabase.CreateAsset(characterType, assetPath);
            AssetDatabase.CreateAsset(idleState, "Assets/KS Animation 2D Examples/IdleState.asset");
            AssetDatabase.CreateAsset(walkState, "Assets/KS Animation 2D Examples/WalkState.asset");
            AssetDatabase.SaveAssets();

            Debug.Log("Example assets created at: " + assetPath);
        }

        private static bool CheckInstallation()
        {
            // Check if core components exist
            return System.Type.GetType("KalponicStudio.PlayableAnimatorComponent, Assembly-CSharp") != null &&
                   System.Type.GetType("KalponicStudio.AnimationStateMachine, Assembly-CSharp") != null &&
                   System.Type.GetType("KalponicStudio.CharacterTypeSO, Assembly-CSharp") != null;
        }

        private static void ValidateInstallationInternal()
        {
            bool isValid = true;
            string report = "KS Animation 2D Installation Report:\n\n";

            // Check core components
            if (System.Type.GetType("KalponicStudio.PlayableAnimatorComponent, Assembly-CSharp") != null)
            {
                report += "✅ PlayableAnimatorComponent found\n";
            }
            else
            {
                report += "❌ PlayableAnimatorComponent missing\n";
                isValid = false;
            }

            if (System.Type.GetType("KalponicStudio.AnimationStateMachine, Assembly-CSharp") != null)
            {
                report += "✅ AnimationStateMachine found\n";
            }
            else
            {
                report += "❌ AnimationStateMachine missing\n";
                isValid = false;
            }

            if (System.Type.GetType("KalponicStudio.CharacterTypeSO, Assembly-CSharp") != null)
            {
                report += "✅ CharacterTypeSO found\n";
            }
            else
            {
                report += "❌ CharacterTypeSO missing\n";
                isValid = false;
            }

            // Note: Scripting define symbols check removed for cross-version compatibility
            report += "ℹ️  KS Animation 2D uses runtime component detection\n";

            report += "\n" + (isValid ? "✅ Installation is valid!" : "❌ Installation has issues - check file locations");

            Debug.Log(report);
            EditorUtility.DisplayDialog("Installation Validation",
                report.Replace("✅", "✓").Replace("❌", "✗").Replace("ℹ️", "ℹ"),
                "OK");
        }

        private static void OpenDocumentation()
        {
            string docPath = "Assets/Plugins/KS Animation 2D/README.md";
            if (File.Exists(docPath))
            {
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(docPath, 1);
            }
            else
            {
                Application.OpenURL("https://github.com/jony100200/Anomaly-Directive/blob/main/Assets/Plugins/KS%20Animation%202D/README.md");
            }
        }

        private static Sprite CreateDefaultSprite()
        {
            // Create a simple colored texture for demo purposes
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }
            texture.SetPixels(colors);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    /// <summary>
    /// Simple demo character controller for the demo scene
    /// Shows basic usage of KS Animation 2D
    /// </summary>
    public class DemoCharacterController : MonoBehaviour
    {
        [Header("Animation")]
        [Tooltip("Reference to the PlayableAnimatorComponent.")]
        [SerializeField] private KalponicStudio.PlayableAnimatorComponent animator;
        [Tooltip("Speed of movement.")]
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (animator == null)
                animator = GetComponent<KalponicStudio.PlayableAnimatorComponent>();
        }

        private void Update()
        {
            // Simple movement and animation
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            Vector2 movement = new Vector2(moveX, moveY) * moveSpeed;
            rb.linearVelocity = movement;

            // Simple animation logic
            if (movement.magnitude > 0.1f)
            {
                if (animator != null)
                    animator.Play("Walk");
            }
            else
            {
                if (animator != null)
                    animator.Play("Idle");
            }

            // Flip sprite based on direction
            if (moveX != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(moveX), 1, 1);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("KS Animation 2D Demo");
            GUILayout.Label("Use WASD to move the character");
            GUILayout.Label("Watch the animations change!");
        }
    }
}
