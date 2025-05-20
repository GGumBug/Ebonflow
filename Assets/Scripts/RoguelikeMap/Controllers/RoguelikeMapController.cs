using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeMap
{
    /// <summary>
    /// 맵 노드 클릭과 활성화/비활성화 로직을 처리하는 컨트롤러
    /// </summary>
    public class RoguelikeMapController
    {
        // 외부 데이터 제공용 델리게이트
        private readonly Func<int, List<MapNode>> _getNodeRow;
        private readonly Func<int, int, MapNode> _getNode;

        // 외부 구독용 이벤트
        public event Action<Vector2Int> OnCellSelected;
        public event Action OnSaveMap;
        public event Func<Vector2Int> GetCurrentNodePosition;
        public event Func<bool> HasSelection;

        public RoguelikeMapController(MapLayout mapLayout)
        {
            if (mapLayout == null) throw new ArgumentNullException(nameof(mapLayout));
            _getNodeRow = mapLayout.GetNodeRow;
            _getNode = mapLayout.GetNode;
        }

        public void CheckAndActivateFirstRow()
        {
            if (!HasSelection())
            {
                foreach (var node in _getNodeRow(0))
                {
                    node.IsActive = true;
                    foreach (var edge in node.Edges)
                        edge.IsActive = true;
                }
            }
        }

        /// <summary>
        /// 노드 클릭 처리
        /// </summary>
        /// <param name="selectedCellPosition">열(x)과 층(y) 인덱스</param>
        public void SelectNode(Vector2Int selectedCellPosition)
        {
            Vector2Int currentNodePos = GetCurrentNodePosition.Invoke();
            var currentNode = _getNode(currentNodePos.y, currentNodePos.x);

            var row  = selectedCellPosition.y;
            var col  = selectedCellPosition.x;
            var selectedNode = _getNode(row, col);

            DeactivateRow(_getNodeRow(row - 1), _getNodeRow(row), currentNode, selectedNode);

            ActivateEdges(selectedNode.Edges);

            OnCellSelected?.Invoke(selectedCellPosition);

            OnSaveMap?.Invoke();
        }

        private void DeactivateRow(List<MapNode> prevRow, List<MapNode> currentRow, MapNode currentNode, MapNode selectedNode)
        {
            foreach (var node in currentRow)
            {
                node.IsActive = false;
            }

            if (HasSelection())
            {
                foreach (var prevNode in prevRow)
                {
                    foreach (var edge in prevNode.Edges)
                    {
                        edge.IsActive = false;

                        if (prevNode == currentNode && edge.To == selectedNode)
                        {
                            edge.HasPassed = true;
                        }
                    }
                }
            }
            else
            {
                foreach (var firstNode in currentRow)
                {
                    foreach (var edge in firstNode.Edges)
                        edge.IsActive = false;
                }
            }
        }

        private void ActivateEdges(IEnumerable<MapEdge> edges)
        {
            foreach (var edge in edges)
            {
                edge.IsActive = true;
                edge.To.IsActive = true;
            }
        }
    }
}