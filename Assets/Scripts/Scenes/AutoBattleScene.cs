using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using AutoBattle;

public class AutoBattleScene : SceneBase, IAStarGridSettings
{
    [SerializeField] private Button _btnStartBattle;
    [SerializeField] private Vector2Int _gridTopRight;
    [SerializeField] private Vector2Int _gridBottomLeft;

    public Vector2Int GridTopRight => _gridTopRight;
    public Vector2Int GridBottomLeft => _gridBottomLeft;


    public override async UniTask FinalizeLoading()
    {
        await UniTask.Yield();
    }

    public override async UniTask InitializeData()
    {
        await UniTask.Yield();
    }

    public override async UniTask LoadAssets()
    {
        await AutoBattleUnitManager.Instance.LoadAsset();
    }

    public override async UniTask SetupScene()
    {
        AStarAlgorithmManager.Instance.InitializeGrid(this);
        AutoBattleUnitManager.Instance.Setup();
        AutoBattleManager.Instance.Setup();

        _btnStartBattle.onClick.AddListener(AutoBattleManager.Instance.StartBattle);

        await UniTask.Yield();
    }
}
