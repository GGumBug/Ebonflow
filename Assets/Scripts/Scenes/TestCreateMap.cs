using UnityEngine;
using RoguelikeMap;

public class TestCreateMap : MonoBehaviour
{
    private void Awake() 
    {
        RoguelikeMapManager roguelikeMapManager = gameObject.AddComponent<RoguelikeMapManager>();
        roguelikeMapManager.Setup(15, 7);
    }
}
