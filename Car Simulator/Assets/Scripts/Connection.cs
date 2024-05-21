using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

public class Connection : MonoBehaviour
{
    WebSocket websocket;
    TCPClient client;
    // Start is called before the first frame update
    async void Start()
    {
        client = GetComponent<TCPClient>();
        websocket = new WebSocket("ws://localhost:6000");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // Reading a plain text message
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received OnMessage! (" + bytes.Length + " bytes) " + message);
            client.HandleServerMessage(message);
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    public async void SendWebSocketMessage(String message)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
            Debug.Log("Sent message: " + message);
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}