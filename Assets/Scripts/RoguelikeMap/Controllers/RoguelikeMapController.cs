using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeMap
{
    public class RoguelikeMapController
    {
        private Func<List<List<MapNode>>> getGrid;
        private Func<List<List<MapEdge>>> getPaths;

        public event Action<Vector2Int> OnCellSelected;
        public event Action OnSaveMap;

        public RoguelikeMapController(MapLayout mapLayout)
        {
            getGrid = () => mapLayout.Grid;
            getPaths = () => mapLayout.Paths;
        }

        private MapNode GetMapNode(Vector2Int cellPosition)
            => getGrid()[cellPosition.y][cellPosition.x];

        /// <summary>
        /// 노드 클릭 처리
        /// </summary>
        /// <param name="cellPosition">열(x)과 층(y) 인덱스</param>
        public void HandleNodeClick(Vector2Int cellPosition, LocationType locationType)
        {
            // 1) 선택 정보 통보
            OnCellSelected?.Invoke(cellPosition);

            var grid = getGrid();
            var row = cellPosition.y;
            var col = cellPosition.x;
            var currentNode = grid[row][col];

            // 2) 비활성화 로직
            DeactivateRow(grid[cellPosition.y]);

            // 3) 연결된 엣지의 대상 노드들 활성화
            ActivateEdges(currentNode.Edges);

            OnSaveMap?.Invoke();
        }

        private void DeactivateRow(List<MapNode> row)
        {
            foreach (var node in row)
                node.IsActive = false;
        }

        private void ActivateEdges(IEnumerable<MapEdge> edges)
        {
            foreach (var edge in edges)
                edge.To.IsActive = true;
        }
    }
}