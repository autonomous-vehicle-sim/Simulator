using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Splines;

public class PaletteValuesDisplay : MonoBehaviour
{
    public GameObject imagePrefab;
    public Image selectedImage;

    private const int GRID_WIDTH = 3;
    private const int GRID_HEIGHT = 5;
    private const float OFFSET = 7.0f;
    private const float CELL_WIDTH = 60.0f;
    private const float CELL_HEIGHT = 60.0f;
    private const string IMAGES_DIRECTORY_PATH = "PaletteImages/";

    void Start()
    {
        Texture2D [] textures = Resources.LoadAll<Texture2D>(IMAGES_DIRECTORY_PATH);
        Sprite[] sprites = CreateSpriteArrayFromTextureArray(textures);

        Array.Sort(sprites, (s1, s2) => s1.name.CompareTo(s2.name));
        Sprite defaultImage = sprites[0];

        GenerateGrid(sprites);
        UpdateSelectedPaletteValueImage(defaultImage);
    }

    Sprite[] CreateSpriteArrayFromTextureArray(Texture2D[] textures)
    {
        Sprite[] sprites = new Sprite[textures.Length];

        for (int i = 0; i < textures.Length; i++)
        {
            sprites[i] = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.zero);
            sprites[i].name = textures[i].name;
        }
        return sprites;
    }
    void GenerateGrid(Sprite[] sprites)

    {
        int spriteIndex = 0;
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                float posX = (x * CELL_WIDTH) + (x * OFFSET);
                float posY = (-y * CELL_HEIGHT) - (y * OFFSET);

                Sprite sprite = sprites[spriteIndex];
                spriteIndex = (++spriteIndex) % sprites.Length;
                DisplayPaletteValueImage(posX, posY, sprite);
            }
        }
    }

    void DisplayPaletteValueImage(float posX, float posY, Sprite sprite)
    {
        GameObject image = CreateImage(posX, posY);
        image.name = sprite.name;
        SetImageOnGameObject(image, sprite);
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

    void SetImageOnGameObject(GameObject gameObject, Sprite sprite)
    {
        gameObject.GetComponent<Image>().sprite = sprite;
    }

    void AddButtonOnClickEvent(GameObject gameObject)
    {
        Button button = gameObject.AddComponent<Button>();
        button.onClick.AddListener(() => OnButtonClick(gameObject.GetComponent<Image>().sprite));
    }

    void UpdateSelectedPaletteValueImage(Sprite sprite)
    {
        selectedImage.name = sprite.name;
        selectedImage.sprite = sprite;
    }

    void OnButtonClick(Sprite clickedSprite)
    {
        RectTransform imageRectTransform = selectedImage.GetComponent<RectTransform>();
        imageRectTransform.rotation = Quaternion.identity;

        UpdateSelectedPaletteValueImage(clickedSprite);
    }
}
