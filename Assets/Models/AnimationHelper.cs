using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

public static class AnimationHelper
{
    public static List<ItemAnimationFrame> ParseAnimation(string animationJson)
    {
        if (string.IsNullOrEmpty(animationJson))
            return new List<ItemAnimationFrame>();

        try
        {
            return JsonConvert.DeserializeObject<List<ItemAnimationFrame>>(animationJson);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse animation: {e.Message}");
            return new List<ItemAnimationFrame>();
        }
    }
}
