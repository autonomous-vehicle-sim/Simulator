using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PaletteValuesDisplay : MonoBehaviour
{

    public GameObject imagePrefab;
    private int GRID_WIDTH = 3;
    private int GRID_HEIGHT = 5;
    private const string IMAGES_DIRECTORY_PATH = "Assets/Resources/PaletteImages/";
    private const float CELL_WIDTH = 50.0f;
    private const float CELL_HEIGHT = 50.0f;
    private const float OFFSET = 5.0f;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        RectTransform canvasRect = GetComponent<RectTransform>();
        string[] imageFiles = Directory.GetFiles(IMAGES_DIRECTORY_PATH, "*.png");
        Array.Sort(imageFiles);
        int imageIndex = 0;

        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                float xPos = (x * CELL_WIDTH) + (x * OFFSET);
                float yPos = (-y * CELL_HEIGHT) - (y * OFFSET);

                GameObject image = Instantiate(imagePrefab, transform);
                RectTransform imageRect = image.GetComponent<RectTransform>();
                imageRect.anchoredPosition = new Vector2(xPos, yPos);
                imageRect.sizeDelta = new Vector2(CELL_WIDTH, CELL_HEIGHT);

                string imagePath = imageFiles[imageIndex];
                Sprite sprite = LoadSprite(imagePath);

                sprite.name = imagePath;
                image.GetComponent<Image>().sprite = sprite;
                imageIndex = (++imageIndex) % imageFiles.Length;

                Button button = image.AddComponent<Button>();
                button.onClick.AddListener(() => OnButtonClick(image.GetComponent<Image>()));
            }
        }
    }

    Sprite LoadSprite(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        return sprite;
    }


    void OnButtonClick(Image clickedImage)
    {
        UnityEngine.Debug.Log("Clicked: " + clickedImage.sprite.name);
    }
}
