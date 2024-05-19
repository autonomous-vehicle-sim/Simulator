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
    [SerializeField] GameObject carPrefab;
    [SerializeField] GameObject mapPrefab;
    [SerializeField] List<List<GameObject>> cars = new List<List<GameObject>>();
    [SerializeField] List<GameObject> maps = new List<GameObject>();
    private int queuedMaps = 0;
    private int queuedCars = 0;
    private ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

    void Start()
    {
        pathTimestamp = DateTime.Now.ToString();
        pathTimestamp = Regex.Replace(pathTimestamp, ":", ".");
        cameraRecorder = new CameraRecorder();
        connection = GetComponent<Connection>();
    }

    void Update()
    {
        if (actionQueue.Count > 0)
        {
            actionQueue.TryDequeue(out Action action);
            action.Invoke();
        }
    }

    private void InitNewCar(int mapId, float topSpeed, float maxSteeringAngle, int posX = 0, int posY = 0)
    {
        GameObject car = Instantiate(carPrefab);
        int instanceId = cars[mapId].Count;
        car.GetComponent<CarController>().SetTopSpeed(topSpeed);
        car.GetComponent<CarController>().SetMaxSteeringAngle(maxSteeringAngle);
        car.GetComponent<CarController>().SetMapInfo(mapId, instanceId);
        var children = car.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Cars");
        }
        cars[mapId].Add(car);
        car.transform.position = new Vector3(mapId * 1000 + posX, 10, 0 + posY);
        car.GetComponent<Rigidbody>().position = new Vector3(mapId * 1000 + posX, 10, 0 + posY);
    }

    private void InitNewMap(int seed = -1)
    {
        if (seed == -1)
            seed = UnityEngine.Random.Range(0, 1000 * 1000);
        int mapId = maps.Count;
        GameObject map = Instantiate(mapPrefab);
        DynamicFloor dynamicFloor = map.GetComponentInChildren<DynamicFloor>();
        dynamicFloor.SetSeed(seed);
        dynamicFloor.Generate();
        maps.Add(map);
        map.transform.position = new Vector3(mapId * 1000, 0, 0);
    }

    public static explicit operator TCPClient(GameObject v)
    {
        throw new NotImplementedException();
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
                InitNewMap(seed);
                cars.Add(new List<GameObject>());
                SendMessageToServer("map " + maps.Count.ToString() + " initialized");
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
            actionQueue.Enqueue(() =>
            {
                int instanceId = cars[mapId].Count;
                float topSpeed = float.Parse(arguments[2]);
                float maxSteeringAngle = float.Parse(arguments[3]);
                int posX = Int32.Parse(arguments[4]);
                int posY = Int32.Parse(arguments[5]);
                if (topSpeed > 0 && maxSteeringAngle > 0)
                {
                    queuedCars++;
                    InitNewCar(mapId, topSpeed, maxSteeringAngle);
                    SendMessageToServer("car " + mapId.ToString() + " " + instanceId.ToString() + " initialized");
                }
                else
                    SendMessageToServer("Invalid init car values provided");
            });
            return;
        }

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
            }
            return;
        }
        if (arguments[2] == "get")
        {
            if (arguments[3] == "steer")
            {
                actionQueue.Enqueue(() =>
                {
                    float steer = cars[mapId][instanceId].GetComponent<CarInputController>().GetSteeringInput() * 100.0f;
                    string message = "steer " + arguments[0] + " " + arguments[1] + " " + arguments[3] + " " + steer.ToString() + " " + DateTime.Now.ToString();
                    if (cars[mapId][instanceId].activeSelf == false)
                        message = arguments[0] + " " + arguments[1] + " " + "deleted";
                    SendMessageToServer(message);
                });
            }
            else if (arguments[3] == "engine")
            {
                actionQueue.Enqueue(() =>
                {
                    float engine = cars[mapId][instanceId].GetComponent<CarInputController>().GetAccelInput() * 100.0f;
                    string message = "engine " + arguments[0] + " " + arguments[1] + " " + arguments[3] + " " + engine.ToString() + " " + DateTime.Now.ToString();
                    if (cars[mapId][instanceId].activeSelf == false)
                        message = arguments[0] + " " + arguments[1] + " " + "deleted";
                    SendMessageToServer(message);
                });
            }
            return;
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