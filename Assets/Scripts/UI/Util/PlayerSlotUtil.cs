using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class PlayerSlotUtil
{
    public static IEnumerator LoadPlayerInventory(int playerId, Action<List<Item>> onLoaded)
    {
        string baseUrl = GameConfig.ApiBaseUrl;
        string url = $"{baseUrl}/inventory/player/{playerId}?isEquippedCheck=true";
        string token = PlayerPrefs.GetString("token", "");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Load inventory failed: {request.error}");
                yield break;
            }

            string json = request.downloadHandler.text;
            List<Item> items = JsonHelper.FromJson<Item>(json);
            onLoaded?.Invoke(items);
        }
    }

    public static IEnumerator LoadSpriteFromUrl(int imgId, Action<Sprite> onLoaded)
    {
        string url = $"{GameConfig.BaseUrl}/uploads/hd/item/{imgId}.png";
        string token = PlayerPrefs.GetString("token", "");

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load sprite: " + request.error);
                onLoaded?.Invoke(null);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            onLoaded?.Invoke(sprite);
        }
    }

    public static IEnumerator LoadSpriteFromAtlas(int itemId, Action<Sprite> onLoaded)
    {
        string url = $"{GameConfig.ApiBaseUrl}/image-data/item/{itemId}";
        string token = PlayerPrefs.GetString("token", "");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Load ImageData failed: " + request.error);
                onLoaded?.Invoke(null);
                yield break;
            }

            ImageData imageData = JsonUtility.FromJson<ImageData>(request.downloadHandler.text);
            Texture2D atlasTexture = Resources.Load<Texture2D>($"big/{imageData.imageId}");

            if (atlasTexture == null)
            {
                Debug.LogError($"Atlas not found: big/{imageData.imageId}");
                onLoaded?.Invoke(null);
                yield break;
            }

            Rect rect = new Rect(imageData.x, imageData.y, imageData.w, imageData.h);
            Sprite sprite = Sprite.Create(atlasTexture, rect, new Vector2(0.5f, 0f));
            onLoaded?.Invoke(sprite);
        }
    }
}