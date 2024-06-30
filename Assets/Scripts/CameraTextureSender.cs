using System;
using System.Collections;
using UnityEngine;
using WebSocketSharp;

public class CameraTextureSender : MonoBehaviour
{
    [SerializeField] private WebSocketClient _client;
    [SerializeField] private Camera _streamingCamera;

    WebSocket websocket;
    RenderTexture renderTexture;
    Texture2D cameraTexture;

    void Awake()
    {
        // Set up the WebSocket client event listeners
        _client.onOpen.AddListener(OnWebSocketOpenedHandler);
        _client.onClose.AddListener(OnWebSocketCloseHandler);
    }

    private void OnWebSocketOpenedHandler(WebSocket ws)
    {
        Debug.Log("Broadcaster connected to websocket");
        websocket = ws;
        // Start the coroutine to send the camera image
        StartCoroutine(SendCameraImage());
    }

    private void OnWebSocketCloseHandler()
    {
        websocket = null;
    }

    private void OnDestroy()
    {
        // Clean up resources
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
        if (cameraTexture != null)
        {
            Destroy(cameraTexture);
        }
    }

    private IEnumerator SendCameraImage()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (renderTexture == null)
            {
                // Initialize the RenderTexture
                renderTexture = new RenderTexture(3840, 2160, 24);
                _streamingCamera.targetTexture = renderTexture;
                cameraTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            }

            // Render the camera's view to the RenderTexture
            _streamingCamera.Render();
            RenderTexture.active = renderTexture;

            // Read the RenderTexture contents into a Texture2D
            cameraTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            cameraTexture.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = null;
            _streamingCamera.targetTexture = null; // Reset the target texture

            // Encode the Texture2D to PNG
            byte[] cameraBytes = cameraTexture.EncodeToPNG();
            string base64String = Convert.ToBase64String(cameraBytes);

            // Create and send the WebSocket message
            WebSocketMessage message = new WebSocketMessage
            {
                id = "UnityVR",
                imageData = base64String,
                width = renderTexture.width,
                height = renderTexture.height
            };

            string jsonMessage = JsonUtility.ToJson(message);
            websocket.Send(jsonMessage);
            Debug.Log("Message sent: " + jsonMessage);
        }
    }
}
