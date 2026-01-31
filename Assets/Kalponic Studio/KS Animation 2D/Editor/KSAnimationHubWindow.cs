using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KalponicStudio.Editor
{
    /// <summary>
    /// Central UIToolkit hub for KS Animation 2D tools.
    /// </summary>
    public sealed class KSAnimationHubWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Kalponic Studio/Animation/KS Animation 2D/Hub";

        [MenuItem(MenuPath)]
        public static void ShowWindow()
        {
            var window = GetWindow<KSAnimationHubWindow>();
            window.titleContent = new GUIContent("KS Animation 2D Hub");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;

            var title = new Label("KS Animation 2D - Hub")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 16,
                    marginBottom = 8
                }
            };
            root.Add(title);

            var desc = new Label("Quick access to installer, setup, editors, and diagnostics.")
            {
                style = { marginBottom = 12, whiteSpace = WhiteSpace.Normal }
            };
            root.Add(desc);

            var grid = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            grid.Add(MakeButton("Install System", () => EditorApplication.ExecuteMenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Install System")));
            grid.Add(MakeButton("Validate Installation", () => EditorApplication.ExecuteMenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Validate Installation")));
            grid.Add(MakeButton("Create Demo Scene", () => EditorApplication.ExecuteMenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Create Demo Scene")));
            grid.Add(MakeButton("Auto Setup", () => EditorApplication.ExecuteMenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Auto Setup/Setup Selected Object")));
            grid.Add(MakeButton("Profile Wizard", () => EditorApplication.ExecuteMenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Profile Wizard")));
            grid.Add(MakeButton("Sprite Animation Editor", () => EditorApplication.ExecuteMenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Sprite Animation Editor")));
            grid.Add(MakeButton("Live Debugger", () => EditorApplication.ExecuteMenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Live Debugger")));
            grid.Add(MakeButton("Hitbox Matrix Editor", () => EditorApplication.ExecuteMenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Hitbox Collision Matrix")));
            grid.Add(MakeButton("Character Editor", () => EditorApplication.ExecuteMenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Character Editor")));
            root.Add(grid);

            var footer = new Label("Tip: Use the Sprite Animation Editor for frame/event editing.")
            {
                style = { marginTop = 12, whiteSpace = WhiteSpace.Normal }
            };
            root.Add(footer);
        }

        private Button MakeButton(string text, System.Action onClick)
        {
            var button = new Button(onClick) { text = text };
            button.style.width = 180;
            button.style.height = 32;
            button.style.marginRight = 8;
            button.style.marginBottom = 8;
            return button;
        }
    }
}
