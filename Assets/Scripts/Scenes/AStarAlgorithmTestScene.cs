using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AStarAlgorithmTestScene : SceneBase
{
    [SerializeField] private TestCharacter startChracter;
    [SerializeField] private TestCharacter targetChracter;
    [SerializeField] private Button btnFindPath;

    [SerializeField] private TestCharacter[] enemies;

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
        btnFindPath.onClick.AddListener(StartDrawPath);

        await UniTask.Yield();
    }

    private void StartDrawPath()
    {
        var hash = GetEnemyHashSet();
        AStarAlgorithmManager.Instance.DrawPath(startChracter, hash, true, true);
    }

    HashSet<IAStarPathPoint> GetEnemyHashSet()
    {
        HashSet<IAStarPathPoint> newHashSet = new HashSet<IAStarPathPoint>();

        foreach (var enemy in enemies)
            newHashSet.Add(enemy);

        return newHashSet;
    }
}
