using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class MapValueDisplay : MonoBehaviour
{
    public GameObject imagePrefab;
    public Image selectedImage;
    public Button saveButton;
    public Button loadButton;

    private const string IMAGES_DIRECTORY_PATH = "Assets/Resources/PaletteImages/";
    private const string DEFAULT_IMAGE_NAME = "0_empty.png";
    private const int DEFAULT_GRID_WIDTH = 5;
    private const int DEFAULT_GRID_HEIGHT = 5;
    private const int MAX_GRID_HEIGHT = 10;
    private const int MAX_GRID_WIDTH = 20;
    private const float OFFSET = 1.0f;
    private const float CELL_WIDTH = 50.0f;
    private const float CELL_HEIGHT = 50.0f;
    private const float DEFAULT_ROTATE_ANGLE = 0.0f;
    private const bool DIMENSION_TYPE_HEIGHT = true;
    private const string DEFAULT_DIALOGUE_DIRECTORY = "C://";
    private const string DEFAULT_SAVE_FILE_NAME = "custom_map.map";
    private const string PREFERRED_EXTENSION = "map";
    private int gridWidth;
    private int gridHeight;
    private (string imageName, float rotationAngle)[,] mapGrid = new(string, float)[MAX_GRID_HEIGHT, MAX_GRID_WIDTH];

    void Start()
    {
        InitializeDefaultMapGrid();
        SizeInputScript.onEndEdit += ModifySizeOfGrid;
        GenerateGrid();
        saveButton.onClick.AddListener(SaveImage);
        loadButton.onClick.AddListener(LoadImage);
    }

    void InitializeDefaultMapGrid()
    {
        gridWidth = DEFAULT_GRID_WIDTH;
        gridHeight = DEFAULT_GRID_HEIGHT;
        for (int i = 0; i < MAX_GRID_HEIGHT; i++)
        {
            for (int j = 0; j < MAX_GRID_WIDTH; j++)
            {
                mapGrid[i, j] = (DEFAULT_IMAGE_NAME, DEFAULT_ROTATE_ANGLE);
            }
        }
    }

    void ModifySizeOfGrid(string dimensionSize, bool dimensionType)
    {
        int size = Int32.Parse(dimensionSize);
        DeleteGrid();
        if (dimensionType == DIMENSION_TYPE_HEIGHT)
        {
            if(size < gridHeight)
            {
                RemoveRows(size);
            }
            gridHeight = size;
        }
        else
        {
            if (size < gridWidth)
            {
                RemoveColumns(size);
            }
            gridWidth = size;
        }
        GenerateGrid();
    }

    void DeleteGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void RemoveRows(int newSize)
    {
        for (int i = newSize; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                mapGrid[i, j] = (DEFAULT_IMAGE_NAME, DEFAULT_ROTATE_ANGLE);
            }
        }
    }

    void RemoveColumns(int newSize)
    {
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = newSize; j < gridWidth; j++)
            {
                mapGrid[i, j] = (DEFAULT_IMAGE_NAME, DEFAULT_ROTATE_ANGLE);
            }
        }
    }

    void GenerateGrid()
    {
        int imageIndex = 0;
        string imageName;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                float posX = (x * CELL_WIDTH) + (x * OFFSET);
                float posY = (-y * CELL_HEIGHT) - (y * OFFSET);
                imageName = IMAGES_DIRECTORY_PATH + mapGrid[y, x].imageName;

                DisplayPaletteValueImage(posX, posY, imageName, imageIndex);
                imageIndex++;
            }
        }
    }

    void DisplayPaletteValueImage(float posX, float posY, string imagePath, int imageIndex)
    {
        GameObject image = CreateImage(posX, posY);
        image.name = imagePath;
        SetImageOnGameObject(image, imagePath, imageIndex);
        AddButtonOnClickEvent(image, imageIndex);
    }

    GameObject CreateImage(float posX, float posY)
    {
        GameObject image = Instantiate(imagePrefab, transform);
        RectTransform imageRect = image.GetComponent<RectTransform>();
        imageRect.anchoredPosition = new Vector2(posX, posY);
        imageRect.sizeDelta = new Vector2(CELL_WIDTH, CELL_HEIGHT);

        return image;
    }

    void SetImageOnGameObject(GameObject gameObject, string imagePath, int imageIndex)
    {
        Sprite sprite = LoadSprite(imagePath);
        gameObject.GetComponent<Image>().sprite = sprite;
        int rowPosition = imageIndex / gridWidth;
        int columnPosition = imageIndex % gridWidth;
        RotateImage(gameObject, mapGrid[rowPosition, columnPosition].rotationAngle);
    }

    Sprite LoadSprite(string path)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        return sprite;
    }

    void RotateImage(GameObject gameObject, float angle, float previousAngle = 0)
    {
        RectTransform imageRectTransform = gameObject.GetComponent<RectTransform>();
        imageRectTransform.Rotate(Vector3.back, previousAngle);
        imageRectTransform.Rotate(Vector3.forward, angle);
    }

    void AddButtonOnClickEvent(GameObject gameObject, int imageIndex)
    {
        Button button = gameObject.AddComponent<Button>();
        button.onClick.AddListener(() => OnButtonClick(gameObject, imageIndex));
    }

    void UpdateMapSelectedImage(GameObject gameObject, int imageIndex)
    {
        int rowPosition = imageIndex / gridWidth;
        int columnPosition = imageIndex % gridWidth;
        string[] imageNameSplit = selectedImage.name.Split('/');
        float rotationAngle = selectedImage.transform.eulerAngles.z;

        Sprite sprite = LoadSprite(selectedImage.name);
        gameObject.GetComponent<Image>().sprite = sprite;
        RotateImage(gameObject, rotationAngle, mapGrid[rowPosition, columnPosition].rotationAngle);
        mapGrid[rowPosition, columnPosition] = (imageNameSplit[imageNameSplit.Length - 1], rotationAngle);
    }

    void OnButtonClick(GameObject gameObject, int imageIndex)
    {
        UpdateMapSelectedImage(gameObject, imageIndex);
    }

    public void SaveImage()
    {
        string saveFilePath = EditorUtility.SaveFilePanel("Save file", DEFAULT_DIALOGUE_DIRECTORY, DEFAULT_SAVE_FILE_NAME, PREFERRED_EXTENSION);

        using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create))
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, mapGrid);
        }
    }

    public void LoadImage()
    {
        string selectedImagePath = EditorUtility.OpenFilePanel("Load file", DEFAULT_DIALOGUE_DIRECTORY, PREFERRED_EXTENSION);

        using (FileStream fileStream = new FileStream(selectedImagePath, FileMode.Open))
        {
            IFormatter formatter = new BinaryFormatter();
            mapGrid = ((string imageName, float rotationAngle)[,])formatter.Deserialize(fileStream);
        }

        DeleteGrid();
        GenerateGrid();
    }

}