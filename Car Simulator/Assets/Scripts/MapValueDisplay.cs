using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine.SceneManagement;

public class MapValueDisplay : MonoBehaviour
{
    public GameObject imagePrefab;
    public Image selectedImage;
    public Button saveButton;
    public Button loadButton;
    public Button playButton;


    private const string IMAGES_DIRECTORY_PATH = "Assets/Resources/PaletteImages/";
    private const string DEFAULT_IMAGE_NAME = "0_empty.png";
    private const int DEFAULT_GRID_WIDTH = 5;
    private const int DEFAULT_GRID_HEIGHT = 5;
    private const int MAX_GRID_HEIGHT = 10;
    private const int MAX_GRID_WIDTH = 20;
    private const int CELL_WIDTH = 50;
    private const int CELL_HEIGHT = 50;
    private const float OFFSET = 2.0f;
    private const float DEFAULT_ROTATE_ANGLE = 0.0f;
    private const bool DIMENSION_TYPE_HEIGHT = true;
    private const string DEFAULT_DIALOGUE_DIRECTORY = "C://";
    private const string DEFAULT_SAVE_FILE_NAME = "custom_map.map";
    private const string PREFERRED_EXTENSION = "map";
    private const string CREATED_MAP_PATH = "C:/UnitySimulator/created_map.png";
    private int gridWidth;
    private int gridHeight;
    private (string imageName, float rotationAngle)[,] mapGrid = new(string, float)[MAX_GRID_HEIGHT, MAX_GRID_WIDTH];
    ///TTTTTTTTESSSSSSSSSSSSSSSSSSSSSSSSSTTTTTTTTTTTTTTTTTTTTTT
    //public GameObject receiverObject;
    private MapLoader mapLoader;

    void Start()
    {
        InitializeDefaultMapGrid();
        SizeInputScript.onEndEdit += ModifySizeOfGrid;
        GenerateGrid();
        saveButton.onClick.AddListener(SaveImage);
        loadButton.onClick.AddListener(LoadImage);
        playButton.onClick.AddListener(PlaySimulation);
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
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                writer.Write(gridWidth); // first saves grid size
                writer.Write(gridHeight);

                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, mapGrid);
            }
        }
    }
    public void LoadImage()
    {
        string selectedImagePath = EditorUtility.OpenFilePanel("Load file", DEFAULT_DIALOGUE_DIRECTORY, PREFERRED_EXTENSION);

        using (FileStream fileStream = new FileStream(selectedImagePath, FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                gridWidth = reader.ReadInt32(); //first saved the grid size
                gridHeight = reader.ReadInt32();

                IFormatter formatter = new BinaryFormatter();
                mapGrid = ((string imageName, float rotationAngle)[,])formatter.Deserialize(fileStream);
            }
        }
        DeleteGrid();
        GenerateGrid();
    }
    public void PlaySimulation()
    {
        SaveMergedImage();
        SceneManager.LoadScene("CreatorGameScene");
    }
    void SaveMergedImage()
    {
        int totalWidth = gridWidth * CELL_WIDTH;
        int totalHeight = gridHeight * CELL_HEIGHT;
        Texture2D mergedTexture = new Texture2D(totalWidth, totalHeight);
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                string imagePath = IMAGES_DIRECTORY_PATH + mapGrid[i, j].imageName;
                float imageRotateAngle = mapGrid[i, j].rotationAngle;
                byte[] fileData = System.IO.File.ReadAllBytes(imagePath);
                Texture2D texture = new Texture2D(CELL_WIDTH, CELL_HEIGHT);
                texture.LoadImage(fileData);
                Texture2D scaledTexture = ScaleTexture(texture, CELL_WIDTH, CELL_HEIGHT);
                Texture2D rotatedTexture = RotateTexture(scaledTexture, imageRotateAngle);

                Color[] pixels = rotatedTexture.GetPixels();
                mergedTexture.SetPixels(j * CELL_WIDTH, (gridHeight - i - 1) * CELL_HEIGHT, CELL_WIDTH, CELL_HEIGHT, pixels);
            }
        }

        mergedTexture.Apply();
        byte[] pngBytes = mergedTexture.EncodeToPNG();
        File.WriteAllBytes(CREATED_MAP_PATH, pngBytes);
        TextureHolder.texture = mergedTexture;

        //TESTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT
        /*Scene otherScene = SceneManager.GetSceneByName("CreatorGameScene");
        GameObject[] rootObjects = otherScene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            MapLoader otherClass = obj.GetComponent<MapLoader>();
            if (otherClass != null)
            {
                otherClass.SetTexture(mergedTexture); 
                break; // Znaleziono, nie trzeba szukaæ dalej
            }
        
        }*/
        //mapLoader = FindObjectOfType<MapLoader>();

       // mapLoader.SetTexture(mergedTexture);
    }
    Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    { 
        RenderTexture tmpTexture = RenderTexture.GetTemporary(targetWidth, targetHeight);
        RenderTexture.active = tmpTexture;
        Graphics.Blit(source, tmpTexture);
        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        RenderTexture.ReleaseTemporary(tmpTexture);
        return result;
    }
    Texture2D RotateTexture(Texture2D texture, float angle)
    {
        int width = texture.width;
        int height = texture.height;
        Texture2D rotatedTexture = new Texture2D(width, height);
        Color[] originalPixels = texture.GetPixels();
        Color[] rotatedPixels = new Color[originalPixels.Length];

        float angleRad = -angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        float centerX = width / 2f;
        float centerY = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float newX = cos * (x - centerX) - sin * (y - centerY) + centerX;
                float newY = sin * (x - centerX) + cos * (y - centerY) + centerY;

                if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                {
                    rotatedPixels[y * width + x] = GetBilinearPixel(originalPixels, width, height, newX, newY);
                }
                else
                {
                    rotatedPixels[y * width + x] = new Color(0, 0, 0, 0);
                }
            }
        }

        rotatedTexture.SetPixels(rotatedPixels);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    Color GetBilinearPixel(Color[] pixels, int width, int height, float positionX, float positionY)
    {
        int x = Mathf.FloorToInt(positionX);
        int y = Mathf.FloorToInt(positionY);

        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (x >= width - 1) x = width - 2;
        if (y >= height - 1) y = height - 2;

        float positionXRatio = positionX - x;
        float positionYRatio = positionY - y;
        float xOpposite = 1 - positionXRatio;
        float yOpposite = 1 - positionYRatio;

        Color result = (pixels[(y * width) + x] * xOpposite + pixels[(y * width) + x + 1] * positionXRatio) * yOpposite +
                       (pixels[((y + 1) * width) + x] * xOpposite + pixels[((y + 1) * width) + x + 1] * positionXRatio) * positionYRatio;
        return result;
    }
}