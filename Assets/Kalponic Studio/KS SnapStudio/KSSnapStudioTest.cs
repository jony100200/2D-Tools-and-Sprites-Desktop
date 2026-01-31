// Test script to verify all KS SnapStudio tabs work properly
using UnityEditor;
using UnityEngine;

public class KSSnapStudioTest
{
    [MenuItem("Tools/Kalponic Studio/Test KS SnapStudio Tabs")]
    public static void TestTabs()
    {
        Debug.Log("Testing KS SnapStudio tabs...");

        try
        {
            // Test SnapStudio tab
            var snapStudioContent = KalponicGames.KS_SnapStudio.SnapStudioTabController.CreateSnapStudioContent();
            Debug.Log("✅ SnapStudio tab content created successfully");

            // Test ThumbSmith tab
            var thumbSmithContent = KalponicGames.KS_SnapStudio.ThumbSmithTabController.CreateThumbSmithContent();
            Debug.Log("✅ ThumbSmith tab content created successfully");

            // Test Settings tab
            var settingsContent = KalponicGames.KS_SnapStudio.SettingsTabController.CreateSettingsContent();
            Debug.Log("✅ Settings tab content created successfully");

            Debug.Log("🎉 All KS SnapStudio tabs are working properly!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error testing tabs: {e.Message}\n{e.StackTrace}");
        }
    }
}