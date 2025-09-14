using Cysharp.Threading.Tasks;
using RoguelikeMap;
using RoguelikeMap.UI;
using AutoBattle;

public class MapScene : SceneBase, INodeClickHandler
{
    private UIMapView _mapView;
    private RoguelikeMapDirector _roguelikeMapManager;

    public override async UniTask LoadAssets()
    {
        _mapView = await UIManager.Instance.OpenUIAsync<UIMapView>(); 
    }

    public override async UniTask InitializeData()
    {
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

        _mapView.ResetScrollbarVertical();
    }

    public async void OnNodeClicked(int stage, int floor, int locationTypeId)
    {
        AutoBattleDataManager.Instance.AutoBattleSceneDataContext.Reset(() => new AutoBattleStageData(
            true,
            stage,
            floor,
            locationTypeId,
            -1
        ));

        MapSaveLoadManager.Instance.SaveMap();
        await SceneLoadManager.Instance.LoadSceneAsyncWithLoadingUI<AutoBattleScene>();
    }
}