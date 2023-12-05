using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TorqueMeter : MonoBehaviour
{
    private TorqueBar torqueBarObject;
    private TMP_Text torqueValueObject;
    private TMP_Text directionIndicatorObject;

    private void UpdateTorque(float torque)
    {
        torqueBarObject.SetTorque(Mathf.Abs(torque));
        torqueValueObject.SetText(Mathf.Round(Mathf.Abs(torque)).ToString());
        if (torque > 0)
        {
            directionIndicatorObject.text = "F";
        }
        else if (torque < 0)
        {
            directionIndicatorObject.text = "R";
        }
        else
        {
            directionIndicatorObject.text = "";
        }
    }

    private void Start()
    {
        torqueBarObject = GetComponentInChildren<TorqueBar>();
        torqueValueObject = GameObject.Find("TorqueValue").GetComponent<TMP_Text>();
        directionIndicatorObject = GameObject.Find("TorqueDirectionIndicator").GetComponent<TMP_Text>();
        WheelController.onTorqueChange += UpdateTorque;
    }
}
