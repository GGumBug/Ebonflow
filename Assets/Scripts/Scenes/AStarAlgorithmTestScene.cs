using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AStarAlgorithmTestScene : SceneBase, IAStarGridSettings
{
    [SerializeField] private AStarAgent _startChracter;
    [SerializeField] private Button _btnFindPath;
    [SerializeField] private Vector2Int _gridTopRight;
    [SerializeField] private Vector2Int _gridBottomLeft;
    [SerializeField] private AStarAgent[] _enemies;

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
        AStarAlgorithmManager.Instance.CreateGridFromTilemap(this);
        _btnFindPath.onClick.AddListener(StartDrawPath);

        await UniTask.Yield();
    }

    private void StartDrawPath()
    {
        var hash = GetEnemyHashSet();
        AStarAlgorithmManager.Instance.DrawPath(_startChracter, hash, true, true);
    }

    HashSet<IAStarPathPoint> GetEnemyHashSet()
    {
        HashSet<IAStarPathPoint> newHashSet = new HashSet<IAStarPathPoint>();

        foreach (var enemy in _enemies)
            newHashSet.Add(enemy);

        return newHashSet;
    }
}
