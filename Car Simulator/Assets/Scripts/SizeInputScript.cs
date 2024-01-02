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
    public static event Action<string,bool> onEndEdit;

    void Start()
    {
        inputField.text = "5";
        inputField.onEndEdit.AddListener(OnEndEditListener);
    }

    void OnEndEditListener(string input)
    {
        bool dimensionType;
        if (!int.TryParse(input, out int numericValue) || numericValue < 1)
        {
            inputField.text = "1";
            numericValue = 1;
        }
        if(inputField.name=="Width Input")
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
