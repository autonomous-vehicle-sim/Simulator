using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowModeSetting : MonoBehaviour
{
    [SerializeField] private SettingsController settingsController;

    private TMP_Dropdown windowModeDropdown;

    public void HandleWindowModeChange(int index)
    {
        Debug.Log(index);
        if (windowModeDropdown != null)
        {
            string windowModeName = windowModeDropdown.options[index].text;
            settingsController.SetWindowMode(windowModeName);
        }
    }

    private void Start()
    {
        windowModeDropdown = GetComponentInChildren<TMP_Dropdown>();
        windowModeDropdown.value = windowModeDropdown.options.FindIndex(option => option.text == settingsController.windowModeName);
    }
}
