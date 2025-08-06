using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOffsetAdjuster : MonoBehaviour
{
    private SpriteRenderer sr;

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        SpriteOffsetManager.Instance?.Register(sr);
    }

    void OnDisable()
    {
        SpriteOffsetManager.Instance?.Unregister(sr);
    }
}