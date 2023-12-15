using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateScript : MonoBehaviour
{
    public Image selectedImage;

    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        UnityEngine.Debug.Log("Rotating: " + selectedImage.name);
    }
}
