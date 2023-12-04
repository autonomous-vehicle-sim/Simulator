using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SteeringAngleDisplay : MonoBehaviour
{
    private TMP_Text steeringAngleValueObject;
    private string angleSuffix = "°";

    private void UpdateAngle(float angle)
    {
        steeringAngleValueObject.text = Mathf.Round(angle).ToString() + angleSuffix;
    }

    private void Start()
    {
        steeringAngleValueObject = GameObject.Find("SteeringAngleValue").GetComponent<TMP_Text>();
        WheelController.onSteeringChange += UpdateAngle;
    }
}
