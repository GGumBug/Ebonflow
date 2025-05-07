using UnityEngine;
using RoguelikeMap;
using Cysharp.Threading.Tasks;

public class TestCreateMap : MonoBehaviour
{
    private async UniTask Awake() 
    {
        RoguelikeMapManager roguelikeMapManager = gameObject.AddComponent<RoguelikeMapManager>();
        await roguelikeMapManager.Setup();
    }
}
