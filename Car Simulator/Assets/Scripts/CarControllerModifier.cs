using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerModifier : MonoBehaviour
{
    public bool steeringControlled = true;
    private float modifiedAccelInput = 0f;
    private float modifiedSteeringInput = 0f;
    public float changedFrequency = 1.0f;
    private float timeSinceLastChanged = 0f; //variable for simulation 

    void Start()
    {

    }

    void Update()
    {
        if (steeringControlled)
        {
            timeSinceLastChanged += Time.deltaTime; 
            Debug.Log(modifiedAccelInput);
            Debug.Log(modifiedSteeringInput);

            if (timeSinceLastChanged > changedFrequency)
            {
                timeSinceLastChanged = 0f;
                float accel = Random.Range(-1f, 1f);
                float angle = Random.Range(-1f, 1f);
                SetInputValues(accel, angle); //sets value from server  
            }
        }

    }
    public void SetSteeringControlled(bool steeringControlled)
    {
        this.steeringControlled = steeringControlled;
    }

    public bool IsSteeringControlled()
    {
        return steeringControlled;
    }

    public void SetInputValues(float accel, float steering)
    {
        modifiedAccelInput = accel;
        modifiedSteeringInput = steering;
    }

    public float GetAccelInput()
    {
        return modifiedAccelInput;
    }

    public float GetSteeringInput()
    {
        return modifiedSteeringInput;
    }
    
}
