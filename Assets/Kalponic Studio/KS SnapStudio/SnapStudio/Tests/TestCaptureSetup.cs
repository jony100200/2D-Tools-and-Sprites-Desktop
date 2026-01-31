using UnityEngine;
using KalponicGames.KS_SnapStudio;

public class TestCaptureSetup : MonoBehaviour
{
    void Start()
    {
        // Find the character object
        GameObject character = GameObject.Find("CharacterTest");

        if (character != null)
        {
            // Add the KSPlayModeCapture component
            KSPlayModeCapture capture = character.AddComponent<KSPlayModeCapture>();

            // Configure basic settings
            capture.CharacterObject = character;
            capture.OutRoot = "KS_SnapStudio_Test";
            capture.Width = 1024;
            capture.Height = 1024;
            capture.Fps = 24;
            capture.MaxFrames = 24;
            capture.AutoCaptureOnPlay = false; // Don't auto-start for testing
            capture.ShowPreviewWindow = true;

            Debug.Log("KSPlayModeCapture component added to character");
        }
        else
        {
            Debug.LogError("CharacterTest object not found");
        }
    }
}