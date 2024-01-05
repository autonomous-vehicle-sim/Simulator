using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    public static SettingsController Instance { get; private set; }

    [HideInInspector] public string WindowModeName;

    private const string DEFAULT_WINDOW_MODE = "Windowed Fullscreen";

    private readonly Dictionary<string, FullScreenMode> _windowModeNames = new()
    {
        { "Windowed Fullscreen", FullScreenMode.FullScreenWindow },
        { "Fullscreen", FullScreenMode.ExclusiveFullScreen },
        { "Windowed", FullScreenMode.Windowed }
    };

    public void SetWindowModeFromName(string modeName)
    {
        WindowModeName = modeName;
        FullScreenMode windowMode = _windowModeNames[modeName];
        Screen.fullScreenMode = windowMode;
        PlayerPrefs.SetString("windowMode", modeName);
        Debug.Log("Setting window mode to " + windowMode);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }

        string windowModeName = PlayerPrefs.GetString("windowMode", DEFAULT_WINDOW_MODE);
        SetWindowModeFromName(windowModeName);
    }
}
