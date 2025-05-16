using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeMap
{
    public class MapLayout
    {
        public int MaxRow { get; }
        public int MaxColumn { get; }
        public List<List<MapNode>> Grid { get; }
        public List<List<MapEdge>> Paths { get; }

        public MapLayout(
            int maxRow,
            int maxColumn,
            List<List<MapNode>> grid,
            List<List<MapEdge>> paths)
        {
            MaxRow = maxRow;
            MaxColumn = maxColumn;
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Paths = paths ?? throw new ArgumentNullException(nameof(paths));
        }

        public List<MapNode> GetNodeRow(int row)
        {
            if (row < 0 || row >= Grid.Count)
            {
                Debug.LogWarning($"rowIndex {row}가 유효 범위(0~{Grid.Count - 1})를 벗어났습니다.");
                return null;
            }

            return Grid[row];
        }

        public MapNode GetNode(int rowIndex, int columnIndex)
        {
            var row = GetNodeRow(rowIndex);

            if (row == null)
                return null;

            if (columnIndex < 0 || columnIndex >= row.Count)
            {
                Debug.LogWarning($"columnIndex {columnIndex}가 유효 범위(0~{row.Count - 1})를 벗어났습니다.");
                return null;
            }
            
            return row[columnIndex];
        }
    }
}