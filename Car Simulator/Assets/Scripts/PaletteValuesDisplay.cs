using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PaletteValuesDisplay : MonoBehaviour
{

    public GameObject imagePrefab;
    public int gridWidth = 3;
    public int gridHeight = 4;
    public const string IMAGES_DIRECTORY_PATH = "Assets/Resources/PaletteImages/";

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        RectTransform canvasRect = GetComponent<RectTransform>();
        string[] imageFiles = Directory.GetFiles(IMAGES_DIRECTORY_PATH, "*.png");
        
        float cellWidth = 50.0f;
        float cellHeight = 50.0f;
        int imageIndex = 0;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                float xPos = x * cellWidth + cellWidth / 2;
                float yPos = -y * cellHeight - cellHeight / 2;

                GameObject image = Instantiate(imagePrefab, transform);
                RectTransform imageRect = image.GetComponent<RectTransform>();

                imageRect.anchoredPosition = new Vector2(xPos, yPos);
                imageRect.sizeDelta = new Vector2(cellWidth, cellHeight);
                
                string imagePath = imageFiles[imageIndex];
                Sprite sprite = LoadSprite(imagePath);
                image.GetComponent<Image>().sprite = sprite;
                imageIndex = imageIndex++ % imageFiles.Length;
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

}
