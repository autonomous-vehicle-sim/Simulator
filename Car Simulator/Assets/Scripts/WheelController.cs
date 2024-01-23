using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public List<AxleInfo> AxleInfos;        // information about each individual axle
    public float MaxMotorTorque;
    public float MaxSteeringAngle;
    public float CurrentMotorTorque { get; private set; }
    public float CurrentSteeringAngle { get; private set; }

    public delegate void TorqueChangedEventHandler(float torque);
    public static event TorqueChangedEventHandler TorqueChanged;
    public delegate void SteeringChangedEventHandler(float steeringAngle);
    public static event SteeringChangedEventHandler SteeringChanged;

    // Transforms the given wheel collider based on simulated position
    public void ApplyLocalPositionToMesh(WheelCollider collider)
    {
        if (collider.transform.childCount == 0) return;

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        Transform wheelMesh = collider.transform.GetChild(0);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = rotation;
    }

    private void FixedUpdate()
    {
        CurrentMotorTorque = MaxMotorTorque * Input.GetAxis("Vertical");
        CurrentSteeringAngle = MaxSteeringAngle * Input.GetAxis("Horizontal");

        TorqueChanged?.Invoke(CurrentMotorTorque);
        SteeringChanged?.Invoke(CurrentSteeringAngle);

        foreach (AxleInfo axleInfo in AxleInfos)
        {
            if (axleInfo.Motor)
            {
                axleInfo.LeftWheel.motorTorque = CurrentMotorTorque;
                axleInfo.RightWheel.motorTorque = CurrentMotorTorque;
            }
            if (axleInfo.Steering)
            {
                axleInfo.LeftWheel.steerAngle = CurrentSteeringAngle;
                axleInfo.RightWheel.steerAngle = CurrentSteeringAngle;
            }

            ApplyLocalPositionToMesh(axleInfo.LeftWheel);
            ApplyLocalPositionToMesh(axleInfo.RightWheel);
        }
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider LeftWheel;
    public WheelCollider RightWheel;
    public bool Motor;          // is this wheel attached to the motor?
    public bool Steering;       // does this wheel apply steer angle?
}