using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowModeSetting : MonoBehaviour
{
    private TMP_Dropdown windowModeDropdown;

    public void HandleWindowModeChange(int index)
    {
        if (windowModeDropdown != null)
        {
            string windowModeName = windowModeDropdown.options[index].text;
            SettingsController.Instance.SetWindowModeFromName(windowModeName);
        }
    }

    private void Start()
    {
        windowModeDropdown = GetComponentInChildren<TMP_Dropdown>();
        int optionIndex = windowModeDropdown.options.FindIndex(option => option.text == SettingsController.Instance.WindowModeName);
        windowModeDropdown.SetValueWithoutNotify(optionIndex);
    }
}
