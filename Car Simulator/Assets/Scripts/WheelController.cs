using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;        // information about each individual axle
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float currentMotorTorque { get; private set; }
    public float currentSteeringAngle { get; private set; }

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
        currentMotorTorque = maxMotorTorque * Input.GetAxis("Vertical");
        currentSteeringAngle = maxSteeringAngle * Input.GetAxis("Horizontal");

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = currentMotorTorque;
                axleInfo.rightWheel.motorTorque = currentMotorTorque;
            }
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = currentSteeringAngle;
                axleInfo.rightWheel.steerAngle = currentSteeringAngle;
            }

            ApplyLocalPositionToMesh(axleInfo.leftWheel);
            ApplyLocalPositionToMesh(axleInfo.rightWheel);
        }
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;          // is this wheel attached to the motor?
    public bool steering;       // does this wheel apply steer angle?
}