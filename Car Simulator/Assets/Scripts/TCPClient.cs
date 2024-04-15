using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class TCPClient : MonoBehaviour
{
    private string serverIP = "127.0.0.1"; // Set this to your server's IP address.
    private int serverPort = 1984;             // Set this to your server's port.
    [SerializeField] public bool connectToServer;
    private bool connectedToServer = false;
    [SerializeField] GameObject carPrefab;
    [SerializeField] List<GameObject> cars = new List<GameObject>();

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;
    private ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

    void Start()
    {
        
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


    private void HandleServerMessage(string message)
    {
        string[] arguments = message.Split(' ');
        if(arguments.Length == 0)
        {
            Debug.LogError("Received empty message from server");
            return;
        }
        if(arguments[0] == "init_new_map")
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
                GameObject car = Instantiate(carPrefab);
                cars.Add(car);
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
            if(arguments[3] == "steer")
            {
                actionQueue.Enqueue(() =>
                {
                    float steer = float.Parse(arguments[4]) / 100.0f;
                    cars[instance_id].GetComponent<CarInputController>().SetSteeringInput(steer);
                });
            }
            else if(arguments[3] == "engine")
            {
                actionQueue.Enqueue(() =>
                {
                    float acceleration = float.Parse(arguments[4]) / 100.0f;
                    cars[instance_id].GetComponent<CarInputController>().SetAccelInput(acceleration);
                });
            }
            return;
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