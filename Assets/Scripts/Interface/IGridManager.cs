using AutoBattle;
using AutoBattle.Input;
using UnityEngine;

public interface IGridManager
{
    GridType Type { get; }
    bool IsValidCell(Vector2Int cell);
    bool IsCellOccupied(Vector2Int cell);
    void PlaceUnit(IUnitDraggable draggable, Vector2Int cell);
    void RemoveUnit(IUnitDraggable draggable);
}