using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeMap
{
    public class RoguelikeMapManager : MonoBehaviour
    {
        private MapGrid _grid;
        private RoguelikeMapGenerator _mapGenerator;

        public void Setup(int row, int col)
        {
            _mapGenerator = new RoguelikeMapGenerator();
            _grid = new MapGrid(_mapGenerator.CreateMap(row, col));
        }

        private void OnDrawGizmos()
        {
            if (_grid == null || _grid.GetMap == null) return;

            // MapGrid에서 2D 리스트를 꺼내는 API라고 가정
            List<List<MapNode>> layout = _grid.GetMap;

            for (int r = 0; r < layout.Count; r++)
            {
                Color LineColor = Color.white;
                switch (r)
                {
                    case 0:
                    LineColor = Color.red;
                    break;
                    case 1:
                    LineColor = Color.magenta;
                    break;
                    case 2:
                    LineColor = Color.yellow;
                    break;
                    case 3:
                    LineColor = Color.blue;
                    break;
                    case 4:
                    LineColor = Color.green;
                    break;
                    case 5:
                    LineColor = Color.cyan;
                    break;
                }

                for (int c = 0; c < layout[r].Count; c++)
                {
                    MapNode node = layout[r][c];
                    Vector3 pos = new Vector3(node.position.x, node.position.y, 0);

                    // 노드 위치에 작은 원을 그린다
                    Gizmos.color = Color.gray;
                    Gizmos.DrawWireSphere(pos, 0.2f);

                    // 그 노드에서 뻗어나간 엣지(경로)들을 연결선으로 그린다
                    Gizmos.color = LineColor;
                    foreach (var edge in node.Edges)
                    {
                        Vector3 toPos = new Vector3(edge.To.position.x, edge.To.position.y, 0);
                        Gizmos.DrawLine(pos, toPos);
                    }
                }
            }
        }
    }
}