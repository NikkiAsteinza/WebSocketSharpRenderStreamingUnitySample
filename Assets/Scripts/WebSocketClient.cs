// Author: Nikki Asteinza (2024-06-22)
//This Unity script, called WebSocketClient, manages the connection to a WebSocket server and facilitates communication through custom events.

//Functionality:
//WebSocket Connection: Establishes a WebSocket connection to the specified server.
//Custom Events: Uses UnityEvent to handle connection events, received messages, and connection closure.
//Message and Error Handling: Processes messages received from the server and handles connection errors.

using System;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

[System.Serializable]
public class OnOpen : UnityEvent<WebSocket>{}

public class WebSocketClient : MonoBehaviour
{

    public OnOpen onOpen;
    [HideInInspector]
    public UnityEvent<string> onMessage;
    [HideInInspector]
    public UnityEvent onClose;
    private string url = "ws://localhost:3000";
    private WebSocket ws;

    void Awake()
    {
        Debug.Log("Initializing WebSocket connection...");
        ws = new WebSocket(url);

        ws.OnOpen += (sender, e) =>
        {
            onOpen.Invoke(ws);
            Debug.Log("WebSocket connection established");
        };

        ws.OnMessage += (sender, e) =>
        {
            try
            {
                Debug.Log("Message received from server");
                onMessage.Invoke(e.Data);

            }
            catch (Exception ex)
            {
                Debug.LogError("Exception while processing message: " + ex.Message + "\nStack Trace: " + ex.StackTrace);
            }
        };

        ws.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket error: " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log(e.Reason);
            onClose.Invoke();
            Debug.Log("WebSocket connection closed");
        };

        ws.Connect();
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
        }
    }
}
