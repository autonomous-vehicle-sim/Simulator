using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    [HideInInspector] public string windowModeName;

    private const string DEFAULT_WINDOW_MODE = "Windowed Fullscreen";

    private Dictionary<string, FullScreenMode> windowModeNames = new Dictionary<string, FullScreenMode>()
    {
        { "Windowed Fullscreen", FullScreenMode.FullScreenWindow },
        { "Fullscreen", FullScreenMode.ExclusiveFullScreen },
        { "Windowed", FullScreenMode.Windowed }
    };

    public void SetWindowModeFromName(string modeName)
    {
        windowModeName = modeName;
        FullScreenMode windowMode = windowModeNames[modeName];
        Screen.fullScreenMode = windowMode;
        PlayerPrefs.SetString("windowMode", modeName);
        Debug.Log("Setting window mode to " + windowMode);
    }

    private void Awake()
    {
        string windowModeName = PlayerPrefs.GetString("windowMode", DEFAULT_WINDOW_MODE);
        SetWindowModeFromName(windowModeName);
    }
}
