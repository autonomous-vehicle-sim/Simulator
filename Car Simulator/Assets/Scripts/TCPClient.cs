using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TCPClient : MonoBehaviour
{
    Connection connection;
    private string pathTimestamp;
    [SerializeField] public bool connectToServer;
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private GameObject etiPrefab;
    [SerializeField] private List<List<GameObject>> cars = new List<List<GameObject>>();
    [SerializeField] private List<GameObject> maps = new List<GameObject>();
    private List<bool> reportedStateOfMaps = new List<bool>();
    private int queuedMaps = 0;
    private int queuedCars = 0;
    private const int DISTANCE_BETWEEN_MAPS = 5000;
    private ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();
    private ScreenshotScript screenshot;

    void Start()
    {
        screenshot = gameObject.AddComponent<ScreenshotScript>();
        pathTimestamp = DateTime.Now.ToString();
        pathTimestamp = Regex.Replace(pathTimestamp, ":", ".");
        connection = GetComponent<Connection>();
    }

    void Update()
    {
        for(int i = 0; i < reportedStateOfMaps.Count; i++)
        {
            if (!reportedStateOfMaps[i] && (maps[i].GetComponentInChildren<DynamicFloor>() == null || maps[i].GetComponentInChildren<DynamicFloor>().isMapReady))
            {
                reportedStateOfMaps[i] = true;
                screenshot.TakeScreenshot(maps[i].GetComponentInChildren<Camera>());
                SendMessageToServer("map;" + i.ToString() + ";finished initialization;" + screenshot.latestScreenshotPath);
            }
        }
        if (actionQueue.Count > 0)
        {
            actionQueue.TryDequeue(out Action action);
            action.Invoke();
        }
    }

    private void InitNewCar(int mapId, float topSpeed, float maxSteeringAngle, int posX = 0, int posZ = 0)
    {
        GameObject car = Instantiate(carPrefab);
        posX = Mathf.Clamp(posX, -500, 500);
        posZ = Mathf.Clamp(posZ, -500, 500);
        int instanceId = cars[mapId].Count;
        car.GetComponent<CarController>().SetTopSpeed(topSpeed);
        car.GetComponent<CarController>().SetMaxSteeringAngle(maxSteeringAngle);
        int offsetX = mapId * DISTANCE_BETWEEN_MAPS + posX;
        int offsetZ = posZ;
        int offsetY = 10;
        car.GetComponent<CarController>().SetMapInfo(mapId, instanceId, mapId * DISTANCE_BETWEEN_MAPS, 0);
        var children = car.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Cars");
        }
        cars[mapId].Add(car);
        if(maps[mapId].GetComponentInChildren<DynamicFloor>() == null)
        {
            offsetX = mapId * DISTANCE_BETWEEN_MAPS - 111;
            offsetY = 80;
            offsetZ = 40;
            
        }
        car.transform.position = new Vector3(offsetX, offsetY, offsetZ);
        car.GetComponent<Rigidbody>().position = new Vector3(offsetX, offsetY, offsetZ);
    }

    private void InitNewMap(int seed = -1)
    {
        if (seed == -1)
            seed = UnityEngine.Random.Range(0, 1000 * 1000);
        int mapId = maps.Count;
        GameObject map;
        if (seed == -2)
        {
            map = Instantiate(etiPrefab);
        }
        else
        {
            map = Instantiate(mapPrefab);
            DynamicFloor dynamicFloor = map.GetComponentInChildren<DynamicFloor>();
            dynamicFloor.SetSeed(seed); 
            dynamicFloor.Generate();
        }
        maps.Add(map);
        reportedStateOfMaps.Add(false);
        map.transform.position = new Vector3(mapId * DISTANCE_BETWEEN_MAPS, 0, 0);
    }

    public void HandleServerMessage(string message)
    {
        Debug.Log("Received " + message);  
        string[] arguments = message.Split(' ');
        if (arguments.Length == 0)
        {
            Debug.Log("Received empty message from server");
            return;
        }
        if (arguments[0] == "Connected")
        {
            Debug.Log("Connected, nice");
            return;
        }
        if (arguments[0] == "init_new_map")
        {
            queuedMaps++;
            actionQueue.Enqueue(() =>
            {
                int seed = -1;
                if (arguments.Length > 1)
                    seed = Int32.Parse(arguments[1]);
                int mapId = maps.Count;
                InitNewMap(seed);
                cars.Add(new List<GameObject>());
                SendMessageToServer("map " + mapId.ToString() + " started initialization");
            });
            return;
        }

        int mapId = Int32.Parse(arguments[0]);
        if(mapId >= queuedMaps)
        {
            actionQueue.Enqueue(() =>
            {
                SendMessageToServer("Invalid map id provided");
            });
            return;
        }
        Debug.Assert(mapId >= 0);

        if (arguments[1] == "delete")
        {
            actionQueue.Enqueue(() =>
            {
                foreach(GameObject car in cars[mapId]){
                    car.SetActive(false);
                }
                maps[mapId].SetActive(false);
                SendMessageToServer("map " + mapId.ToString() + " deleted");
            });
            return;
        }
        if (arguments[1] == "init_new")
        {
            float topSpeed = float.Parse(arguments[2]);
            float maxSteeringAngle = float.Parse(arguments[3]);
            if (topSpeed <= 0 || maxSteeringAngle <= 0)
            {
                SendMessageToServer("Invalid init car values provided");
                return;
            }
            queuedCars++;
            actionQueue.Enqueue(() =>
            {
                int instanceId = cars[mapId].Count;
                int posX = Int32.Parse(arguments[4]);
                int posY = Int32.Parse(arguments[5]);
                InitNewCar(mapId, topSpeed, maxSteeringAngle);
                SendMessageToServer("car " + mapId.ToString() + " " + instanceId.ToString() + " initialized");
                    
            });
            return;
        }
        //todo: add checks to validate
        int instanceId = Int32.Parse(arguments[1]);
        Debug.Assert(instanceId >= 0);
        if (instanceId >= queuedCars)
        {
            actionQueue.Enqueue(() =>
            {
                SendMessageToServer("Invalid car id provided");
            });
            return;
        }
        if (arguments[2] == "set")
        {
            if (arguments[3] == "steer")
            {
                actionQueue.Enqueue(() =>
                {
                    float steer = float.Parse(arguments[4]) / 100.0f;
                    CarController carController = cars[mapId][instanceId].GetComponent<CarController>();
                    if (steer * 100.0 > carController.GetMaxSteeringAngle())
                    {
                        steer = carController.GetMaxSteeringAngle();
                    }
                    if (steer * 100.0 < -carController.GetMaxSteeringAngle())
                    {
                        steer = -carController.GetMaxSteeringAngle();
                    }
                    cars[mapId][instanceId].GetComponent<CarInputController>().SetSteeringInput(steer);
                    SendMessageToServer("car " + mapId.ToString() + " " + instanceId.ToString() + " steer set to " + steer.ToString());
                });
                return;
            }
            else if (arguments[3] == "engine")
            {
                actionQueue.Enqueue(() =>
                {
                    float acceleration = float.Parse(arguments[4]) / 100.0f;
                    Debug.Log(acceleration);
                    CarController carController = cars[mapId][instanceId].GetComponent<CarController>();
                    if (acceleration * 100.0f > carController.GetTopSpeed())
                    {
                        acceleration = carController.GetTopSpeed();
                        Debug.Log("engine value too high. Cropped to " + acceleration.ToString());
                    }
                    if (acceleration * 100.0f < -carController.GetTopSpeed())
                    {
                        acceleration = -carController.GetTopSpeed();
                        Debug.Log("engine value too low. Cropped to " + acceleration.ToString());
                    }
                    cars[mapId][instanceId].GetComponent<CarInputController>().SetAccelInput(acceleration);
                    Debug.Log("car " + instanceId.ToString() + " set to engine " + acceleration.ToString());
                    SendMessageToServer("car " + instanceId.ToString() + " engine set to " + acceleration.ToString());
                });
                return;
            }
        }
        else if (arguments[2] == "delete")
        {
            actionQueue.Enqueue(() =>
            {
                cars[mapId][instanceId].SetActive(false);
                SendMessageToServer("car " + mapId.ToString() + " " + instanceId.ToString() + " deleted");
            });
        }
        else
            Debug.LogError("Invalid message sent from server");
    }
    public void SendMessageToServer(string message)
    {
        connection.SendWebSocketMessage(message);
    }
}
