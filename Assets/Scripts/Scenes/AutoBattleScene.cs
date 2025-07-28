using AutoBattle;
using Cysharp.Threading.Tasks;
using DeckSystem;
using UnityEngine;
using UnityEngine.UI;

public class AutoBattleScene : SceneBase, IAStarGridSettings
{
    [SerializeField] private Button _btnStartBattle;
    [SerializeField] private Vector2Int _gridTopRight;
    [SerializeField] private Vector2Int _gridBottomLeft;

    private UIAutoBattle _uiAutoBattle;

    public Vector2Int GridTopRight => _gridTopRight;
    public Vector2Int GridBottomLeft => _gridBottomLeft;

    public override async UniTask LoadAssets()
    {
        await AutoBattleUnitManager.Instance.LoadAsset();
        _uiAutoBattle = await UIManager.Instance.OpenUIAsync<UIAutoBattle>();
    }

    public override async UniTask InitializeData()
    {
        AStarAlgorithmManager aStarAlgorithmManager = AStarAlgorithmManager.Instance;
        aStarAlgorithmManager.InitializeGrid(this);

        AutoBattleDataManager autoBattleDataManager = AutoBattleDataManager.Instance;
        autoBattleDataManager.Setup();

        AutoBattleManager.Instance.Setup();

        AutoBattleUnitManager autoBattleUnitManager = AutoBattleUnitManager.Instance;
        autoBattleUnitManager.Setup(aStarAlgorithmManager.Grid, _uiAutoBattle.SellZonePanel);

        aStarAlgorithmManager.RegisteBattleRoster(autoBattleUnitManager.Roster);

        _uiAutoBattle.SetUp(CardDrawManager.Instance, autoBattleDataManager.AutoBattlePlayerDataContext);
        autoBattleDataManager.AutoBattlePlayerDataContext.OnAddSoulCoin += _uiAutoBattle.AutoBattleShopUI.CheckCanBuyCards;
        autoBattleDataManager.AutoBattlePlayerDataContext.OnSpendSoulCoin += _uiAutoBattle.AutoBattleShopUI.CheckCanBuyCards;

        await CardDrawManager.Instance.SetUp(AutoBattleUnitManager.Instance, _uiAutoBattle.AutoBattleShopUI);
    }

    public override async UniTask SetupScene()
    {
        _btnStartBattle.onClick.AddListener(() => AutoBattleManager.Instance.StateController.GameState = AutoBattleGameState.BattlePhase);

        await UniTask.Yield();
    }

    public override async UniTask FinalizeLoading()
    {
        AutoBattleManager.Instance.StateController.GameState = AutoBattleGameState.PreparationPhase;

        await UniTask.Yield();
    }
}
