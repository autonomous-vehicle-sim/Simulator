using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionSetting : MonoBehaviour
{
    private TMP_Dropdown _resolutionDropdown;
    private Resolution[] _resolutions;
    private int _currentResolutionIndex;

    public void HandleResolutionChange(int index)
    {
        if (_resolutionDropdown != null)
        {
            string resolutionText = _resolutionDropdown.options[index].text;
            int resWidth = int.Parse(resolutionText.Split(" ")[0]);
            int resHeight = int.Parse(resolutionText.Split(" ")[2]);
            SettingsController.Instance.SetResolution(resWidth, resHeight);
        }
    }

    private void Start()
    {
        _resolutionDropdown = GetComponentInChildren<TMP_Dropdown>();
        _resolutions = Screen.resolutions;
        _currentResolutionIndex = -1;

        int initialWidth = SettingsController.Instance.ResWidth;
        int initialHeight = SettingsController.Instance.ResHeight;

        List<string> options = new();
        for (int i = 0; i < _resolutions.Length; i++)
        {
            Resolution resolution = _resolutions[i];
            if (resolution.width == initialWidth && 
                resolution.height == initialHeight &&
                resolution.refreshRateRatio.numerator == Screen.currentResolution.refreshRateRatio.numerator &&
                resolution.refreshRateRatio.denominator == Screen.currentResolution.refreshRateRatio.denominator)
            {
                _currentResolutionIndex = i;
            }
            options.Add(resolution.ToString());
        }

        if (_currentResolutionIndex == -1)
        {
            _currentResolutionIndex = options.Count;
            options.Add(initialWidth + " x " + initialHeight + " @ " + Screen.currentResolution.refreshRateRatio + "Hz");
        }

        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.SetValueWithoutNotify(_currentResolutionIndex);
    }
}
