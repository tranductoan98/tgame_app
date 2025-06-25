using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;

public class ChatSocketManager : MonoBehaviour
{
    public static ChatSocketManager Instance { get; set; }
    private WebSocket websocket;

    public bool IsConnected => websocket != null && websocket.State == WebSocketState.Open;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public async void Init(string token, int playerId)
    {
        string url = $"{GameConfig.ChatSocketUrl}?playerId={playerId}&token={token}";

        websocket = new WebSocket(url);

        websocket.OnOpen += () =>
        {
            Debug.Log("🔌 ChatSocket đã kết nối!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("❌ ChatSocket lỗi: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("🔌 ChatSocket đóng");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("📩 Received chat: " + message);
        };

        await websocket.Connect();
    }

    void Update()
    {
        websocket?.DispatchMessageQueue();
    }

    public async void SendChatMessage(string message)
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
