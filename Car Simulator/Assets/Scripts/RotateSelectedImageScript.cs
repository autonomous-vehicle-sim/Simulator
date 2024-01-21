using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateScript : MonoBehaviour
{
    public Image selectedImage;

    private const float ROTATION_ANGLE = 90.0f;

    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        RectTransform imageRectTransform = selectedImage.GetComponent<RectTransform>();
        imageRectTransform.Rotate(Vector3.forward, ROTATION_ANGLE);
    }
}