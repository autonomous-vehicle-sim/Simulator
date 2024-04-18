using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using static UnityEditor.Experimental.GraphView.GraphView;
using JetBrains.Annotations;
using System.Text.RegularExpressions;

public class TCPClient : MonoBehaviour
{
    private string serverIP = "127.0.0.1"; // Set this to your server's IP address.
    private int serverPort = 1984;             // Set this to your server's port.
    private const int CARS_LAYER = 6;
    private String pathTimestamp;
    [SerializeField] public bool connectToServer;
    private bool connectedToServer = false;
    [SerializeField] GameObject carPrefab;
    [SerializeField] List<GameObject> cars = new List<GameObject>();
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
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        SerializedProperty layerSP = layers.GetArrayElementAtIndex(CARS_LAYER);
        layerSP.stringValue = "cars";
        Physics.IgnoreLayerCollision(CARS_LAYER, CARS_LAYER, true);
        tagManager.ApplyModifiedProperties();
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
            Action action;
            actionQueue.TryDequeue(out action);
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
    private void InitNewCar(int map_id, int car_id, float topSpeed, float maxSteeringAngle)
    {
        GameObject car = Instantiate(carPrefab);
        car.GetComponent<CarController>().SetTopSpeed(topSpeed);
        car.GetComponent<CarController>().SetMaxSteeringAngle(maxSteeringAngle);
        car.GetComponent<CameraRecorder>().SetPath(pathTimestamp, map_id.ToString(), car_id.ToString());
        var children = car.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children)
        {
            child.gameObject.layer = LayerMask.NameToLayer("cars");
        }
        cars.Add(car);
    }
    private void GetCamera(int instance_id, string map_id)
    {
        GameObject frameComponent = cars[instance_id].transform.GetChild(0).gameObject;
        Camera[] cameras = frameComponent.GetComponentsInChildren<Camera>();
        foreach (Camera cam in cameras)
        {
            cam.cullingMask = CAMERA_LAYER;
        }
        cameraRecorder.SetCar(cars[instance_id]);
        cameraRecorder.SetCameras(cameras);
        String path = "getCamera/" + map_id + "/" + instance_id.ToString();
        cameraRecorder.SavePhoto(path,"test"); // path , photo_name
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
            //to do
            return;
        }

        int map_id = Int32.Parse(arguments[0]);
        Debug.Assert(map_id >= 0);

        if (arguments[1] == "init_new")
        {
            actionQueue.Enqueue(() =>
            {
                InitNewCar(0, Int32.Parse(arguments[0]), float.Parse(arguments[2]), float.Parse(arguments[3])); // map_id, car_id, topSpeed, maxsteeringAngle
            });
            return;
        }
        if (arguments[1] == "delete")
        {
            //to do
            return;
        }

        int instance_id = Int32.Parse(arguments[1]);
        Debug.Assert(instance_id >= 0);
        if (arguments[2] == "set")
        {
            if (arguments[3] == "steer")
            {
                actionQueue.Enqueue(() =>
                {
                    float steer = float.Parse(arguments[4]) / 100.0f;
                    if (steer * 100.0 > cars[instance_id].GetComponent<CarController>().GetMaxSteeringAngle())
                    {
                        steer = cars[instance_id].GetComponent<CarController>().GetMaxSteeringAngle();
                    }
                    if (steer * 100.0 < -cars[instance_id].GetComponent<CarController>().GetMaxSteeringAngle())
                    {
                        steer = -cars[instance_id].GetComponent<CarController>().GetMaxSteeringAngle();
                    }
                    cars[instance_id].GetComponent<CarInputController>().SetSteeringInput(steer);
                    Debug.Log(cars[instance_id].GetComponent<CarInputController>().GetSteeringInput());
                });
            }
            else if (arguments[3] == "engine")
            {
                actionQueue.Enqueue(() =>
                {
                    float acceleration = float.Parse(arguments[4]) / 100.0f;
                    if (acceleration * 100.0f > cars[instance_id].GetComponent<CarController>().GetTopSpeed())
                    {
                        acceleration = cars[instance_id].GetComponent<CarController>().GetTopSpeed();
                    }
                    if (acceleration * 100.0f < -cars[instance_id].GetComponent<CarController>().GetTopSpeed())
                    {
                        acceleration = -cars[instance_id].GetComponent<CarController>().GetTopSpeed();
                    }
                    cars[instance_id].GetComponent<CarInputController>().SetAccelInput(acceleration);
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
                    GetCamera(instance_id, arguments[0]); //car_id, map_id
                }
                );
            }
            else if (arguments[2] == "get")
            {
                if (arguments[3] == "steer")
                {
                    Debug.LogError("xd driven development");
                    actionQueue.Enqueue(() =>
                    {
                        float steer = cars[instance_id].GetComponent<CarInputController>().GetSteeringInput() * 100.0f;
                        string message = arguments[0] + " " + arguments[1] + " " + arguments[3] + " " + steer.ToString() + " " + DateTime.Now.ToString();
                        if (cars[instance_id].activeSelf == false)
                            message = arguments[0] + " " + arguments[1] + " " + "deleted";
                        SendMessageToServer(message);
                    });
                }
                else if (arguments[3] == "engine")
                {
                    actionQueue.Enqueue(() =>
                    {
                        float steer = cars[instance_id].GetComponent<CarInputController>().GetSteeringInput() * 100.0f;
                        string message = arguments[0] + " " + arguments[1] + " " + arguments[3] + " " + steer.ToString() + " " + DateTime.Now.ToString();
                        if (cars[instance_id].activeSelf == false)
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
                    cars[instance_id].SetActive(false);
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