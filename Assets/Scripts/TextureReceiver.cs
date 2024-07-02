// Author: Nikki Asteinza (2024-06-22)
//This Unity script is designed to receive images through a WebSocket connection and render them onto a RenderTexture.

//Functionality:
//WebSocket Connection Management: Listens for connection, message, and close events from a WebSocket server.
//Image Processing: Decodes base64 image data received from the WebSocket server and converts it into a Texture2D.
//Rendering: Renders the decoded image onto a RenderTexture for display in Unity.

//Key Features:
//Event Handling: Utilizes UnityEvent to handle WebSocket events.
//Image Decoding: Processes JSON messages, extracts base64-encoded image data, and converts it to a texture.
//Thread Safety: Ensures image processing runs on Unity's main thread to avoid concurrency issues.

using System;
using UnityEngine;
using WebSocketSharp;

public class TextureReceiver : MonoBehaviour
{
    [SerializeField] private WebSocketClient _client;
    [SerializeField] private RenderTexture renderTexture;
    
    WebSocket websocket;
    void Awake()
    {
        _client.onOpen.AddListener(OnWebSocketOpenedHandler);
        _client.onMessage.AddListener(OnMessageReceivedHandler);
        _client.onClose.AddListener(OnWebSocketCloseHandler);
    }

    private void OnWebSocketOpenedHandler(WebSocket ws)
    {
        Debug.Log("Receiver connected to websocket");
        websocket = ws;
    }

    private void OnWebSocketCloseHandler()
    {
        websocket = null;
    }

    private void OnMessageReceivedHandler(string message)
    {
        ProcessMessage(message);
    }

    private void ProcessMessage(string jsonMessage)
    {
        try
        {
            // Parse JSON message
            WebSocketMessage message = JsonUtility.FromJson<WebSocketMessage>(jsonMessage);
            if (message == null)
            {
                Debug.LogError("Failed to parse JSON message");
                return;
            }
            if (message.id != "webcomponent")
            {

                return;
            }

            Debug.Log("Message identifier: " + message.id);

            // Convert base64 string to byte array
            byte[] imageBytes = Convert.FromBase64String(message.imageData);
            int[] imageSize = { message.width, message.height };
            // Process image bytes (load into texture) on the main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ProcessImage(imageBytes, imageSize);
            });
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception while processing message: " + ex.Message + "\nStack Trace: " + ex.StackTrace);
        }
    }

    private void ProcessImage(byte[] imageBytes, int[] size)
    {
        try
        {
            // Create texture on the main thread
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes); // Load the image bytes into the texture

            // Downscale the texture if it's too large
            if (texture.width > size[0] || texture.height > size[1])
            {
                texture.Reinitialize(Mathf.Min(texture.width, size[0]), Mathf.Min(texture.height, size[1]));
            }

            // Create or resize the RenderTexture based on texture dimensions
            if (renderTexture == null || renderTexture.width != texture.width || renderTexture.height != texture.height)
            {
                renderTexture = new RenderTexture(texture.width, texture.height, 0);
            }

            // Make sure RenderTexture is active
            RenderTexture.active = renderTexture;

            // Blit the texture onto the RenderTexture
            Graphics.Blit(texture, renderTexture);

            // Clean up
            RenderTexture.active = null;
            Destroy(texture); // Release the Texture2D
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception while processing image: " + ex.Message + "\nStack Trace: " + ex.StackTrace);
        }
    }
}
