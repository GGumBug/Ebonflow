using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoguelikeMap
{
    public class MapSaveLoad
    {
        private string FilePath(string fileName) =>
            Path.Combine(Application.persistentDataPath, fileName + ".json");

        public void Save(string fileName, MapLayout mapLayout)
        {
            var data = new MapData
            {
                // 행 단위 노드, 세대 단위 엣지를 저장할 배열 초기화
                nodes = new NodeDataRow[mapLayout.Grid.Count],
                edges = new EdgeDataRow[mapLayout.Paths.Count]
            };

            // --- 1) nodes: 행(row) 단위로 NodeDataRow 생성 ---
            for (int r = 0; r < mapLayout.Grid.Count; r++)
            {
                var rowList = mapLayout.Grid[r]
                    .Select(n => new NodeData
                    {
                        row = (int)n.position.y,
                        col = (int)n.position.x,
                        type = n.type
                    })
                    .ToList();

                data.nodes[r] = new NodeDataRow { row = rowList };
            }

            // --- 2) 좌표(row,col) → flat index 매핑 생성 ---
            var indexMap = new Dictionary<(int row, int col), int>();
            int idx = 0;
            for (int r = 0; r < mapLayout.Grid.Count; r++)
            {
                for (int c = 0; c < mapLayout.Grid[r].Count; c++)
                {
                    var node = mapLayout.Grid[r][c];
                    indexMap[((int)node.position.y, (int)node.position.x)] = idx++;
                }
            }

            // --- 3) edges: 세대(generation) 단위로 EdgeDataRow 생성 ---
            for (int g = 0; g < mapLayout.Paths.Count; g++)
            {
                var edgeList = new List<EdgeData>();
                foreach (var e in mapLayout.Paths[g])
                {
                    var fromKey = ((int)e.From.position.y, (int)e.From.position.x);
                    var toKey = ((int)e.To.position.y, (int)e.To.position.x);

                    if (!indexMap.TryGetValue(fromKey, out int fromIdx) ||
                        !indexMap.TryGetValue(toKey, out int toIdx))
                    {
                        Debug.LogWarning($"Save: 좌표 매핑 누락 from={fromKey}, to={toKey}");
                        continue;
                    }

                    edgeList.Add(new EdgeData
                    {
                        fromIndex = fromIdx,
                        toIndex = toIdx,
                        generation = e.Generation
                    });
                }
                data.edges[g] = new EdgeDataRow { path = edgeList };
            }

            // --- 4) JSON 직렬화 & 파일 쓰기 ---
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(FilePath(fileName), json);
            Debug.Log($"Map saved to {FilePath(fileName)}");
        }

        /// <summary>
        /// JSON에서 MapData를 읽어옵니다.
        /// </summary>
        public bool TryLoadData(string fileName, out MapData data)
        {
            var path = FilePath(fileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Map file not found: {path}");
                data = null;
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                data = JsonUtility.FromJson<MapData>(json);
                return data != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load map data from {path}: {ex.Message}");
                data = null;
                return false;
            }
        }

        public MapLayout ReconstructLayout(MapData data)
        {
            // 1) 그리드 + flat allNodes 리스트 동시 생성
            var grid = new List<List<MapNode>>(data.nodes.Length);
            var allNodes = new List<MapNode>();
            for (int r = 0; r < data.nodes.Length; r++)
            {
                var rowList = new List<MapNode>(data.nodes[r].row.Count);
                foreach (var nd in data.nodes[r].row)
                {
                    var node = new MapNode(nd.row, nd.col, nd.type);
                    rowList.Add(node);
                    allNodes.Add(node);       // 저장 시 SelectMany(r => r) 순서와 1:1 매칭
                }
                grid.Add(rowList);
            }

            // 2) flat index → allNodes[index] 로 간선 복원
            var paths = new List<List<MapEdge>>(data.edges.Length);
            for (int gen = 0; gen < data.edges.Length; gen++)
            {
                var edgeList = new List<MapEdge>();
                foreach (var ed in data.edges[gen].path)
                {
                    var from = allNodes[ed.fromIndex];
                    var to = allNodes[ed.toIndex];

                    var e = new MapEdge
                    {
                        From = from,
                        To = to,
                        Generation = ed.generation
                    };
                    from.Edges.Add(e);
                    edgeList.Add(e);
                }
                paths.Add(edgeList);
            }

            return new MapLayout(grid, paths);
        }

        /// <summary>
        /// 데이터 로드 후 곧바로 MapLayout을 반환합니다.
        /// </summary>
        public bool TryLoadLayout(string fileName, out MapLayout layout)
        {
            layout = null;
            if (!TryLoadData(fileName, out var data))
                return false;

            layout = ReconstructLayout(data);
            Debug.Log($"Map layout reconstructed from {FilePath(fileName)}");
            return true;
        }
    }
}