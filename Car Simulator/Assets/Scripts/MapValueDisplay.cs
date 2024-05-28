using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine.SceneManagement;
using SFB;


public class MapValueDisplay : MonoBehaviour
{
    public GameObject imagePrefab;
    public Image selectedImage;
    public Button saveButton;
    public Button loadButton;
    public Button playButton;
    public Button selectPositionButton;
    public Button carOrientationButton;
    public TMPro.TextMeshProUGUI informationText;

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
    private const string DEFAULT_DIALOGUE_DIRECTORY = "C:/UnitySimulator/";
    private const string DEFAULT_SAVE_FILE_NAME = "custom_map.map";
    private const string PREFERRED_EXTENSION = "map";
    private const string CREATED_MAP_PATH = "C:/UnitySimulator/created_map.png";
    private int gridWidth;
    private int gridHeight;
    private bool isSelectPositionMode = false;
    private (string imageName, float rotationAngle)[,] mapGrid = new(string, float)[MAX_GRID_HEIGHT, MAX_GRID_WIDTH];
    private GameObject vehicleStartTile = null;
    private GameObject selectedTile = null;
    private Color sideColor = Color.red;
    private Color selectPositionModeColor = Color.grey;


    void Start()
    {
        InitializeDefaultMapGrid();
        SizeInputScript.onEndEdit += ModifySizeOfGrid;
        GenerateGrid();
        saveButton.onClick.AddListener(SaveMap);
        loadButton.onClick.AddListener(LoadMap);
        playButton.onClick.AddListener(PlaySimulation);
        selectPositionButton.onClick.AddListener(SelectPositionMode);
        carOrientationButton.onClick.AddListener(SelectCarOrientation);

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
        SpawnVehiclePosition.ChangeMapSize(gridWidth, gridHeight);

        HighlightVehicleStartTile();
    }

