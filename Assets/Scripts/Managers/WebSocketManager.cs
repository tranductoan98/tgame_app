using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;
using Newtonsoft.Json;

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance { get; set; }
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

    public async void Connect(string token, int playerId)
    {
        if (IsConnected) return;

        string url = $"{GameConfig.SocketUrl}/ws-game?playerId={playerId}&token={token}";

        websocket = new WebSocket(url);

        websocket.OnOpen += () =>
        {
            Debug.Log("✅ WebSocket đã kết nối");
            InvokeRepeating(nameof(SendPing), 30f, 30f);
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("❌ WebSocket lỗi: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("🔒 WebSocket đóng");
            CancelInvoke(nameof(SendPing));
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);

            if (MapSceneManager.Instance != null)
            {
                MapSceneManager.Instance.OnSocketMessageReceived(message);
            }
        };

        try
        {
            await websocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ Kết nối WebSocket thất bại: " + ex.Message);
        }
    }

    public async void Disconnect()
    {
        if (websocket != null)
        {
            await websocket.Close();
            websocket = null;
            Debug.Log("🔌 WebSocket đã ngắt kết nối");
        }
    }

    void SendPing()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            string pingMessage = "{\"type\": \"ping\"}";
            websocket.SendText(pingMessage);
            Debug.Log("🔁 Sent ping");
        }
    }

    public async void SendText(string message)
    {
        if (IsConnected)
        {
            await websocket.SendText(message);
        }
        else
        {
            Debug.LogWarning("⚠️ Không thể gửi: WebSocket chưa kết nối.");
        }
    }

    public void SendJson(object data)
    {
        string json = JsonConvert.SerializeObject(data);
        SendText(json);
    }

    private void Update()
    {
        websocket?.DispatchMessageQueue();
    }
    async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
            websocket = null;
        }
    }
}
