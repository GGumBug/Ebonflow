using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AStarAlgorithmTestScene : SceneBase, IAStarGridSettings
{
    [SerializeField] private Button _btnFindPath;
    [SerializeField] private Vector2Int _gridTopRight;
    [SerializeField] private Vector2Int _gridBottomLeft;

    public Vector2Int GridTopRight => _gridTopRight;
    public Vector2Int GridBottomLeft => _gridBottomLeft;


    public override UniTask FinalizeLoading()
    {
        throw new System.NotImplementedException();
    }

    public override UniTask InitializeData()
    {
        throw new System.NotImplementedException();
    }

    public override UniTask LoadAssets()
    {
        throw new System.NotImplementedException();
    }

    public override UniTask SetupScene()
    {
        throw new System.NotImplementedException();
    }

    public override async UniTask DebugMode()
    {
        AStarAlgorithmManager.Instance.InitializeGrid(this);
        AutoBattleManager.Instance.Setup();
        await UniTask.Yield();  
    }

    private void Start()
    {
        AutoBattleManager.Instance.StartBattle();
    }
}
