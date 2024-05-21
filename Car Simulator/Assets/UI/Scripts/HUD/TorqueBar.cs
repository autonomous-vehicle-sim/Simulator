using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TorqueBar : MonoBehaviour
{
    [SerializeField] private float _maxTorque;

    private Image _barImage;
    private float _maxBarFillAmount = 0.75f;

    public void SetTorque(float torque)
    {
        float barFillAmount = (torque / _maxTorque) * _maxBarFillAmount;
        _barImage.fillAmount = barFillAmount;
    }

    private void Start()
    {
        _barImage = GetComponent<Image>();
    }
}
