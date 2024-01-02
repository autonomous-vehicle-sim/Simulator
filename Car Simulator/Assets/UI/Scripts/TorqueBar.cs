using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TorqueBar : MonoBehaviour
{
    [SerializeField] private float maxTorque;

    private Image BarImage;
    private float maxBarFillAmount = 0.75f;

    public void SetTorque(float torque)
    {
        float barFillAmount = (torque / maxTorque) * maxBarFillAmount;
        BarImage.fillAmount = barFillAmount;
    }

    private void Start()
    {
        BarImage = GetComponent<Image>();
    }
}
