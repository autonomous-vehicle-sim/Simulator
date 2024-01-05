using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    public static SettingsController instance { get; private set; }

    [HideInInspector] public string windowModeName;

    private const string DEFAULT_WINDOW_MODE = "Windowed Fullscreen";

    private readonly Dictionary<string, FullScreenMode> windowModeNames = new()
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
        if (instance != null && instance != this)
        {
            Destroy(instance);
        }
        else
        {
            instance = this;
        }

        string windowModeName = PlayerPrefs.GetString("windowMode", DEFAULT_WINDOW_MODE);
        SetWindowModeFromName(windowModeName);
    }
}
