using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;

public class ChatSocketManager : MonoBehaviour
{
    private WebSocket websocket;

    async void Start()
    {
        websocket = new WebSocket("ws://<YOUR-IP>:8080/ws-chat");

        websocket.OnOpen += () => {
            Debug.Log("🔌 Chat WebSocket Connected!");
        };

        websocket.OnError += (e) => {
            Debug.Log("❌ Chat WebSocket Error: " + e);
        };

        websocket.OnClose += (e) => {
            Debug.Log("🔌 Chat WebSocket Closed");
        };

        websocket.OnMessage += (bytes) => {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("📩 Received chat: " + message);
        };

        await websocket.Connect();
    }

    void Update()
    {
        websocket?.DispatchMessageQueue();
    }

    public async void SendMessage(string message)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
