using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SteeringAngleDisplay : MonoBehaviour
{
    private TMP_Text _steeringAngleValueObject;
    private string _angleSuffix = "°";

    private void UpdateAngle(float angle)
    {
        _steeringAngleValueObject.text = Mathf.Round(angle).ToString() + _angleSuffix;
    }

    private void Start()
    {
        _steeringAngleValueObject = GameObject.Find("SteeringAngleValue").GetComponent<TMP_Text>();
        CarController.SteeringChanged += UpdateAngle;
    }
}
