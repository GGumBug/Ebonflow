using UnityEngine;

public interface IUnitSpawner
{
    Unit Spawn(int unitId, int starLevel, TeamType team, Vector2Int position);
}

public class UnitSpawner : IUnitSpawner
{
    readonly GameObject _prefab;
    readonly Transform  _container;

    public UnitSpawner(GameObject prefab, Transform container)
    {        
        _prefab = prefab;
        _container = container;
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