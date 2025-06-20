using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class PlayerAPI
{
    private static string baseUrl = $"{GameConfig.ApiBaseUrl}/player";
    private static string baseUrl2 = $"{GameConfig.ApiBaseUrl}";

    public static IEnumerator GetPlayers(int userId, Action<List<PlayerData>> onSuccess, Action<string> onError)
    {
        string token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            onError?.Invoke("Token không hợp lệ hoặc đã hết hạn");
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get($"{baseUrl}/my-players/{userId}");
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(www.error);
        }
        else
        {
            var json = www.downloadHandler.text;
            try
            {
                if (string.IsNullOrWhiteSpace(json) || json == "[]")
                {
                    onSuccess?.Invoke(new List<PlayerData>());
                }
                else
                {
                    var players = JsonConvert.DeserializeObject<List<PlayerData>>(json);
                    onSuccess?.Invoke(players);
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke("Lỗi phân tích dữ liệu: " + ex.Message);
            }
        }
    }

    public static IEnumerator RegisterPlayer(int id, string name, string gd, Action<string> onSuccess, Action<string> onError)
    {
        string token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            onError?.Invoke("Token không hợp lệ hoặc đã hết hạn");
            yield break;
        }

        var data = new
        {
            userId = id,
            characterName = name,
            gender = gd
        };

        string json = JsonConvert.SerializeObject(data);
        var request = new UnityWebRequest($"{baseUrl}/create", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
        }
        else
        {
            var response = request.downloadHandler.text;
            onSuccess?.Invoke(response);
        }
    }

    public static IEnumerator getPositionByPlayerId(int playerId, Action<PlayerPositionData> onSuccess, Action<string> onError)
    {
        string token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            onError?.Invoke("Token không hợp lệ hoặc đã hết hạn");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl2}/player-position/{playerId}");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("❌ Lỗi tải vị trí player: " + request.error);
        }
        else
        {
            var json = request.downloadHandler.text;
            try
            {
                var pos = JsonConvert.DeserializeObject<PlayerPositionData>(json);
                onSuccess?.Invoke(pos);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Lỗi parse JSON: " + ex.Message);
            }
        }
    }

    public static IEnumerator GetPlayersInMap(int mapId, Action<List<PlayerPositionData>> onDone, Action<string> onError)
    {
        string token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            onError?.Invoke("Token không hợp lệ hoặc đã hết hạn");
            yield break;
        }

        string url = $"{GameConfig.ApiBaseUrl}/player-position/map/{mapId}";
        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var list = JsonConvert.DeserializeObject<List<PlayerPositionData>>(request.downloadHandler.text);
            onDone?.Invoke(list);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    public static IEnumerator LogoutPlayer(Action onSuccess, Action<string> onError)
    {
        string token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            onError?.Invoke("Token không hợp lệ hoặc đã hết hạn");
            yield break;
        }

        string url = $"{GameConfig.ApiBaseUrl}/player/logout";
        var request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("Authorization", "Bearer " + token);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"Lỗi mạng: {request.error}");
        }
        else if (request.responseCode == 200)
        {
            PlayerPrefs.DeleteKey("token");
            PlayerPrefs.DeleteKey("playerid");
            PlayerPrefs.DeleteKey("userId");
            onSuccess?.Invoke();
        }
        else
        {
            string message = $"Lỗi không xác định ({request.responseCode})";
            onError?.Invoke(message);
        }
    }
}
