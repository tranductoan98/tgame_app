using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PlayerRenderUtil
{
    public static IEnumerator LoadAndRenderPlayer(MonoBehaviour caller, Transform root, int playerId, Action<List<PlayerRenderData>> onRenderPartsReady)
    {
        yield return PlayerSlotUtil.LoadPlayerInventory(playerId, items =>
        {
            caller.StartCoroutine(RenderEquippedItems(root, items, caller, onRenderPartsReady));
        });
    }

    public static IEnumerator RenderEquippedItems(Transform root, List<Item> items, MonoBehaviour caller, Action<List<PlayerRenderData>> onRenderPartsReady)
    {
        foreach (Transform child in root)
        GameObject.Destroy(child.gameObject);

        List<Item> equippedItems = items
            .Where(item => item.equipped)
            .OrderBy(item => item.zorder)
            .ToList();

        List<PlayerRenderData> renderParts = new List<PlayerRenderData>();

        foreach (var item in equippedItems)
        {
            List<ItemAnimationFrame> frames = AnimationHelper.ParseAnimation(item.animation);

            var idleFrames = frames.Take(2).ToArray();
            var moveFrames = frames.Count >= 4 ? frames.Skip(2).Take(2).ToArray() : null;

            Sprite[] idleSprites = new Sprite[2];
            Sprite[] moveSprites = moveFrames != null ? new Sprite[2] : null;

            if (item.icon == 0)
            {
                for (int i = 0; i < idleSprites.Length; i++)
                {
                    yield return caller.StartCoroutine(PlayerSlotUtil.LoadSpriteFromAtlas(item.itemId, result => idleSprites[i] = result));
                }
                if (moveSprites != null)
                {
                    for (int i = 0; i < moveSprites.Length; i++)
                    {
                        yield return caller.StartCoroutine(PlayerSlotUtil.LoadSpriteFromAtlas(item.itemId, result => moveSprites[i] = result));
                    }
                }
            }
            else
            {
                for (int i = 0; i < idleFrames.Length; i++)
                {
                    yield return caller.StartCoroutine(PlayerSlotUtil.LoadSpriteFromUrl(idleFrames[i].img, result => idleSprites[i] = result));
                }
                if (moveFrames != null)
                {
                    for (int i = 0; i < moveFrames.Length; i++)
                    {
                        yield return caller.StartCoroutine(PlayerSlotUtil.LoadSpriteFromUrl(moveFrames[i].img, result => moveSprites[i] = result));
                    }
                }
            }

            GameObject go = new GameObject($"Part_{item.itemId}");
            go.transform.SetParent(root, false);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            go.transform.localScale = new Vector3(item.height, item.width, 1);
            sr.sortingOrder = item.zorder;

            renderParts.Add(new PlayerRenderData
            {
                go = go,
                sr = sr,
                item = item,
                idleFrames = idleFrames,
                moveFrames = moveFrames,
                idleSprites = idleSprites,
                moveSprites = moveSprites
            });
        }

        onRenderPartsReady?.Invoke(renderParts);
    }
}
