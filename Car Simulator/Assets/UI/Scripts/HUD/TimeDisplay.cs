using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeDisplay : MonoBehaviour
{
    private TMP_Text _timerValueObject;

    public float Time { get; private set; }
    public bool IsRunning { get; private set; }

    public void StartTimer()
    {
        IsRunning = true;
    }

    public void StopTimer()
    {
        IsRunning = false;
    }

    public void ResetTimer()
    {
        Time = 0.0f;
    }

    private void Start()
    {
        Time = 0.0f;
        _timerValueObject = GameObject.Find("TimeValue").GetComponent<TMP_Text>();
        StartTimer();
    }

    private void Update()
    {
        string seconds = (Mathf.Round(Mathf.Floor(Time)) % 60).ToString("00");
        string minutes = Mathf.Round(Mathf.Floor(Time / 60) % 60).ToString("00");
        string hours = Mathf.Round(Mathf.Floor(Time / 3600) % 100).ToString("00");
        _timerValueObject.text = hours + ":" + minutes + ":" + seconds;
        if (IsRunning) Time += UnityEngine.Time.deltaTime;
    }
}
