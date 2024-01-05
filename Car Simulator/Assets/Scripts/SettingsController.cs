using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    public static SettingsController Instance { get; private set; }

    [HideInInspector] public string WindowModeName;
    [HideInInspector] public int ResWidth;
    [HideInInspector] public int ResHeight;

    private const string DEFAULT_WINDOW_MODE = "Windowed";

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

    public void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreenMode);
        PlayerPrefs.SetInt("resWidth", width);
        PlayerPrefs.SetInt("resHeight", height);
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

        string initialWindowModeName = PlayerPrefs.GetString("windowMode", DEFAULT_WINDOW_MODE);
        SetWindowModeFromName(initialWindowModeName);

        // todo: read resolution from PlayerPrefs
    }
}
