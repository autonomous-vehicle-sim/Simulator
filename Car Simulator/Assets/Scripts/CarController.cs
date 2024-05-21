using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public List<Wheel> wheels;
    public float MaxSteeringAngle = 40.0f;
    public float TopSpeed = 60.0f;
    public float CurrentSpeed { get; private set; }
    public float CurrentSteeringAngle { get; private set; }

    public delegate void SpeedChangedEventHandler(float speed);
    public static event SpeedChangedEventHandler SpeedChanged;
    public delegate void SteeringChangedEventHandler(float steeringAngle);
    public static event SteeringChangedEventHandler SteeringChanged;
    private CarInputController inputModifier;

    [SerializeField] private float _wheelRayLength = 1.0f;
    [SerializeField] private float _suspensionRestDist = 0.7f;
    [SerializeField] private float _springStrength = 90.0f;
    [SerializeField] private float _springDamper = 5.5f;
    [SerializeField] private float _wheelGripFactor = 0.7f;         // [0-1] (0 = no grip, 1 = full grip)
    [SerializeField] private float _wheelMass = 0.05f;
    [SerializeField] private float _frictionStrength = 10.0f;
    [SerializeField] private float _torque = 45.0f;
    [SerializeField] private AnimationCurve _torqueCurve = new AnimationCurve();
    [SerializeField] private AnimationCurve _frictionCurve = new AnimationCurve();
    [SerializeField] private bool _drawDebugRays = false;

    public int mapId { get; private set; }
    public int carId { get; private set; }
    private int _originX;
    private int _originY;

    private Rigidbody _carRigidBody;

    public void SetMaxSteeringAngle(float steeringAngle)
    {
        MaxSteeringAngle = steeringAngle;
    }
    public float GetMaxSteeringAngle()
    {
        return MaxSteeringAngle; 
    }
    public void SetTopSpeed(float topSpeed)
    {
        TopSpeed = topSpeed;
    }
    public float GetTopSpeed()
    {
        return TopSpeed;
    }

    public void SetMapInfo(int mapId, int carId, int originX, int originY)
    {
        this.mapId = mapId;
        this.carId = carId;
        _originX = originX;
        _originY = originY;
    }


    // Evaluates how much torque should be applied given current car speed (fraction, from 0 to 1).
    // Returns a number from 0 to 1 - fraction of torque to apply.
    private float EvaluateTorqueCurve(float speedFraction)
    {
        return _torqueCurve.Evaluate(speedFraction);
    }

    // Evaluates how much rolling friction should be applied given current car speed (fraction, from 0 to 1).
    // Returns a number from 0 to 1 - fraction of rolling friction to apply.
    private float EvaluateFrictionCurve(float speedFraction)
    {
        return _frictionCurve.Evaluate(speedFraction);
    }

    private void Start()
    {
        _carRigidBody = GetComponent<Rigidbody>();
        inputModifier = GetComponent<CarInputController>();
    }


    private void FixedUpdate()
    {
        if (_carRigidBody != null)
        {
            float accelInput, steeringInput;
            accelInput = inputModifier.GetAccelInput();
            steeringInput = inputModifier.GetSteeringInput();

            float currentX = _carRigidBody.position.x;
            float currentY = _carRigidBody.position.y;
            float currentZ = _carRigidBody.position.z;

            if (currentX > _originX + 1000 )
            {
                _carRigidBody.position = new Vector3(currentX - 1750, currentY, currentZ);
                gameObject.transform.position = new Vector3(currentX - 1750, currentY, currentZ);
                currentX = _carRigidBody.position.x;
            }

            if (currentX < _originX - 1000)
            {
                _carRigidBody.position = new Vector3(currentX + 1750, currentY, currentZ);
                gameObject.transform.position = new Vector3(currentX + 1750, currentY, currentZ);
                currentX = _carRigidBody.position.x;
            }

            if (currentZ > _originY + 1000)
            {
                _carRigidBody.position = new Vector3(currentX, currentY, currentZ - 1750);
                gameObject.transform.position = new Vector3(currentX, currentY, currentZ - 1750);
            }

            if (currentZ < _originY - 1000)
            {
                _carRigidBody.position = new Vector3(currentX, currentY, currentZ + 1750);
                gameObject.transform.position = new Vector3(currentX, currentY, currentZ + 1750);
            }

            Debug.Assert(wheels.Count > 0);
            foreach (Wheel wheel in wheels)
            {
                Transform wheelTransform = wheel.wheelObject.GetComponent<Transform>();

                // Wheel rotation
                if (wheel.steering)
                {
                    wheelTransform.localRotation = Quaternion.Euler(Vector3.up * MaxSteeringAngle * steeringInput);
                }

                bool rayDidHit = Physics.Raycast(wheelTransform.position, wheelTransform.TransformDirection(Vector3.down), out RaycastHit wheelRay, _wheelRayLength);
                if (rayDidHit)
                {
                    Vector3 wheelWorldVelocity = _carRigidBody.GetPointVelocity(wheelTransform.position);

                    // Suspension force
                    Vector3 springDir = wheelTransform.up;
                    float springOffset = _suspensionRestDist - wheelRay.distance;
                    float springVelocity = Vector3.Dot(springDir, wheelWorldVelocity);
                    float springForce = springOffset * _springStrength - springVelocity * _springDamper;
                    _carRigidBody.AddForceAtPosition(springDir * springForce, wheelTransform.position);

                    // Steering force
                    Vector3 steeringDir = wheelTransform.right;
                    float steeringVelocity = Vector3.Dot(steeringDir, wheelWorldVelocity);
                    float desiredSteeringDeltaVelocity = -steeringVelocity * _wheelGripFactor;
                    float desiredSteeringAcceleration = desiredSteeringDeltaVelocity / Time.fixedDeltaTime;
                    _carRigidBody.AddForceAtPosition(steeringDir * _wheelMass * desiredSteeringAcceleration, wheelTransform.position);
                    //Debug.Log(accelInput);
                    Vector3 accelDir = wheelTransform.forward;
                    float currentTorque = 0.0f;
                    float carForwardSpeed = Vector3.Dot(transform.forward, _carRigidBody.velocity);
                    float speedFraction = Mathf.Clamp01(Mathf.Abs(carForwardSpeed) / TopSpeed);          // [0-1], (0 = no speed, 1 = full speed)
                    if (accelInput != 0.0f)
                    {
                        //Debug.Log(accelInput);
                        // Acceleration/braking
                        if (wheel.motor)
                        {
                            float availableTorque = EvaluateTorqueCurve(speedFraction);
                            currentTorque = availableTorque * _torque * accelInput;
                            
                            _carRigidBody.AddForceAtPosition(accelDir * currentTorque, wheelTransform.position);
                        }
                    }

                    // Friction
                    float carSpeed = Vector3.Dot(transform.forward, _carRigidBody.velocity);
                    float frictionForce = Mathf.Min(_frictionStrength * EvaluateFrictionCurve(speedFraction), Mathf.Abs(carSpeed));
                    if (carSpeed < 0.0f)
                    {
                        frictionForce = -frictionForce;
                    }
                    _carRigidBody.AddForceAtPosition(-accelDir * frictionForce, wheelTransform.position);

                    if (_drawDebugRays)
                    {
                        Debug.DrawRay(wheelTransform.position, wheelTransform.TransformDirection(Vector3.down) * _wheelRayLength);
                        Debug.DrawRay(wheelTransform.position, springDir * springForce, Color.cyan);
                        Debug.DrawRay(wheelTransform.position, steeringDir * _wheelMass * desiredSteeringAcceleration, Color.magenta);
                        Debug.DrawRay(wheelTransform.position, -accelDir * frictionForce, Color.red);
                        Debug.DrawRay(wheelTransform.position, accelDir * currentTorque, Color.yellow);
                    }
                }
            }

            CurrentSpeed = _carRigidBody.velocity.magnitude;
            CurrentSteeringAngle = MaxSteeringAngle * steeringInput;

            SpeedChanged?.Invoke(CurrentSpeed);
            SteeringChanged?.Invoke(CurrentSteeringAngle);
        }
    }

    private void OnDrawGizmos()
    {
        if (_drawDebugRays)
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
    }

    [System.Serializable]
    public class Wheel
    {
        public GameObject wheelObject;
        public bool steering;
        public bool motor;
    }
}