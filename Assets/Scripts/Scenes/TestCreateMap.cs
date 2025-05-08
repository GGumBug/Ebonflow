using UnityEngine;
using RoguelikeMap;
using Cysharp.Threading.Tasks;

public class TestCreateMap : MonoBehaviour
{
    private async UniTask Awake() 
    {
        RoguelikeMapManager roguelikeMapManager = gameObject.AddComponent<RoguelikeMapManager>();
        MapGenerationSettings mapGenerationSettings = await AddressableManager.Instance.Load<MapGenerationSettings>(AddressableKeyExtensions.ToKey(AddressableKey.MapGenerationSettings));
        var uiMapView = await AddressableManager.Instance.InstantiateAsync<UIMapView>(AddressableKeyExtensions.ToKey(AddressableKey.UIMapView));
        roguelikeMapManager.Setup(mapGenerationSettings, uiMapView);
    }
}
