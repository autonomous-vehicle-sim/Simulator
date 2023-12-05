using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModeDisplay : MonoBehaviour
{
    private TMP_Text modeTextObject;

    public void SetModeManual()
    {
        modeTextObject.text = "Manual";
    }

    public void SetModeAutonomous()
    {
        modeTextObject.text = "Autonomous";
    }

    private void Start()
    {
        modeTextObject = GameObject.Find("ModeValue").GetComponent<TMP_Text>();
    }
}
