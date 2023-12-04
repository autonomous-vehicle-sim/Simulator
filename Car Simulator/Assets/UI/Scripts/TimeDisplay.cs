using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeDisplay : MonoBehaviour
{
    private TMP_Text timerValueObject;

    public float time { get; private set; }
    public bool isRunning { get; private set; }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        time = 0.0f;
    }

    private void Start()
    {
        time = 0.0f;
        timerValueObject = GameObject.Find("TimeValue").GetComponent<TMP_Text>();
        StartTimer();
    }

    private void Update()
    {
        string seconds = (Mathf.Round(Mathf.Floor(time)) % 60).ToString("00");
        string minutes = Mathf.Round(Mathf.Floor(time / 60) % 60).ToString("00");
        string hours = Mathf.Round(Mathf.Floor(time / 3600) % 100).ToString("00");
        timerValueObject.text = hours + ":" + minutes + ":" + seconds;
        if (isRunning) time += Time.deltaTime;
    }
}
