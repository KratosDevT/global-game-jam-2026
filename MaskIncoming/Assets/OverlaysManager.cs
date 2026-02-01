using UnityEditor;
using UnityEngine;

public class OverlaysManager : MonoBehaviour
{
    public void Exit()
    {

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;  // Editor: ferma Play Mode
#else
            Application.Quit();  // Build: chiude app
#endif

    }
}
