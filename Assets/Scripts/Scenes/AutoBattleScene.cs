using AutoBattle;
using CombatSystem;
using Cysharp.Threading.Tasks;
using DeckSystem;
using StageEditor;
using UnityEngine;
using UnityEngine.UI;

public class AutoBattleScene : SceneBase, IAStarGridSettings
{
    [SerializeField] private Button _btnStartBattle;
    [SerializeField] private Vector2Int _gridTopRight;
    [SerializeField] private Vector2Int _gridBottomLeft;

    private AutoBattleStagePicker _autoBattleStagePicker;
    private StageData _currentStageData;
    private UIAutoBattle _uiAutoBattle;

    public Vector2Int GridTopRight => _gridTopRight;
    public Vector2Int GridBottomLeft => _gridBottomLeft;

    public override async UniTask LoadAssets()
    {
        _autoBattleStagePicker = new AutoBattleStagePicker();

        AutoBattleDataManager dataManager = AutoBattleDataManager.Instance;
        if (isDebugMode)
        {
            Debug.LogWarning("autobattleSceneDataContext가 null 입니다. 더미데이터 생성");
            dataManager.Setup();
            AutoBattleStageData dummy = new AutoBattleStageData(true, 1, 0, 2, 0);
            _currentStageData = _autoBattleStagePicker.PickStage(dummy);
        }
        else
        {
            _currentStageData = _autoBattleStagePicker.PickStage(dataManager.AutoBattleSceneDataContext.Data);
            dataManager.AutoBattleSceneDataContext.SetShouldResumeBattle(true);
            dataManager.AutoBattleSceneDataContext.SetStageID(_currentStageData.stageID);
            dataManager.AutoBattleSceneDataContext.Save();
        }

        await AutoBattleUnitManager.Instance.LoadAsset();
        _uiAutoBattle = await UIManager.Instance.OpenUIAsync<UIAutoBattle>();
    }

    public override async UniTask InitializeData()
    {
        AStarAlgorithmManager aStarAlgorithmManager = AStarAlgorithmManager.Instance;
        aStarAlgorithmManager.InitializeGrid(this);

        AutoBattleManager.Instance.Setup();
        CombatManager.Instance.Setup();

        AutoBattleUnitManager autoBattleUnitManager = AutoBattleUnitManager.Instance;
        autoBattleUnitManager.Setup(aStarAlgorithmManager.Grid, _uiAutoBattle.SellZonePanel, _currentStageData.unitList);

        aStarAlgorithmManager.RegisteBattleRoster(autoBattleUnitManager.Roster);

        AutoBattleDataManager autoBattleDataManager = AutoBattleDataManager.Instance;
        _uiAutoBattle.SetUp(CardDrawManager.Instance, autoBattleDataManager.AutoBattlePlayerDataContext);
        autoBattleDataManager.AutoBattlePlayerDataContext.OnAddSoulCoin += _uiAutoBattle.AutoBattleShopUI.CheckCanBuyCards;
        autoBattleDataManager.AutoBattlePlayerDataContext.OnSpendSoulCoin += _uiAutoBattle.AutoBattleShopUI.CheckCanBuyCards;

        CardDrawManager.Instance.SetUp(AutoBattleUnitManager.Instance, _uiAutoBattle.AutoBattleShopUI);

        await UniTask.Yield();
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
