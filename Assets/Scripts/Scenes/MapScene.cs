using Cysharp.Threading.Tasks;
using RoguelikeMap;
using RoguelikeMap.UI;
using UnityEngine;

public class MapScene : SceneBase, INodeClickHandler
{
    private MapGenerationSettings _mapGenerationSettings;
    private UIMapView _mapView;
    private RoguelikeMapManager _roguelikeMapManager;

    public override async UniTask LoadAssets()
    {
        _mapGenerationSettings = await AddressableManager.Instance.Load<MapGenerationSettings>(AddressableKeyExtensions.ToKey(AddressableKey.MapGenerationSettings));
        _mapView = await AddressableManager.Instance.InstantiateAsync<UIMapView>(AddressableKeyExtensions.ToKey(AddressableKey.UIMapView));
    }

    public override async UniTask InitializeData()
    {
        _roguelikeMapManager = gameObject.AddComponent<RoguelikeMapManager>();
        await UniTask.Yield();
    }

    public override async UniTask SetupScene()
    {
        _roguelikeMapManager.Setup(_mapGenerationSettings, _mapView, this);
        await UniTask.Yield();
    }

    public override async UniTask FinalizeLoading()
    {
        await UniTask.Yield();
    }

    public override async UniTask DebugMode()
    {
        _mapGenerationSettings = await AddressableManager.Instance.Load<MapGenerationSettings>(AddressableKeyExtensions.ToKey(AddressableKey.MapGenerationSettings));
        _mapView = await AddressableManager.Instance.InstantiateAsync<UIMapView>(AddressableKeyExtensions.ToKey(AddressableKey.UIMapView));
        _roguelikeMapManager = gameObject.AddComponent<RoguelikeMapManager>();
        _roguelikeMapManager.Setup(_mapGenerationSettings, _mapView, this);
    }

    public void OnNodeClicked(int stage, int floor, int locationTypeId)
    {
        var context = new AutoBattleSceneContext(
            stage,
            floor,
            locationTypeId
        );

        Debug.Log($"Floor : {floor} LocationType : {(LocationType)context.LocationTypeId}");
    }
}