using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SizeInputScript : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMPro.TextMeshProUGUI errorText;

    private const bool HEIGHT = true;
    private const bool WIDTH = false;
    private const int MAX_HEIGHT = 10;
    private const int MAX_WIDTH = 20;
    private const int MIN_SIZE = 1;
    private const string DEFAULT_SIZE = "5";
    private const string MIN_SIZE_TEXT = "1";

    public static event Action<string, bool> onEndEdit;

    void Start()
    {
        inputField.text = DEFAULT_SIZE;
        inputField.onEndEdit.AddListener(OnEndEditListener);
    }

    void OnEndEditListener(string input)
    {
        bool dimensionType;
        if (!int.TryParse(input, out int numericValue) || numericValue < MIN_SIZE)
        {
            inputField.text = MIN_SIZE_TEXT;
            numericValue = MIN_SIZE;
        }
        if(inputField.name == "Width Input")
        {
            dimensionType = WIDTH;
            if(numericValue > MAX_WIDTH)
            {
                inputField.text = MAX_WIDTH.ToString();
                numericValue = MAX_WIDTH;

                errorText.gameObject.SetActive(true);
                errorText.text = "MAX WIDTH: " + MAX_WIDTH;
                errorText.color = Color.red;
            }
        }
        else
        {
            dimensionType = HEIGHT;
            if (numericValue > MAX_HEIGHT)
            {
                inputField.text = MAX_HEIGHT.ToString();
                numericValue = MAX_HEIGHT;

                errorText.gameObject.SetActive(true);
                errorText.text = "MAX HEIGHT: " + MAX_HEIGHT;
                errorText.color = Color.red;
            }
        }
        onEndEdit?.Invoke(numericValue.ToString(), dimensionType);
    }
}