    void DeleteGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
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
        image.name = (imageIndex + imagePath);
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
        if(isSelectPositionMode)
        {
            vehicleStartTile = gameObject;
            SpawnVehiclePosition.SetCarPositionAndMapSize(imageIndex % gridWidth, imageIndex / gridWidth, gridWidth, gridHeight);

            if (selectedTile != gameObject)
            {
                if (selectedTile != null)
                {
                    RemoveTileHighlight(selectedTile);
                }
                selectedTile = gameObject;
                ChangeTileSideColor(gameObject, imageIndex);
            }
        }
        else
        {
            UpdateMapSelectedImage(gameObject, imageIndex);
        }
    }

    public void SaveMap()
    {
        try
        {
            string saveFilePath = ShowSaveFileDialog("Save file", DEFAULT_DIALOGUE_DIRECTORY, DEFAULT_SAVE_FILE_NAME, PREFERRED_EXTENSION);

            if (!Directory.Exists(DEFAULT_DIALOGUE_DIRECTORY))
            {
                Directory.CreateDirectory(DEFAULT_DIALOGUE_DIRECTORY);
            }
            using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    SaveGridSizeAndSpawnPosition(writer);
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fileStream, mapGrid);
                }
            }
            DisplayInformationText("Map saved succesfull!", Color.green);
        }
        catch(System.Exception ex) 
        {
            Debug.Log(ex.Message);
            DisplayInformationText("Map saved failed!", Color.red);
        }
    }

    string ShowSaveFileDialog(string title, string initialDirectory, string defaultFileName, string filter)
    {
        var extensions = new[] {
            new ExtensionFilter("Files", filter)
        };
        string path = StandaloneFileBrowser.SaveFilePanel(title, initialDirectory, defaultFileName, extensions);
        return path;
    }


    private void HideInformationText()
    {
        informationText.gameObject.SetActive(false);
    }

    private void SaveGridSizeAndSpawnPosition(BinaryWriter writer)
    {
        writer.Write(gridWidth);
        writer.Write(gridHeight);
        writer.Write(SpawnVehiclePosition.yPosition);
        writer.Write(SpawnVehiclePosition.xPosition);
        writer.Write(SpawnVehiclePosition.rotation);
    }

    public void LoadMap()
    {
        try
        {
            string selectedImagePath = ShowOpenFileDialog("Load file", DEFAULT_DIALOGUE_DIRECTORY, PREFERRED_EXTENSION);

            using (FileStream fileStream = new FileStream(selectedImagePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    LoadGridSizeAndSpawnPosition(reader);
                    IFormatter formatter = new BinaryFormatter();
                    mapGrid = ((string imageName, float rotationAngle)[,])formatter.Deserialize(fileStream);
                }
            }
            DeleteGrid();
            ChangeCreatorDataAfterLoadMap();
            GenerateGrid();
            HighlightVehicleStartTile();
            DisplayInformationText("Map load succesfull!!", Color.green);

        }
        catch (Exception ex) 
        {
            Debug.Log(ex.Message);
            DisplayInformationText("Map load failed!", Color.red);
        }
    }

    string ShowOpenFileDialog(string title, string initialDirectory, string filter)
    {
        var extensions = new[] {
            new ExtensionFilter("Files", filter)
        };
        string[] paths = StandaloneFileBrowser.OpenFilePanel(title, initialDirectory, extensions, false);
        return paths.Length > 0 ? paths[0] : null;
    }

    private void DisplayInformationText(string text, Color color)
    {
        informationText.gameObject.SetActive(true);
        informationText.text = text;
        informationText.color = color;
        Invoke("HideInformationText", 3f);
    }

    private void LoadGridSizeAndSpawnPosition(BinaryReader reader)
    {
        gridWidth = reader.ReadInt32();
        gridHeight = reader.ReadInt32();
        SpawnVehiclePosition.yPosition = reader.ReadInt32();
        SpawnVehiclePosition.xPosition = reader.ReadInt32();
        SpawnVehiclePosition.rotation = reader.ReadInt32();
    }

    public void ChangeCreatorDataAfterLoadMap()
    {
        GameObject inputFieldObject = GameObject.Find("Width Input");
        TMP_InputField inputField = inputFieldObject.GetComponent<TMP_InputField>();
        inputField.text = gridWidth.ToString();
        inputFieldObject = GameObject.Find("Height Input");
        inputField = inputFieldObject.GetComponent<TMP_InputField>();
        inputField.text = gridHeight.ToString();

        Image buttonImage = carOrientationButton.GetComponentInChildren<Image>();
        buttonImage.rectTransform.localEulerAngles = new Vector3(0, 0, SpawnVehiclePosition.rotation);

        SpawnVehiclePosition.ChangeMapSize(gridWidth, gridHeight);
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
        MapTextureHolder.texture = mergedTexture;
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

    void SelectPositionMode()
    {
        isSelectPositionMode = !isSelectPositionMode;
        UpdateSelectPositionButtonColor();
    }

    void UpdateSelectPositionButtonColor()
    {
        ColorBlock colorBlock = selectPositionButton.colors;
        if (isSelectPositionMode)
        {
            colorBlock.normalColor = selectPositionModeColor;
            colorBlock.selectedColor = selectPositionModeColor;
        }
        else
        {
            colorBlock.normalColor = Color.white;
            colorBlock.selectedColor = Color.white;
        }
        selectPositionButton.colors = colorBlock;
    }

    void RemoveTileHighlight(GameObject gameObject)
    {
        var outline = gameObject.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }

        ResetTileSideColor(gameObject);
    }

    void HighlightVehicleStartTile()
    {
        int yPos = SpawnVehiclePosition.yPosition;
        int xPos = SpawnVehiclePosition.xPosition;
        int imageIndex = yPos * gridWidth + xPos;
        GameObject mapPallet = GameObject.Find("Map Values");

        if (imageIndex > mapPallet.transform.childCount)
        {
            SpawnVehiclePosition.SetCarPosition(0, 0);
            imageIndex = 0;
            Debug.Log("HighlightVehicleStartTile: Index out of range.");

        }
        GameObject gameObject = mapPallet.transform.GetChild(imageIndex).gameObject;
        vehicleStartTile = gameObject;

        if (selectedTile != null)
        {
            RemoveTileHighlight(selectedTile);
        }

        selectedTile = gameObject;
        ChangeTileSideColor(selectedTile, imageIndex);
    }

    void ChangeTileSideColor(GameObject gameObject, int imageIndex)
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        float angle = rectTransform.eulerAngles.z;
        AddSideBorder(gameObject, RotatePosition(new Vector2(0, height / 2), angle), (new Vector2(width, 2)));
        AddSideBorder(gameObject, RotatePosition(new Vector2(width / 2, 0), angle), (new Vector2(2, height)));
        AddSideBorder(gameObject, RotatePosition(new Vector2(0, -height / 2), angle), (new Vector2(width, 2)));
        AddSideBorder(gameObject, RotatePosition(new Vector2(-width / 2, 0), angle), new Vector2(2, height));
    }

    Vector2 RotatePosition(Vector2 position, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        float newX = cos * position.x - sin * position.y;
        float newY = sin * position.x + cos * position.y;

        return new Vector2(newX, newY);
    }

    void AddSideBorder(GameObject gameObject, Vector2 position, Vector2 size)
     {
        GameObject frame = new GameObject("Frame");
        frame.transform.SetParent(gameObject.transform);
        RectTransform rectTransform = frame.AddComponent<RectTransform>();

        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Image borderImage = frame.AddComponent<Image>();
        borderImage.color = sideColor;
     }

    void ResetTileSideColor(GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.name == "Frame")
            {
                Destroy(child.gameObject);
            }
        }
    }

    void SelectCarOrientation()
    {
        SpawnVehiclePosition.RotateTheCar();

        Image buttonImage = carOrientationButton.GetComponentInChildren<Image>();
        buttonImage.rectTransform.localEulerAngles = new Vector3(0, 0, SpawnVehiclePosition.rotation);
    }
}

public static class SpawnVehiclePosition
{
    public static int xPosition;
    public static int yPosition;
    public static int rotation;
    public static int mapWidth;
    public static int mapHeight;
    private const int ROTATION_ANGLE = 15;

    static SpawnVehiclePosition()
    {
        xPosition = 0;
        yPosition=0;
        rotation = -180;
        mapWidth = 5;
        mapHeight = 5;
    }

    public static void SetCarPosition(int x, int y)
    {
        xPosition = x;
        yPosition = y;
    }

    public static void SetCarPositionAndMapSize(int x, int y, int gridWidth, int gridHeight)
    {
        xPosition = x;
        yPosition=y;
        mapWidth=gridWidth;
        mapHeight=gridHeight;
    }

    public static void SetCarRotation(int angle)
    {
        rotation=angle;
    }

    public static void RotateTheCar()
    {
        rotation -= ROTATION_ANGLE;
        rotation %= 360;
    }

    public static void ChangeMapSize(int gridWidth, int gridHeight)
    {
        mapWidth = gridWidth;
        mapHeight = gridHeight;
    }

    public static void Print()
    {
        Debug.Log($"Position X {xPosition}, Position Y {yPosition}, Rotation {rotation}, Map Width {mapWidth} Map Height {mapHeight}");
    }
}