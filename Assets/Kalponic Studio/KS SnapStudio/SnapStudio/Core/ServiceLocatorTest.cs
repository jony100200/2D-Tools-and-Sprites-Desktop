#if UNITY_EDITOR
using UnityEngine;

namespace KalponicGames.KS_SnapStudio.SnapStudio
{
    public class ServiceLocatorTest : MonoBehaviour
    {
        void Start()
        {
            // Test if ServiceLocator is accessible within the same namespace
            if (ServiceLocator.MainWindow != null)
            {
                Debug.Log("ServiceLocator accessible from same namespace");
            }
            else
            {
                Debug.Log("ServiceLocator not accessible from same namespace");
            }
        }
    }
}
#endif
