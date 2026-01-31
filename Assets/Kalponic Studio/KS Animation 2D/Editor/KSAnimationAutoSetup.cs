using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace KalponicStudio.Editor
{
    /// <summary>
    /// Auto-setup system for KS Animation 2D components
    /// Provides automatic component detection, validation, and one-click setup
    /// </summary>
    public static class KSAnimationAutoSetup
    {
        #region Menu Items
        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Auto Setup/Setup Selected Object", false, 100)]
        private static void SetupSelectedObject()
        {
            var selected = Selection.activeGameObject;
            if (selected != null)
            {
                SetupAnimationComponents(selected);
            }
            else
            {
                EditorUtility.DisplayDialog("No Selection", "Please select a GameObject to set up animation components.", "OK");
            }
        }

        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Auto Setup/Validate Scene", false, 101)]
        private static void ValidateScene()
        {
            var results = ValidateAllAnimationObjects();
            ShowValidationResults(results);
        }

        [MenuItem("Tools/Kalponic Studio/Animation/KS Animation 2D/Auto Setup/Create Animation Template", false, 102)]
        private static void CreateAnimationTemplate()
        {
            var template = CreateAnimationObjectTemplate();
            Selection.activeGameObject = template;
            EditorGUIUtility.PingObject(template);
        }
        #endregion

        #region Auto Setup Logic
        /// <summary>
        /// Automatically sets up animation components on a GameObject
        /// </summary>
        public static void SetupAnimationComponents(GameObject target)
        {
            if (target == null) return;

            bool modified = false;

            // Ensure SpriteRenderer exists
            var spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = target.AddComponent<SpriteRenderer>();
                Debug.Log($"Added SpriteRenderer to {target.name}");
                modified = true;
            }

            // Add SpriteAnimationNodes if missing
            var nodes = target.GetComponent<SpriteAnimationNodes>();
            if (nodes == null)
            {
                nodes = target.AddComponent<SpriteAnimationNodes>();
                Debug.Log($"Added SpriteAnimationNodes to {target.name}");
                modified = true;
            }

            // Add SpriteAnimationAttachments if missing
            var attachments = target.GetComponent<SpriteAnimationAttachments>();
            if (attachments == null)
            {
                attachments = target.AddComponent<SpriteAnimationAttachments>();
                Debug.Log($"Added SpriteAnimationAttachments to {target.name}");
                modified = true;
            }

            // Try to add an animator component
            var animator = TryAddAnimatorComponent(target);
            if (animator != null)
            {
                modified = true;
            }

            if (modified)
            {
                EditorUtility.SetDirty(target);
                Debug.Log($"Animation setup completed for {target.name}");
            }
            else
            {
                Debug.Log($"{target.name} already has all required animation components");
            }
        }

        private static Component TryAddAnimatorComponent(GameObject target)
        {
            if (target.GetComponent<PlayableAnimatorComponent>() == null)
            {
                var animator = target.AddComponent<PlayableAnimatorComponent>();
                Debug.Log($"Added PlayableAnimatorComponent to {target.name}");
                return animator;
            }

            return null;
        }
        #endregion

        #region Validation System
        /// <summary>
        /// Validates all animation objects in the scene
        /// </summary>
        public static List<ValidationResult> ValidateAllAnimationObjects()
        {
            var results = new List<ValidationResult>();

            // Find all objects with animation components
            var animators = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(mb => mb is IAnimator)
                .Cast<IAnimator>();

            foreach (var animator in animators)
            {
                var monoBehaviour = animator as MonoBehaviour;
                if (monoBehaviour != null)
                {
                    var result = ValidateAnimationObject(monoBehaviour.gameObject);
                    if (result != null)
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Validates a single animation object
        /// </summary>
        public static ValidationResult ValidateAnimationObject(GameObject obj)
        {
            var issues = new List<string>();
            var warnings = new List<string>();

            // Check for SpriteRenderer
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                issues.Add("Missing SpriteRenderer component");
            }
            else if (spriteRenderer.sprite == null)
            {
                warnings.Add("SpriteRenderer has no sprite assigned");
            }

            // Check for animation nodes
            var nodes = obj.GetComponent<SpriteAnimationNodes>();
            if (nodes == null)
            {
                warnings.Add("Missing SpriteAnimationNodes component (optional but recommended)");
            }

            // Check for attachments
            var attachments = obj.GetComponent<SpriteAnimationAttachments>();
            if (attachments == null)
            {
                warnings.Add("Missing SpriteAnimationAttachments component (optional)");
            }

            // Check animator configuration
            var animator = obj.GetComponent<IAnimator>() as MonoBehaviour;
            if (animator != null)
            {
                var animatorIssues = ValidateAnimatorConfiguration(animator);
                issues.AddRange(animatorIssues);
            }
            else
            {
                issues.Add("No IAnimator implementation found");
            }

            if (issues.Count > 0 || warnings.Count > 0)
            {
                return new ValidationResult
                {
                    ObjectName = obj.name,
                    GameObject = obj,
                    Issues = issues,
                    Warnings = warnings
                };
            }

            return null;
        }

        private static List<string> ValidateAnimatorConfiguration(MonoBehaviour animator)
        {
            var issues = new List<string>();

            if (animator is PlayableAnimatorComponent)
            {
                // PlayableAnimatorComponent is a complex component with multiple setup options
                // For now, just ensure it exists - detailed validation would require reflection
                // or public properties to be added to the component
            }

            return issues;
        }
        #endregion

        #region Template Creation
        /// <summary>
        /// Creates a complete animation object template
        /// </summary>
        public static GameObject CreateAnimationObjectTemplate()
        {
            var template = new GameObject("AnimationObject_Template");
            template.transform.position = Vector3.zero;

            // Add all required components
            var spriteRenderer = template.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            template.AddComponent<SpriteAnimationNodes>();
            template.AddComponent<SpriteAnimationAttachments>();
            var pac = template.AddComponent<PlayableAnimatorComponent>();

            // Create a basic character type if none exists
            var characterType = CreateBasicCharacterType();
            pac.SetCharacterType(characterType);

            Undo.RegisterCreatedObjectUndo(template, "Create Animation Template");
            return template;
        }

        private static CharacterTypeSO CreateBasicCharacterType()
        {
            var characterType = ScriptableObject.CreateInstance<CharacterTypeSO>();
            characterType.characterName = "TemplateCharacter";
            characterType.description = "Auto-generated template character";

            // Create basic idle state
            var idleState = ScriptableObject.CreateInstance<AnimationStateSO>();
            idleState.stateName = "Idle";
            idleState.frameRate = 30f;
            idleState.sprites = new Sprite[1];
            idleState.sprites[0] = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            characterType.idleState = idleState;

            // Save assets
            string folderPath = "Assets/KS Animation Templates";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "KS Animation Templates");
            }

            AssetDatabase.CreateAsset(characterType, $"{folderPath}/TemplateCharacter.asset");
            AssetDatabase.CreateAsset(idleState, $"{folderPath}/IdleState.asset");
            AssetDatabase.SaveAssets();

            return characterType;
        }
        #endregion

        #region Validation Result
        public class ValidationResult
        {
            public string ObjectName;
            public GameObject GameObject;
            public List<string> Issues = new List<string>();
            public List<string> Warnings = new List<string>();
            public bool HasErrors => Issues.Count > 0;
            public bool HasWarnings => Warnings.Count > 0;
        }
        #endregion

        #region UI Helpers
        private static void ShowValidationResults(List<ValidationResult> results)
        {
            if (results.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Complete", "All animation objects are properly configured!", "OK");
                return;
            }

            var message = $"Found {results.Count} animation object(s) with issues:\n\n";

            foreach (var result in results)
            {
                message += $"{result.ObjectName}:\n";

                if (result.HasErrors)
                {
                    message += "  ERRORS:\n";
                    foreach (var issue in result.Issues)
                    {
                        message += $"    • {issue}\n";
                    }
                }

                if (result.HasWarnings)
                {
                    message += "  WARNINGS:\n";
                    foreach (var warning in result.Warnings)
                    {
                        message += $"    • {warning}\n";
                    }
                }

                message += "\n";
            }

            EditorUtility.DisplayDialog("Validation Results", message, "OK");
        }
        #endregion
    }
}
