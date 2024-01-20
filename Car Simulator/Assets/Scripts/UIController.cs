using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text motorTorqueField;
    [SerializeField] private GameObject car;

    private WheelController wheelController;

    private void Start()
    {
        wheelController = car.GetComponent<WheelController>();
    }

    private void Update()
    {
        motorTorqueField.text = wheelController.currentMotorTorque.ToString("F0");
    }
}
