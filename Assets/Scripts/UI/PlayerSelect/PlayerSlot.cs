using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerSlot : MonoBehaviour
{
    public Transform avatarRoot;

    public GameObject Player;

    public void Setup(PlayerData playerData)
    {

        StartCoroutine(LoadPlayerInventory(playerData.playerid, OnInventoryLoaded));
    }

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

    public IEnumerator LoadSpriteFromUrl(int imgId, Action<Sprite> onLoaded)
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

    public IEnumerator LoadSpriteFromAtlas(int itemId, Action<Sprite> onLoaded)
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

    private void OnInventoryLoaded(List<Item> items)
    {
        List<Item> equippedItems = items
            .Where(item => item.equipped)
            .OrderBy(item => item.zorder)
            .ToList();

        foreach (Transform child in avatarRoot)
        {
            if (child.gameObject != Player)
                Destroy(child.gameObject);
        }

        Player.transform.SetParent(avatarRoot, false);
        Player.transform.localPosition = new Vector3(5,9,0);

        foreach (Transform child in Player.transform)
            Destroy(child.gameObject);

        StartCoroutine(LoadAllSprites(Player.transform, equippedItems, this));
    }

    public IEnumerator LoadAllSprites(Transform root, List<Item> equippedItems, MonoBehaviour caller)
    {
        List<PartSpriteAnim2> partAnims = new List<PartSpriteAnim2>();

        foreach (var item in equippedItems)
        {
            List<ItemAnimationFrame> frames = AnimationHelper.ParseAnimation(item.animation);

            if (frames.Count < 2) continue;

            var frame1 = frames[0];
            var frame2 = frames[1];

            Sprite sprite1 = null, sprite2 = null;

            if (item.icon == 0)
            {
                yield return StartCoroutine(LoadSpriteFromAtlas(item.itemId, result => sprite1 = result));
                yield return StartCoroutine(LoadSpriteFromAtlas(item.itemId, result => sprite2 = result));
            }
            else
            {
                yield return StartCoroutine(LoadSpriteFromUrl(frame1.img, result => sprite1 = result));
                yield return StartCoroutine(LoadSpriteFromUrl(frame2.img, result => sprite2 = result));
            }

            if (sprite1 != null && sprite2 != null)
            {
                partAnims.Add(new PartSpriteAnim2
                {
                    item = item,
                    frames = new[] { frame1, frame2 },
                    sprites = new[] { sprite1, sprite2 }
                });
            }
        }

        foreach (var part in partAnims.OrderBy(p => p.item.zorder))
        {
            GameObject go = new GameObject("Part_" + part.item.itemId);
            go.transform.SetParent(root, false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "PlayerSprite";
            sr.sortingOrder = part.item.zorder;

            StartCoroutine(LoopFrames(sr, part.sprites, part.frames, go.transform, part.item.width));
        }
    }
    private IEnumerator LoopFrames(SpriteRenderer sr, Sprite[] sprites, ItemAnimationFrame[] frames, Transform transform, float scalePercent)
    {
        int index = 0;
        float positionScale = 50f;
        float sizeScale = 50f;

        while (true)
        {
            sr.sprite = sprites[index];

            Vector3 offset = new Vector3(frames[index].dx, frames[index].dy, 0);
            transform.localPosition = offset * positionScale;

            float scale = scalePercent * sizeScale;
            transform.localScale = new Vector3(scale, scale, 1);

            index = (index + 1) % sprites.Length;

            yield return new WaitForSeconds(0.4f);
        }
    }
}