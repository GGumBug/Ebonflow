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

    private UIAutoBattleShop _uIAutoBattleShop;
    private CardDrawManager _cardDrawManager;

    public Vector2Int GridTopRight => _gridTopRight;
    public Vector2Int GridBottomLeft => _gridBottomLeft;

    public override async UniTask LoadAssets()
    {
        await AutoBattleUnitManager.Instance.LoadAsset();
        _uIAutoBattleShop = await UIManager.Instance.OpenUIAsync<UIAutoBattleShop>();
        _cardDrawManager = new CardDrawManager();
    }

    public override async UniTask InitializeData()
    {
        AStarAlgorithmManager aStarAlgorithmManager = AStarAlgorithmManager.Instance;
        aStarAlgorithmManager.InitializeGrid(this);

        AutoBattleDataManager autoBattleDataManager = AutoBattleDataManager.Instance;
        autoBattleDataManager.Setup();

        AutoBattleManager.Instance.Setup();

        AutoBattleUnitManager autoBattleUnitManager = AutoBattleUnitManager.Instance;
        autoBattleUnitManager.Setup(aStarAlgorithmManager.Grid);

        aStarAlgorithmManager.RegisteBattleRoster(autoBattleUnitManager.Roster);

        _uIAutoBattleShop.RequestSoulCoin += autoBattleDataManager.AutoBattlePlayerDataContext.GetSoulCoin;
        _uIAutoBattleShop.SetUp(autoBattleDataManager.AutoBattlePlayerDataContext);
        autoBattleDataManager.AutoBattlePlayerDataContext.OnAddSoulCoin += _uIAutoBattleShop.CheckCanBuyCards;
        autoBattleDataManager.AutoBattlePlayerDataContext.OnSpendSoulCoin += _uIAutoBattleShop.CheckCanBuyCards;

        await _cardDrawManager.SetUp(AutoBattleUnitManager.Instance, _uIAutoBattleShop);
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
