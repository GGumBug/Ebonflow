using Cysharp.Threading.Tasks;
using RoguelikeMap;
using RoguelikeMap.UI;
using AutoBattle;

public class MapScene : SceneBase, INodeClickHandler
{
    private MapGenerationSettings _mapGenerationSettings;
    private UIMapView _mapView;
    private RoguelikeMapDirector _roguelikeMapManager;

    public override async UniTask LoadAssets()
    {
        _mapGenerationSettings = await AddressableManager.Instance.Load<MapGenerationSettings>(AddressableKey.MapGenerationSettings);
        _mapView = await UIManager.Instance.OpenUIAsync<UIMapView>(); 
    }

    public override async UniTask InitializeData()
    {
        MapSaveLoadManager.Instance.Init(_mapGenerationSettings);
        _roguelikeMapManager = gameObject.AddComponent<RoguelikeMapDirector>();
        await UniTask.Yield();
    }

    public override async UniTask SetupScene()
    {
        _mapView.Setup(this);
        _roguelikeMapManager.Setup(_mapView);
        MapSaveLoadManager.Instance.Setup(_roguelikeMapManager);
        _roguelikeMapManager.InitializeMapView();
        await UniTask.Yield();
    }

    public override async UniTask FinalizeLoading()
    {
        await UniTask.Yield();
    }

    public async void OnNodeClicked(int stage, int floor, int locationTypeId)
    {
        AutoBattleDataManager.Instance.AutoBattleSceneDataContext.Reset(() => new AutoBattleStageData(
            stage,
            floor,
            locationTypeId
        ));

        await SceneLoadManager.Instance.LoadSceneAsyncWithLoadingUI<AutoBattleScene>();
    }
}