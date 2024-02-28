using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SavingScript : MonoBehaviour
{

    private const string SAVE_BUTTON_NAME = "Save Button";

    private const string DEFAULT_BROWSING_DIRECTORY = "C://";
    private const string DEFAULT_NAME = "custom_map.map";
    private const string EXTENSION = "map";

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }


    public void OnButtonClick()
    {
        if (button.name == SAVE_BUTTON_NAME)
        {
            saveFile();
        }
        else
        {
            loadFile();
        }
    }

    public void saveFile()
    {
        string title = "Save map";
        string path = EditorUtility.SaveFilePanel(title, DEFAULT_BROWSING_DIRECTORY, DEFAULT_NAME, EXTENSION);

        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("Selected Path: " + path);
        }
    }

    public void loadFile()
    {
        string title = "Load map";
        string path = EditorUtility.OpenFilePanel(title, DEFAULT_BROWSING_DIRECTORY, EXTENSION);

        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("Selected Path: " + path);
        }
    }

}
