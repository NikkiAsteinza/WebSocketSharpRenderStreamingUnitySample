using UnityEngine;
using WebSocketSharp;
using System;
using UnityEngine.Events;

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