using UnityEngine;

public class UnitModel : MonoBehaviour
{
    private void Awake()
    {
        gameObject.AddComponent<SpriteOffsetAdjuster>();
    }
}
