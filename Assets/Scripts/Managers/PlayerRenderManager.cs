using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderManager : MonoBehaviour
{
    public List<PlayerRenderData> parts = new List<PlayerRenderData>();
    public Func<bool> isMovingFunc;
    private bool isMoving = false;
    private float timer = 0f;
    private int frameIndex = 0;
    public float idleFrameDuration = 0.5f;
    public float moveFrameDuration = 0.2f;

    public void Setup(List<PlayerRenderData> data, Func<bool> isMovingFunc)
    {
        this.parts = data;
        this.isMovingFunc = isMovingFunc;
        this.isMoving = isMovingFunc?.Invoke() ?? false;
        frameIndex = 0;
        timer = 0f;
        UpdateFrame(isMoving);
    }

    public void SetMovingState(bool moving)
    {
        if (isMoving != moving)
        {
            isMoving = moving;
            frameIndex = 0;
            timer = 0f;
            UpdateFrame(isMoving);
        }
    }

    void Update()
    {
        bool currentMove = isMovingFunc?.Invoke() ?? false;

        if (currentMove != isMoving)
        {
            SetMovingState(currentMove);
        }

        float duration = isMoving ? moveFrameDuration : idleFrameDuration;

        timer += Time.deltaTime;
        if (timer >= duration)
        {
            timer = 0f;
            UpdateFrame(isMoving);
        }
    }

    private void UpdateFrame(bool moving)
    {
        foreach (var part in parts)
        {
            var sprites = moving ? part.moveSprites : part.idleSprites;
            var frames = moving ? part.moveFrames : part.idleFrames;

            if (sprites == null || frames == null || sprites.Length == 0 || frames.Length == 0)
                continue;

            int index = frameIndex % Mathf.Min(sprites.Length, frames.Length);

            part.sr.sprite = sprites[index];
            part.go.transform.localPosition = new Vector3(frames[index].dx, frames[index].dy, 0);
        }

        frameIndex++;
    }
    
    public void SetDirection(string newDirection)
    {
        float xScale = (newDirection == "LEFT") ? -1f : (newDirection == "RIGHT") ? 1f : transform.localScale.x;
        transform.localScale = new Vector3(xScale, 1f, 1f);
    }
}