using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PaletteValuesDisplay : MonoBehaviour
{
    public GameObject imagePrefab;
    public Image selectedImage;

    private const int GRID_WIDTH = 3;
    private const int GRID_HEIGHT = 5;
    private const float OFFSET = 5.0f;
    private const float CELL_WIDTH = 50.0f;
    private const float CELL_HEIGHT = 50.0f;
    private const string IMAGES_DIRECTORY_PATH = "Assets/Resources/PaletteImages/";

    void Start()
    {
        string[] imageFiles = Directory.GetFiles(IMAGES_DIRECTORY_PATH, "*.png");
        Array.Sort(imageFiles);
        string defaultImage = imageFiles[0];
        GenerateGrid(imageFiles);
        UpdateSelectedPaletteValueImage(defaultImage);
    }

    void GenerateGrid(string[] imageFiles)
    {
        int imageIndex = 0;
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                float posX = (x * CELL_WIDTH) + (x * OFFSET);
                float posY = (-y * CELL_HEIGHT) - (y * OFFSET);

                string imagePath = imageFiles[imageIndex];
                imageIndex = (++imageIndex) % imageFiles.Length;
                DisplayPaletteValueImage(posX, posY, imagePath);
            }
        }
    }

    void DisplayPaletteValueImage(float posX, float posY, string imagePath)
    {
        GameObject image = CreateImage(posX, posY);
        image.name = imagePath;
        SetImageOnGameObject(image, imagePath);
        AddButtonOnClickEvent(image);
    }

    GameObject CreateImage(float posX, float posY)
    {
        GameObject image = Instantiate(imagePrefab, transform);
        RectTransform imageRect = image.GetComponent<RectTransform>();
        imageRect.anchoredPosition = new Vector2(posX, posY);
        imageRect.sizeDelta = new Vector2(CELL_WIDTH, CELL_HEIGHT);

        return image;
    }

    void SetImageOnGameObject(GameObject gameObject, string imagePath)
    {
        Sprite sprite = LoadSprite(imagePath);
        gameObject.GetComponent<Image>().sprite = sprite;
    }

    void AddButtonOnClickEvent(GameObject gameObject)
    {
        Button button = gameObject.AddComponent<Button>();
        button.onClick.AddListener(() => OnButtonClick(gameObject.GetComponent<Image>()));
    }

    void UpdateSelectedPaletteValueImage(string imageName)
    {
        Sprite sprite = LoadSprite(imageName);
        selectedImage.name = imageName;
        selectedImage.sprite = sprite;
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
        UpdateSelectedPaletteValueImage(clickedImage.name);
        selectedImage.name = clickedImage.name;
    }
}
