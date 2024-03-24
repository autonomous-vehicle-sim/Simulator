using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TorqueMeter : MonoBehaviour
{
    private TorqueBar _torqueBarObject;
    private TMP_Text _torqueValueObject;

    private void UpdateTorque(float torque)
    {
        _torqueBarObject.SetTorque(Mathf.Abs(torque));
        _torqueValueObject.SetText(Mathf.Round(Mathf.Abs(torque)).ToString());
    }

    private void Start()
    {
        _torqueBarObject = GetComponentInChildren<TorqueBar>();
        _torqueValueObject = GameObject.Find("TorqueValue").GetComponent<TMP_Text>();
        CarController.SpeedChanged += UpdateTorque;
    }
}
