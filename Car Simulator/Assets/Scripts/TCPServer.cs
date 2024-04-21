using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    [SerializeField] private string[] customMessage = 
        { "init_new_map", "0 init_new 100 100", "0 0 set engine 30", "0 0 set steer 30", "0 0 get camera 0", "0 0 delete", "0 0 get steer", "0 0 get engine" };
    [SerializeField] private bool[] sendCustomMessage;

    private TcpListener server = null;
    private TcpClient client = null;
    private NetworkStream stream = null;
    private Thread thread;

    private void Start()
    {
        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }

    private void Update()
    {
        if (customMessage.Length != sendCustomMessage.Length)
        {
            sendCustomMessage = new bool[customMessage.Length];
        }
        for(int i = 0; i < sendCustomMessage.Length; i++)
        {
            if (sendCustomMessage[i])
            {
                SendMessageToClient(customMessage[i]);
                sendCustomMessage[i] = false;
            }
        }
    }

    private void SetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, 1984);
            server.Start();

            byte[] buffer = new byte[1024];
            string data = null;

            while (true)
            {
                Debug.Log("Waiting for connection...");
                client = server.AcceptTcpClient();
                Debug.Log("Connected!");

                data = null;
                stream = client.GetStream();

                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    data = Encoding.UTF8.GetString(buffer, 0, i);
                    Debug.Log("Received: " + data);
                }
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            server.Stop();
        }
    }

    private void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
        server.Stop();
        thread.Abort();
    }

    public void SendMessageToClient(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message);
        stream.Write(msg, 0, msg.Length);
        Debug.Log("Sent: " + message);
    }
}