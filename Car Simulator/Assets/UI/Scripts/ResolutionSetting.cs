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
            
        }
    }

    private void Start()
    {
        _resolutionDropdown = GetComponentInChildren<TMP_Dropdown>();
        _resolutions = Screen.resolutions;

        List<string> options = new();
        for (int i = 0; i < _resolutions.Length; i++)
        {
            Resolution resolution = _resolutions[i];
            if (resolution.width == Screen.currentResolution.width &&
                resolution.height == Screen.currentResolution.height &&
                resolution.refreshRateRatio.numerator == Screen.currentResolution.refreshRateRatio.numerator &&
                resolution.refreshRateRatio.denominator == Screen.currentResolution.refreshRateRatio.denominator)
            {
                _currentResolutionIndex = i;
            }
            options.Add(resolution.ToString());
        }
        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.SetValueWithoutNotify(_currentResolutionIndex);
    }
}
