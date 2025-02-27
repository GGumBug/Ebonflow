using Cysharp.Threading.Tasks;
using UnityEngine;

public class AStarAlgorithmTestScene : SceneBase
{
    [SerializeField] private TestCharacter startChracter;
    [SerializeField] private TestCharacter targetChracter;

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
        AStarAlgorithmManager.Instance.CreateGridFromTilemap(new Vector2Int(6, 6), new Vector2Int(0, 0));
        AStarAlgorithmManager.Instance.PathFinding(startChracter, targetChracter, true, true);

        await UniTask.Yield();
    }
}
