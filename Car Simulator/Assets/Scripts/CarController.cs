using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public List<Wheel> wheels;

    [SerializeField] private float _wheelRayLength = 0.7f;
    [SerializeField] private float _suspensionRestDist = 0.7f;
    [SerializeField] private float _springStrength = 50.0f;
    [SerializeField] private float _springDamper = 5.0f;
    [SerializeField] private float _wheelGripFactor = 0.8f;         // [0-1] (0 = no grip, 1 = full grip)
    [SerializeField] private float _wheelMass = 0.05f;
    [SerializeField] private float _frictionStrength = 1.0f;
    [SerializeField] private float _topSpeed = 100.0f;
    [SerializeField] private float _torque = 5.0f;
    [SerializeField] private float _steeringAngle = 40.0f;

    private Rigidbody _carRigidBody;

    // Evaluates how much torque should be applied given current car speed (fraction, from 0 to 1).
    // Returns a number from 0 to 1 - fraction of torque to apply.
    private float EvaluateTorqueCurve(float speedFraction)
    {
        // todo
        return 1 - speedFraction;
    }

    private void Start()
    {
        _carRigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_carRigidBody != null)
        {
            float accelInput = Input.GetAxis("Vertical");
            float steeringInput = Input.GetAxis("Horizontal");

            foreach (Wheel wheel in wheels)
            {
                Transform wheelTransform = wheel.wheelObject.GetComponent<Transform>();

                // Wheel rotation
                if (wheel.steering)
                {
                    wheelTransform.localRotation = Quaternion.Euler(Vector3.up * _steeringAngle * steeringInput);
                }

                bool rayDidHit = Physics.Raycast(wheelTransform.position, wheelTransform.TransformDirection(Vector3.down), out RaycastHit wheelRay, _wheelRayLength);
                Debug.DrawRay(wheelTransform.position, wheelTransform.TransformDirection(Vector3.down) * _wheelRayLength);

                if (rayDidHit)
                {
                    Vector3 wheelWorldVelocity = _carRigidBody.GetPointVelocity(wheelTransform.position);

                    // Suspension force
                    Vector3 springDir = wheelTransform.up;

                    float springOffset = _suspensionRestDist - wheelRay.distance;
                    float springVelocity = Vector3.Dot(springDir, wheelWorldVelocity);
                    float springForce = springOffset * _springStrength - springVelocity * _springDamper;

                    _carRigidBody.AddForceAtPosition(springDir * springForce, wheelTransform.position);
                    Debug.DrawRay(wheelTransform.position, springDir * springForce, Color.cyan);

                    // Steering force
                    Vector3 steeringDir = wheelTransform.right;
                    
                    float steeringVelocity = Vector3.Dot(steeringDir, wheelWorldVelocity);
                    float desiredSteeringDeltaVelocity = -steeringVelocity * _wheelGripFactor;
                    float desiredSteeringAcceleration = desiredSteeringDeltaVelocity / Time.fixedDeltaTime;

                    _carRigidBody.AddForceAtPosition(steeringDir * _wheelMass * desiredSteeringAcceleration, wheelTransform.position);
                    Debug.DrawRay(wheelTransform.position, steeringDir * _wheelMass * desiredSteeringAcceleration, Color.magenta);
                }

                // todo: raycast
                Vector3 accelDir = wheelTransform.forward;
                if (accelInput != 0.0f)
                {
                    // Acceleration/braking
                    if (wheel.motor)
                    {
                        float carForwardSpeed = Vector3.Dot(transform.forward, _carRigidBody.velocity);
                        float speedFraction = Mathf.Clamp01(Mathf.Abs(carForwardSpeed) / _topSpeed);          // [0-1], (0 = no speed, 1 = full speed)
                        float availableTorque = EvaluateTorqueCurve(speedFraction);
                        float currentTorque = availableTorque * _torque * accelInput;

                        _carRigidBody.AddForceAtPosition(accelDir * currentTorque, wheelTransform.position);
                        Debug.DrawRay(wheelTransform.position, accelDir * currentTorque, Color.yellow);
                    }
                }

                // Friction
                // todo: is being applied/calculated incorrectly

                //Vector3 carDir = _carRigidBody.velocity.normalized;
                //float carSpeed = Vector3.Dot(carDir, _carRigidBody.velocity);

                Vector3 carDir = wheelTransform.forward;
                float carSpeed = Vector3.Dot(transform.forward, _carRigidBody.velocity);
                float frictionForce = Mathf.Min(_frictionStrength, Mathf.Abs(carSpeed));
                if (carSpeed < 0.0f)
                {
                    frictionForce = -frictionForce;
                }

                _carRigidBody.AddForceAtPosition(-carDir * frictionForce, wheelTransform.position);
                Debug.DrawRay(wheelTransform.position, -carDir * frictionForce, Color.red);
                Debug.Log(-carDir);
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (Wheel wheel in wheels)
        {
            Transform wheelTransform = wheel.wheelObject.GetComponent<Transform>();

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(wheelTransform.position, 0.03f);

            Vector3 suspensionRestPos = wheelTransform.position + _suspensionRestDist * wheelTransform.TransformDirection(Vector3.down);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(suspensionRestPos, 0.03f);
        }
    }

    [System.Serializable]
    public class Wheel
    {
        public GameObject wheelObject;
        public bool steering;
        public bool motor;
    }
}
