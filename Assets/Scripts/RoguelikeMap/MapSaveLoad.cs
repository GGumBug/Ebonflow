using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace RoguelikeMap
{
    public class MapSaveLoad
    {
        public string FilePath(string fileName) =>
            Path.Combine(Application.persistentDataPath, fileName + ".json");

        public void Save(string fileName, MapLayout mapLayout)
        {
            var data = new MapData
            {
                nodes = new List<NodeData>(),
                edges = new List<EdgeData>()
            };

            // 1) 모든 노드를 flat 리스트에 담고 인덱스 맵 생성
            var indexMap = new Dictionary<MapNode, int>();
            int idx = 0;
            foreach (var row in mapLayout.Grid)
                foreach (var node in row)
                    indexMap[node] = idx++;

            // 2) NodeData 채우기
            foreach (var kv in indexMap)
            {
                var node = kv.Key;
                data.nodes.Add(new NodeData
                {
                    row = (int)node.position.y,
                    col = (int)node.position.x,
                    type = node.type
                });
            }

            // 3) EdgeData 채우기
            foreach (var path in mapLayout.Paths)
                foreach (var edge in path)
                    data.edges.Add(new EdgeData
                    {
                        fromIndex = indexMap[edge.From],
                        toIndex = indexMap[edge.To],
                        generation = edge.Generation
                    });

            // 4) JSON으로 저장
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(FilePath(fileName), json);
            Debug.Log($"Map saved to {FilePath(fileName)}");
        }

        /// <summary>
        /// 원시 MapData를 디스크에서 읽어옵니다.
        /// </summary>
        public bool TryLoadData(string fileName, out MapData data)
        {
            var path = FilePath(fileName);
            if (!File.Exists(path))
            {
                Debug.LogError($"Map file not found: {path}");
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

        /// <summary>
        /// MapData를 실제 MapLayout (노드·엣지 구조)으로 재구성합니다.
        /// </summary>
        public MapLayout ReconstructLayout(MapData data)
        {
            // 1) 노드 리스트 생성
            var allNodes = new List<MapNode>(data.nodes.Count);
            foreach (var nd in data.nodes)
            {
                var node = new MapNode(nd.row, nd.col, nd.type);
                allNodes.Add(node);
            }

            // 2) 인덱스 맵 재구축
            var indexMap = new Dictionary<int, MapNode>();
            for (int i = 0; i < allNodes.Count; i++)
                indexMap[i] = allNodes[i];

            // 3) 엣지 리스트 생성
            var paths = new List<List<MapEdge>>();
            // – 예시로 모든 path를 하나의 컬렉션으로 두려면 그냥 data.edges 순회
            var edges = new List<MapEdge>();
            foreach (var ed in data.edges)
            {
                var from = indexMap[ed.fromIndex];
                var to = indexMap[ed.toIndex];
                var e = new MapEdge { From = from, To = to, Generation = ed.generation };
                from.Edges.Add(e);
                edges.Add(e);
            }
            paths.Add(edges);

            // 4) 2D 그리드로 재배치 (rowCount, colCount가 필요합니다)
            int maxRow = data.nodes.Max(n => n.row) + 1;
            int maxCol = data.nodes.Max(n => n.col) + 1;
            var grid = new List<List<MapNode>>(maxRow);
            for (int r = 0; r < maxRow; r++)
            {
                var rowList = new List<MapNode>();
                foreach (var node in allNodes)
                    if (node.position.y == r) //Row 맞는지 체크
                        rowList.Add(node);
                grid.Add(rowList);
            }

            return new MapLayout(grid, paths);
        }

        /// <summary>
        /// 파일에서 불러와 즉시 MapLayout으로 복원해 줍니다.
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