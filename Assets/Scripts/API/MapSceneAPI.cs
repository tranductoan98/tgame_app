using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

public static class MapSceneAPI
{
    public static IEnumerator FetchMapInfo(int mapId, Action<MapInfo> onSuccess, Action<string> onError)
    {
        string url = $"{GameConfig.ApiBaseUrl}/map/{mapId}";
        string token = PlayerPrefs.GetString("token");

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        yield return request.SendWebRequest();

        try
        {
            string json = request.downloadHandler.text;

            if (json != null && json != "")
            {
                MapInfo info = JsonConvert.DeserializeObject<MapInfo>(json);
                onSuccess?.Invoke(info);
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke($"Lỗi parse JSON map info: {ex.Message}");
        }
    }

    public static IEnumerator FetchMapTiles(int mapId, Action<List<MapTileData>> onSuccess, Action<string> onError)
    {
        string url = $"{GameConfig.ApiBaseUrl}/map-image-data/map/{mapId}";
        string token = PlayerPrefs.GetString("token");

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        yield return request.SendWebRequest();

        try
        {
            string json = request.downloadHandler.text;
            List<MapTileData> tiles = JsonConvert.DeserializeObject<List<MapTileData>>(json);
            onSuccess?.Invoke(tiles);
        }
        catch (Exception ex)
        {
            onError?.Invoke($"Lỗi parse JSON: {ex.Message}");
        }
    }
}
