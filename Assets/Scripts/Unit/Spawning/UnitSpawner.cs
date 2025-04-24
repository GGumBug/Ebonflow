using UnityEngine;

public interface IUnitSpawner
{
    Unit Spawn(int unitId, int starLevel, TeamType team, Vector2Int position);
}

public class UnitSpawner : IUnitSpawner
{
    private readonly GameObject _prefab;
    private readonly Transform  _container;
    private IUnitStatRepository _statRepository;

    public UnitSpawner(GameObject prefab, Transform container)
    {        
        _prefab = prefab;
        _container = container;
        _statRepository = new UnitStatRepository();
    }

    public Unit Spawn(int unitId, int starLevel, TeamType team, Vector2Int pos)
    {
        Vector3 spawnPos = new Vector3(pos.x, pos.y, 0);
        GameObject go = Object.Instantiate(_prefab, spawnPos, Quaternion.identity, _container);
        var unit = go.GetComponent<Unit>();
        unit.Setup(unitId, starLevel, team);
        return unit;
    }
}