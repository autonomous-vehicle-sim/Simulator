using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModeDisplay : MonoBehaviour
{
    private TMP_Text _modeTextObject;

    public void SetModeManual()
    {
        _modeTextObject.text = "Manual";
    }

    public void SetModeAutonomous()
    {
        _modeTextObject.text = "Autonomous";
    }

    private void Start()
    {
        _modeTextObject = GameObject.Find("ModeValue").GetComponent<TMP_Text>();
    }
}
