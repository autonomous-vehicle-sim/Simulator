using UnityEngine;
using System.IO;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
public class MapLoader : MonoBehaviour
{
    public string filePath = "C:\\UnitySimulator\\created_map.png";
    public GameObject vehiclePrefab;
    public Material floorMaterial;
    private GameObject vehicleInstance;
    private Texture2D mapTexture;
    private Rigidbody carRigidBody;
    private int originX;
    private int originY;
    private const int TILE_SIZE = 10;
    private int mapHeight;
    private int mapWidth;


    void Start()
    {
        mapHeight = SpawnVehiclePosition.mapHeight * TILE_SIZE;
        mapWidth = SpawnVehiclePosition.mapWidth * TILE_SIZE;
        Texture2D texture = MapTextureHolder.texture;
        Texture2D mapTexture =ScaleTexture(texture, mapWidth, mapHeight);

        CreatePlaneWithTexture(mapTexture);
        SpawnVehicle();

        originX = 0;
        originY = 0;
        carRigidBody = vehicleInstance.GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        float currentX = carRigidBody.position.x;
        float currentY = carRigidBody.position.y;
        float currentZ = carRigidBody.position.z;
        float XSize = mapWidth / 2;
        float backX = mapWidth;
        float YSize = mapHeight / 2;
        float backY = mapHeight;
        float carSize = 3;
        float doubleCarSize = carSize * 2;

        if (currentX > originX + XSize - carSize)
        {
            carRigidBody.position = new Vector3(currentX - backX + doubleCarSize, currentY, currentZ);
            gameObject.transform.position = new Vector3(currentX - backX + doubleCarSize, currentY, currentZ);
            currentX = carRigidBody.position.x;
        }

        if (currentX < originX - XSize + carSize)
        {
            carRigidBody.position = new Vector3(currentX + backX - doubleCarSize, currentY, currentZ);
            gameObject.transform.position = new Vector3(currentX + backX - doubleCarSize, currentY, currentZ);
            currentX = carRigidBody.position.x;
        }

        if (currentZ > originY + YSize - carSize)
        {
            carRigidBody.position = new Vector3(currentX, currentY, currentZ - backY + doubleCarSize);
            gameObject.transform.position = new Vector3(currentX, currentY, currentZ - backY + doubleCarSize);
        }

        if (currentZ < originY - YSize + carSize)
        {
            carRigidBody.position = new Vector3(currentX, currentY, currentZ + backY - doubleCarSize);
            gameObject.transform.position = new Vector3(currentX, currentY, currentZ + backY - doubleCarSize);
        }
    }
    Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture tmpTexture = RenderTexture.GetTemporary(targetWidth, targetHeight);
        tmpTexture.filterMode = FilterMode.Bilinear;
        RenderTexture.active = tmpTexture;
        Graphics.Blit(source, tmpTexture);
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.ARGB32,false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        RenderTexture.ReleaseTemporary(tmpTexture);
        RenderTexture.active = null;
        EnhanceColors(result, 5);

        return result;
    }
    private void EnhanceColors(Texture2D texture, float enhancementFactor)
    {
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            pixel.r = Mathf.Clamp01(pixel.r * enhancementFactor);
            pixel.g = Mathf.Clamp01(pixel.g * enhancementFactor);
            pixel.b = Mathf.Clamp01(pixel.b * enhancementFactor);
            pixels[i] = pixel;
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }
    void CreatePlaneWithTexture(Texture2D texture)
    {
        GameObject mapObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mapObject.name = "Map";

        Renderer renderer = mapObject.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        float planeWidthScale = mapWidth / TILE_SIZE;
        float planeHeightScale = mapHeight / TILE_SIZE;

        mapObject.transform.localScale = new Vector3(planeWidthScale, 1, planeHeightScale);
        mapObject.transform.position = Vector3.zero;
    }
    float CalculateVehicleXPosition()
    {
        float offsetX = mapWidth / 2 - (TILE_SIZE / 2) - (SpawnVehiclePosition.xPosition * TILE_SIZE);
        return offsetX;
    }
    float CalculateVehicleYPosition()
    {
        float offsetY = -mapHeight / 2 + (TILE_SIZE / 2) + (SpawnVehiclePosition.yPosition * TILE_SIZE);
        return offsetY;
    }
    void SpawnVehicle()
    {
        if (vehiclePrefab != null)
        {
            Vector3 spawnPosition = new Vector3(CalculateVehicleXPosition(), 0.5f, CalculateVehicleYPosition());
            Quaternion spawnRotation = Quaternion.Euler(0,  -SpawnVehiclePosition.rotation, 0);
            vehicleInstance = Instantiate(vehiclePrefab, spawnPosition, spawnRotation);
        }
    }
}
public static class MapTextureHolder
{
    public static Texture2D texture;
}


