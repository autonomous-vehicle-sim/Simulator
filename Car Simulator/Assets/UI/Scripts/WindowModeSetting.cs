using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowModeSetting : MonoBehaviour
{
    private TMP_Dropdown _windowModeDropdown;

    public void HandleWindowModeChange(int index)
    {
        if (_windowModeDropdown != null)
        {
            string windowModeName = _windowModeDropdown.options[index].text;
            SettingsController.Instance.SetWindowModeFromName(windowModeName);
        }
    }

    private void Start()
    {
        _windowModeDropdown = GetComponentInChildren<TMP_Dropdown>();
        int optionIndex = _windowModeDropdown.options.FindIndex(option => option.text == SettingsController.Instance.WindowModeName);
        _windowModeDropdown.SetValueWithoutNotify(optionIndex);
    }
}
