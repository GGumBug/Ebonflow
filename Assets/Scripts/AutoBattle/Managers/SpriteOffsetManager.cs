using System.Collections.Generic;
using UnityEngine;

public class SpriteOffsetManager : Singleton<SpriteOffsetManager>
{
    private float referenceY = 0f;
    private float offsetWeight = 0.01f;
    private float colliderOffsetY = -0.02264f;
    private Vector3 referenceScale = Vector3.one;

    private Camera targetCamera;
    private float referenceDistance;
    private Vector3 referenceLocalPosition;

    private readonly List<SpriteRenderer> sprites = new List<SpriteRenderer>();

    void Awake()
    {
        targetCamera = Camera.main;
    }

    /// <summary>
    /// Register a SpriteRenderer for automatic offset adjustments.
    /// </summary>
    public void Register(SpriteRenderer sr)
    {
        if (!sprites.Contains(sr))
        {
            if (sprites.Count == 0) InitializeReference(sr);
            sprites.Add(sr);
        }
    }

    /// <summary>
    /// Unregister a SpriteRenderer.
    /// </summary>
    public void Unregister(SpriteRenderer sr)
    {
        sprites.Remove(sr);
    }

    private void InitializeReference(SpriteRenderer sr)
    {
        Vector3 spritePos = sr.transform.position;
        Vector3 refPos = new Vector3(spritePos.x, referenceY, spritePos.z);
        Vector3 toRef = refPos - targetCamera.transform.position;
        referenceDistance = Mathf.Max(Vector3.Dot(toRef, targetCamera.transform.forward), 0.01f);
        referenceLocalPosition = sr.transform.localPosition;
        referenceScale = sr.transform.localScale;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        foreach (var sr in sprites)
            AdjustSprite(sr);
    }

    private void AdjustSprite(SpriteRenderer sr)
    {
        // Scale adjustment
        Vector3 toSprite = sr.transform.position - targetCamera.transform.position;
        float distance = Mathf.Max(Vector3.Dot(toSprite, targetCamera.transform.forward), 0.01f);
        float scaleFactor = referenceDistance / distance;
        sr.transform.localScale = referenceScale * scaleFactor;

        // Position offset
        Vector3 spriteWorld = sr.transform.position;
        Vector3 camPos = targetCamera.transform.position;

        float xOffset = (spriteWorld.x - camPos.x) * offsetWeight;
        float yOffset = (spriteWorld.y - camPos.y) * offsetWeight;
        float depthDelta = distance - referenceDistance;
        yOffset += depthDelta * colliderOffsetY;

        sr.transform.localPosition = new Vector3(
            referenceLocalPosition.x + xOffset,
            referenceLocalPosition.y + yOffset,
            referenceLocalPosition.z
        );
    }
}
