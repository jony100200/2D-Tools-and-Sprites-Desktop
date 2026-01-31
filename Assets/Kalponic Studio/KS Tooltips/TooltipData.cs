using UnityEngine;

namespace KalponicStudio.Tooltips
{
    /// <summary>
    /// ScriptableObject containing tooltip content and styling configuration.
    /// Game-agnostic data container that can be used for any type of tooltip content.
    /// </summary>
    [CreateAssetMenu(fileName = "TooltipData", menuName = "Kalponic Studio/Tooltips/Tooltip Data")]
    public class TooltipData : ScriptableObject, ITooltipData
    {
        [Header("Content")]
        [Tooltip("Main title text for the tooltip")]
        [SerializeField] private string title;

        [Tooltip("Detailed description text")]
        [TextArea(3, 10)]
        [SerializeField] private string description;

        [Header("Styling")]
        [Tooltip("Maximum width of the tooltip in pixels")]
        [SerializeField] private float maxWidth = 300f;

        [Tooltip("Background color tint")]
        [SerializeField] private Color backgroundTint = Color.white;

        [Tooltip("Title text color")]
        [SerializeField] private Color titleColor = Color.white;

        [Tooltip("Description text color")]
        [SerializeField] private Color descriptionColor = new Color(0.8f, 0.8f, 0.8f);

        // Properties for clean access
        public string Title => title;
        public string Description => description;
        public float MaxWidth => maxWidth;
        public Color BackgroundTint => backgroundTint;
        public Color TitleColor => titleColor;
        public Color DescriptionColor => descriptionColor;

        /// <summary>
        /// Checks if this tooltip has any content to display.
        /// </summary>
        public bool HasContent => !string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(description);

        /// <summary>
        /// Creates a runtime tooltip data instance.
        /// Useful for dynamic tooltips.
        /// </summary>
        public static TooltipData CreateRuntime(string title, string description = "",
            float maxWidth = 300f, Color? backgroundTint = null, Color? titleColor = null, Color? descriptionColor = null)
        {
            var data = CreateInstance<TooltipData>();
            data.title = title;
            data.description = description;
            data.maxWidth = maxWidth;
            data.backgroundTint = backgroundTint ?? Color.white;
            data.titleColor = titleColor ?? Color.white;
            data.descriptionColor = descriptionColor ?? new Color(0.8f, 0.8f, 0.8f);
            return data;
        }
    }
}
