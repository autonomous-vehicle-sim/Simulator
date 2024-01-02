using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class MapValueDisplay : MonoBehaviour
{
    public GameObject imagePrefab;
    public Image selectedImage;

    private const string IMAGES_DIRECTORY_PATH = "Assets/Resources/PaletteImages/";
    private const string DEFAULT_IMAGE_NAME = "0_empty.png";
    private int GRID_WIDTH = 5;
    private  int GRID_HEIGHT = 5;
    private const float OFFSET = 1.0f;
    private const float CELL_WIDTH = 50.0f;
    private const float CELL_HEIGHT = 50.0f;
    private const float DEFAULT_ROTATE_ANGLE = 0.0f;
    private const bool HEIGHT = true;
    private const bool WIDTH = false;
    private List<List<Tuple<string,float>>> mapGrid = new List<List<Tuple<string,float>>>();
    void Start()
    {
        InicjalizeDefaultMapGrid();
        SizeInputScript.onEndEdit += ModifySizeOfGrid;
        GenerateGrid();
    }
    public void InicjalizeDefaultMapGrid()
    {
        for(int i=0;i<GRID_WIDTH; i++)
        {
            List<Tuple<string, float>> tmp = new List<Tuple<string, float>>();
            for (int j=0;j<GRID_HEIGHT; j++)
            {
                tmp.Add(new Tuple<string,float>(DEFAULT_IMAGE_NAME, DEFAULT_ROTATE_ANGLE));
            }
            mapGrid.Add(tmp);
        }
    }
    public void ModifySizeOfGrid(string input, bool dimensionType)
    {
        int size=Int32.Parse(input);
        DeleyGrid();
        if (dimensionType == HEIGHT)
        {
            if(size > GRID_HEIGHT)
            {
                 AddRows(size - GRID_HEIGHT);
            }
            else
            {
                RemoveRows(GRID_HEIGHT - size);
            }
            GRID_HEIGHT = size;
        }
        else
        {
            if (size > GRID_WIDTH)
            {
                AddColumns(size - GRID_WIDTH);
            }
            else
            {
                RemoveColumns(GRID_WIDTH - size);
            }
            GRID_WIDTH = size;
        }
        GenerateGrid();

    }
    public void DeleyGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void AddRows(int numberOfAddRows)
    {
        for (int i = 0; i < numberOfAddRows; i++)
        {
            List<Tuple<string, float>> tmp = new List<Tuple<string, float>>();
            for (int j = 0; j < GRID_WIDTH; j++)
            {
                tmp.Add(new Tuple<string, float>(DEFAULT_IMAGE_NAME, DEFAULT_ROTATE_ANGLE));
            }
            mapGrid.Add(tmp);
        }
    }
    public void RemoveRows(int numberOfDeleteRows)
    {
        int firstIndex = mapGrid.Count - numberOfDeleteRows;
        mapGrid.RemoveRange(firstIndex,numberOfDeleteRows);
    }
    public void AddColumns(int numberOfAddColumns)
    {

        for (int i = 0; i < GRID_HEIGHT; i++)
        {
            for (int j = 0; j < numberOfAddColumns; j++)
            {
                mapGrid[i].Add(new Tuple<string, float>(DEFAULT_IMAGE_NAME, DEFAULT_ROTATE_ANGLE));
            }
        }
    }
    public void RemoveColumns(int numberOfDeleteColumns)
    {
        for (int i = 0; i < GRID_HEIGHT; i++)
        {
            int firstIndex = mapGrid[i].Count - numberOfDeleteColumns;
            mapGrid[i].RemoveRange(firstIndex, numberOfDeleteColumns);
        }
    }
    public void GenerateGrid()
    {
        int imageIndex = 0;

        string imageName;

        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                float posX = (x * CELL_WIDTH) + (x * OFFSET);
                float posY = (-y * CELL_HEIGHT) - (y * OFFSET);
                imageName = IMAGES_DIRECTORY_PATH + mapGrid[y][x].Item1;

                DisplayPaletteValueImage(posX, posY, imageName, imageIndex);
                imageIndex++;
            }
        }
    }
    void DisplayPaletteValueImage(float posX, float posY, string imagePath,int imageIndex)
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
        int rowPosition = imageIndex / GRID_WIDTH;
        int columnPosition = imageIndex % GRID_WIDTH;
        RotateImage(gameObject, mapGrid[rowPosition][columnPosition].Item2);
    }
    Sprite LoadSprite(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        return sprite;
    }
    void RotateImage(GameObject gameObject,float angle)
    {
        RectTransform imageRectTransform = gameObject.GetComponent<RectTransform>();

        imageRectTransform.Rotate(Vector3.forward, angle);
    }
    void AddButtonOnClickEvent(GameObject gameObject, int imageIndex)
    {
        Button button = gameObject.AddComponent<Button>();
        button.onClick.AddListener(() => OnButtonClick(gameObject, imageIndex));
    }

    void UpdateMapSelectedImage(GameObject gameObject,int imageIndex)
    {
        float rotateAngle= selectedImage.transform.eulerAngles.z;
        Sprite sprite = LoadSprite(selectedImage.name);
        gameObject.GetComponent<Image>().sprite = sprite;
        RotateImage(gameObject,rotateAngle);


        int rowPosition = imageIndex / GRID_WIDTH;
        int columnPosition=imageIndex % GRID_WIDTH;
        string[] imageNameSplit = selectedImage.name.Split('/');
        mapGrid[rowPosition][columnPosition] = new Tuple<string, float>(imageNameSplit[imageNameSplit.Length-1], rotateAngle);
    }

    void OnButtonClick(GameObject gameObject,int imageIndex)
    {
        UpdateMapSelectedImage(gameObject, imageIndex);
    }
}
