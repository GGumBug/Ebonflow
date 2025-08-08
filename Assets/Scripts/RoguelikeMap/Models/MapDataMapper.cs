using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoguelikeMap
{
    public class MapDataMapper
    {
        public MapData InitializeMapDataIfNeeded(MapData mapData, MapLayout layout, MapGenerationSettings settings)
        {
            if (mapData == null)
            {
                mapData = new MapData();
            }

            if (mapData.nodes == null || mapData.nodes.Length != layout.Grid.Count)
            {
                mapData.nodes = new NodeDataRow[layout.Grid.Count];
            }
            if (mapData.edges == null || mapData.edges.Length != layout.Paths.Count)
            {
                mapData.edges = new EdgeDataRow[layout.Paths.Count];
            }

            mapData.currentRow = mapData.currentRow < 0 ? -1 : mapData.currentRow;
            mapData.currentIndex = mapData.currentIndex < 0 ? -1 : mapData.currentIndex;
            mapData.maxRow = settings.rowCount;
            mapData.maxCol = settings.colCount;

            return mapData;
        }


        public MapData FromLayout(MapLayout layout, MapGenerationSettings settings, MapData reuse = null)
        {
            var mapData = reuse ?? new MapData();
            if (mapData.nodes == null || mapData.nodes.Length != layout.Grid.Count)
            {
                mapData.nodes = new NodeDataRow[layout.Grid.Count];
            }
            if (mapData.edges == null || mapData.edges.Length != layout.Paths.Count)
            {
                mapData.edges = new EdgeDataRow[layout.Paths.Count];
            }

            mapData.currentRow = mapData.currentRow < 0 ? -1 : mapData.currentRow;
            mapData.currentIndex = mapData.currentIndex < 0 ? -1 : mapData.currentIndex;
            mapData.maxRow = settings.rowCount;
            mapData.maxCol = settings.colCount;

            PopulateNodeDataRows(layout, mapData);
            PopulateEdgeDataRows(layout, mapData);
            return mapData;
        }

        public void PopulateNodeDataRows(MapLayout layout, MapData mapData)
        {
            if (mapData.nodes == null || mapData.nodes.Length != layout.Grid.Count)
            {
                mapData.nodes = new NodeDataRow[layout.Grid.Count];
            }

            for (int r = 0; r < layout.Grid.Count; r++)
            {
                var list = layout.Grid[r]
                    .Select(n => new NodeData
                    {
                        row = (int)n.position.y,
                        col = (int)n.position.x,
                        type = n.type,
                        isActive = n.IsActive
                    })
                    .ToList();

                mapData.nodes[r] = new NodeDataRow { row = list };
            }
        }


        public void PopulateEdgeDataRows(MapLayout layout, MapData mapData)
        {
            var flat = layout.Grid.SelectMany(r => r).ToList();
            var indexMap = flat
                .Select((node, idx) => new { node, idx })
                .ToDictionary(x => ((int)x.node.position.y, (int)x.node.position.x), x => x.idx);

            for (int g = 0; g < layout.Paths.Count; g++)
            {
                var list = new List<EdgeData>();
                foreach (var e in layout.Paths[g])
                {
                    var fromKey = ((int)e.From.position.y, (int)e.From.position.x);
                    var toKey = ((int)e.To.position.y, (int)e.To.position.x);

                    if (!indexMap.TryGetValue(fromKey, out int fIdx)
                        || !indexMap.TryGetValue(toKey, out int tIdx))
                    {
                        Debug.unityLogger?.LogWarning(nameof(MapDataMapper),
                            $"저장 중: 매핑 누락 from={fromKey}, to={toKey}");
                        continue;
                    }

                    list.Add(new EdgeData
                    {
                        fromIndex = fIdx,
                        toIndex = tIdx,
                        generation = e.Generation,
                        isActive = e.IsActive,
                        hasPassed = e.HasPassed
                    });
                }

                mapData.edges[g] = new EdgeDataRow { path = list };
            }
        }
    }
}
