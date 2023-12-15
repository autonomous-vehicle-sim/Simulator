using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SizeInputScript : MonoBehaviour
{
    public TMP_InputField inputField;

    void Start()
    {
        inputField.text = "5";
        inputField.onEndEdit.AddListener(OnEndEditListener);
    }

    void OnEndEditListener(string input)
    {
        if (!float.TryParse(input, out float numericValue) || numericValue < 1)
        {
            inputField.text = "1";
        }
        UnityEngine.Debug.Log("Input value of " + inputField.name + ": " + input);
    }
}
