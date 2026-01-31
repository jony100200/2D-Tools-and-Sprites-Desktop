// TabContainer.cs
// Reusable tab container component for Unity Editor windows
// Following SOLID principles - Single Responsibility for tab management

using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// A reusable tab container component for Unity Editor windows
    /// Manages multiple tabs with lazy-loaded content
    /// </summary>
    public class TabContainer : VisualElement
    {
        private VisualElement tabBar;
        private VisualElement contentArea;
        private List<Button> tabButtons = new List<Button>();
        private List<VisualElement> tabContents = new List<VisualElement>();
        private int selectedTabIndex = -1;

        public TabContainer()
        {
            // Create tab bar
            tabBar = new VisualElement();
            tabBar.style.flexDirection = FlexDirection.Row;
            tabBar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            tabBar.style.paddingLeft = 4;
            tabBar.style.paddingRight = 4;
            tabBar.style.paddingTop = 4;
            tabBar.style.paddingBottom = 4;
            Add(tabBar);

            // Create content area
            contentArea = new VisualElement();
            contentArea.style.flexGrow = 1;
            contentArea.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            Add(contentArea);
        }

        /// <summary>
        /// Adds a new tab to the container
        /// </summary>
        /// <param name="tabName">Display name for the tab</param>
        /// <param name="contentFactory">Factory function that creates the tab content</param>
        /// <returns>The tab button for further customization</returns>
        public Button AddTab(string tabName, Func<VisualElement> contentFactory)
        {
            // Create tab button
            var tabButton = new Button(() => SelectTab(tabButtons.Count)) { text = tabName };
            tabButton.style.flexGrow = 1;
            tabButton.style.height = 25;
            tabButton.style.marginLeft = 2;
            tabButton.style.marginRight = 2;
            tabButton.style.borderTopLeftRadius = 4;
            tabButton.style.borderTopRightRadius = 4;
            tabButton.style.borderBottomLeftRadius = 0;
            tabButton.style.borderBottomRightRadius = 0;

            tabBar.Add(tabButton);
            tabButtons.Add(tabButton);

            // Create content (but don't add it yet)
            var content = contentFactory();
            tabContents.Add(content);

            Debug.Log($"TabContainer: Added tab '{tabName}' with content: {content != null}, child count: {content?.childCount ?? 0}");

            return tabButton;
        }

        /// <summary>
        /// Selects a tab by index
        /// </summary>
        /// <param name="index">Index of the tab to select</param>
        public void SelectTab(int index)
        {
            if (index < 0 || index >= tabButtons.Count) return;

            Debug.Log($"TabContainer: Selecting tab {index}");

            // Update button styles
            for (int i = 0; i < tabButtons.Count; i++)
            {
                if (i == index)
                {
                    tabButtons[i].style.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 1f);
                    tabButtons[i].style.color = Color.white;
                }
                else
                {
                    tabButtons[i].style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1f);
                    tabButtons[i].style.color = Color.gray;
                }
            }

            // Update content
            contentArea.Clear();
            if (index < tabContents.Count)
            {
                var content = tabContents[index];
                if (content != null)
                {
                    contentArea.Add(content);
                    Debug.Log($"TabContainer: Added content for tab {index}, content has {content.childCount} children");
                }
                else
                {
                    Debug.LogWarning($"TabContainer: Content for tab {index} is null");
                }
            }

            selectedTabIndex = index;
        }

        /// <summary>
        /// Gets the currently selected tab index
        /// </summary>
        public int SelectedTabIndex => selectedTabIndex;

        /// <summary>
        /// Gets the number of tabs
        /// </summary>
        public int TabCount => tabButtons.Count;
    }
}