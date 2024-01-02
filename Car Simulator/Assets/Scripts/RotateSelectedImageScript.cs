using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateScript : MonoBehaviour
{
    public Image selectedImage;
    int i = 0;
    void Start()
    {
        Button button = GetComponent<Button>();
        i++;
        button.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        UnityEngine.Debug.Log("Rotating: " + selectedImage.name);
        RectTransform imageRectTransform = selectedImage.GetComponent<RectTransform>();

        imageRectTransform.Rotate(Vector3.forward, 90.0f);
    }
}
