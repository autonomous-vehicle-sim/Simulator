using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TorqueBar : MonoBehaviour
{
    [SerializeField] private float _maxTorque;

    private Image _BarImage;
    private float _maxBarFillAmount = 0.75f;

    public void SetTorque(float torque)
    {
        float barFillAmount = (torque / _maxTorque) * _maxBarFillAmount;
        _BarImage.fillAmount = barFillAmount;
    }

    private void Start()
    {
        _BarImage = GetComponent<Image>();
    }
}
