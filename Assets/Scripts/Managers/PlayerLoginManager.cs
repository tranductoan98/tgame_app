using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
using Newtonsoft.Json;

public class PlayerLoginManager : MonoBehaviour
{
    public void LoginPlayer(int playerId)
    {
        StartCoroutine(LoginPlayerCoroutine(playerId));
    }

    private IEnumerator LoginPlayerCoroutine(int playerId)
    {
        string url = $"{GameConfig.ApiBaseUrl}/player/login";
        string jsonData = JsonConvert.SerializeObject(new { playerId = playerId });
        string token = PlayerPrefs.GetString("token");

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            PlayerData player = JsonConvert.DeserializeObject<PlayerData>(request.downloadHandler.text);
            GameSession.CurrentPlayer = player;

            if (WebSocketManager.Instance != null)
            {
                WebSocketManager.Instance.Connect(token, player.playerid);
                PlayerPrefs.SetInt("playerid", player.playerid);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError("❌ WebSocketManager.Instance is null! Đảm bảo bạn đã gắn WebSocketManager vào scene.");
            }

            SceneManager.LoadScene("MapScene");
        }
        else
        {
            Debug.LogError("Player login failed: " + request.error);
        }
    }
}
