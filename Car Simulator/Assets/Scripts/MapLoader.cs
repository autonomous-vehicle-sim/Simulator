using UnityEngine;
using System.IO;

public class MapLoader : MonoBehaviour
{
    public string filePath = "C:/UnitySimulator/created_map.png";
    public GameObject vehiclePrefab;
    public Material floorMaterial;
    private GameObject vehicleInstance;
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

        CreatePlaneWithPng();
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

    void CreatePlaneWithPng()
    {
        GameObject mapObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mapObject.name = "Map";
        Renderer renderer = mapObject.GetComponent<Renderer>();

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        
        if (floorMaterial != null)
        {
            renderer.material = floorMaterial;
        }
        else
        {
            renderer.material = new Material(Shader.Find("Standard"));
        }

        renderer.material.mainTexture = texture;
        renderer.material.color = Color.white;

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


