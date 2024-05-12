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
    private string serverIP = "127.0.0.1"; // Set this to your server's IP address.
    public int serverPort = 1984;             // Set this to your server's port.
    private const int CARS_LAYER = 6;
    private string pathTimestamp;
    [SerializeField] public bool connectToServer;
    private bool connectedToServer = false;
    [SerializeField] GameObject carPrefab;
    [SerializeField] GameObject mapPrefab;
    [SerializeField] List<List<GameObject>> cars = new List<List<GameObject>>();
    [SerializeField] List<GameObject> maps = new List<GameObject>();
    private const int CAMERA_LAYER = 1;
    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;
    private ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();
    private CameraRecorder cameraRecorder;

    void Start()
    {
        pathTimestamp = DateTime.Now.ToString();
        pathTimestamp = Regex.Replace(pathTimestamp, ":", ".");
        cameraRecorder = new CameraRecorder();
    }

    void Update()
    {
        if(!connectedToServer && connectToServer)
        {
            ConnectToServer();
            connectedToServer = true;
        }
        if (actionQueue.Count > 0)
        {
            actionQueue.TryDequeue(out Action action);
            action.Invoke();
        }
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            Debug.Log("Connected to server.");

            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
        }
    }

    private void ListenForData()
    {
        try
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                if (stream.DataAvailable)
                {
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        // Convert byte array to string message.
                        string serverMessage = Encoding.UTF8.GetString(incomingData);
                        HandleServerMessage(serverMessage);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
    private void InitNewCar(int mapId, float topSpeed, float maxSteeringAngle)
    {
        GameObject car = Instantiate(carPrefab);
        int instanceId = cars[mapId].Count;
        car.GetComponent<CarController>().SetTopSpeed(topSpeed);
        car.GetComponent<CarController>().SetMaxSteeringAngle(maxSteeringAngle);
        car.GetComponent<CameraRecorder>().SetPath(pathTimestamp, mapId.ToString(), instanceId.ToString());
        var children = car.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Cars");
        }
        cars[mapId].Add(car);
        car.transform.position = new Vector3(mapId * 1000, 10, 0);
        car.GetComponent<Rigidbody>().position = new Vector3(mapId * 1000, 10, 0);
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
        Debug.Log(1000 * mapId);
    }
    private void GetCamera(int mapId, int instanceId)
    {
        GameObject frameComponent = cars[mapId][instanceId].transform.GetChild(0).gameObject;
        Camera[] cameras = frameComponent.GetComponentsInChildren<Camera>();
        foreach (Camera cam in cameras)
        {
            cam.cullingMask = CAMERA_LAYER;
        }
        Debug.Log(cameraRecorder.ToString());
        Debug.Log(cars[mapId][instanceId].ToString());

        cameraRecorder.SetCar(cars[mapId][instanceId]);
        cameraRecorder.SetCameras(cameras);
        string path = "getCamera/" + mapId.ToString() + "/" + instanceId.ToString();
        cameraRecorder.SavePhoto(path, "test");
    }
    private void HandleServerMessage(string message)
    {
        string[] arguments = message.Split(' ');
        if (arguments.Length == 0)
        {
            Debug.LogError("Received empty message from server");
            return;
        }
        if (arguments[0] == "init_new_map")
        {
            actionQueue.Enqueue(() =>
            {
                int seed = -1;
                if (arguments.Length > 1)
                    seed = Int32.Parse(arguments[1]);
                InitNewMap(seed);
                cars.Add(new List<GameObject>());
            });
            return;
        }

        int mapId = Int32.Parse(arguments[0]);
        Debug.Assert(mapId >= 0);

        if (arguments[1] == "delete")
        {
            actionQueue.Enqueue(() =>
            {
                foreach(GameObject car in cars[mapId]){
                    car.SetActive(false);
                }
                maps[mapId].SetActive(false);
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
                InitNewCar(mapId, topSpeed, maxSteeringAngle);
            });
            return;
        }

        int instanceId = Int32.Parse(arguments[1]);
        Debug.Assert(instanceId >= 0);
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
                });
            }
            else if (arguments[3] == "engine")
            {
                actionQueue.Enqueue(() =>
                {
                    float acceleration = float.Parse(arguments[4]) / 100.0f;
                    CarController carController = cars[mapId][instanceId].GetComponent<CarController>();
                    if (acceleration * 100.0f > carController.GetTopSpeed())
                    {
                        acceleration = carController.GetTopSpeed();
                    }
                    if (acceleration * 100.0f < -carController.GetTopSpeed())
                    {
                        acceleration = -carController.GetTopSpeed();
                    }
                    cars[mapId][instanceId].GetComponent<CarInputController>().SetAccelInput(acceleration);
                });
            }
            return;
        }
        if (arguments[2] == "get")
        {
            if (arguments[3] == "camera")
            {
                int camera_id = Int32.Parse(arguments[4]);
                actionQueue.Enqueue(() =>
                {
                    GetCamera(mapId, instanceId);
                }
                );
            }
            else if (arguments[2] == "get")
            {
                if (arguments[3] == "steer")
                {
                    actionQueue.Enqueue(() =>
                    {
                        float steer = cars[mapId][instanceId].GetComponent<CarInputController>().GetSteeringInput() * 100.0f;
                        string message = arguments[0] + " " + arguments[1] + " " + arguments[3] + " " + steer.ToString() + " " + DateTime.Now.ToString();
                        if (cars[mapId][instanceId].activeSelf == false)
                            message = arguments[0] + " " + arguments[1] + " " + "deleted";
                        SendMessageToServer(message);
                    });
                }
                else if (arguments[3] == "engine")
                {
                    actionQueue.Enqueue(() =>
                    {
                        float steer = cars[mapId][instanceId].GetComponent<CarInputController>().GetSteeringInput() * 100.0f;
                        string message = arguments[0] + " " + arguments[1] + " " + arguments[3] + " " + steer.ToString() + " " + DateTime.Now.ToString();
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
                });
            }
            else
                Debug.LogError("Invalid message sent from server");

        }
    }
    public void SendMessageToServer(string message)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client not connected to server.");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
        Debug.Log("Sent message to server: " + message);
    }

    void OnApplicationQuit()
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
        if (clientReceiveThread != null)
            clientReceiveThread.Abort();
    }
}