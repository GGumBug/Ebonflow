using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoguelikeMap
{
    /// <summary>
    /// 맵 데이터를 JSON으로 저장하고 불러오는 기능을 제공합니다.
    /// </summary>
    public class MapSaveLoad
    {
        private MapData _mapData;

        /// <summary>
        /// 저장된 맵 데이터에 선택 정보가 있는지 여부
        /// </summary>
        public bool HasSelection() => _mapData != null
                                     && _mapData.currentRow >= 0
                                     && _mapData.currentIndex >= 0;

        /// <summary>
        /// 파일명에 대응하는 전체 경로를 반환합니다.
        /// </summary>
        private string GetFilePath(string fileName)
            => Path.Combine(Application.persistentDataPath, fileName + ".json");

        public Vector2Int GetCurrentNodePosition() => new Vector2Int(_mapData.currentIndex, _mapData.currentRow);

        /// <summary>
        /// MapLayout 정보를 JSON으로 직렬화하여 파일로 저장합니다.
        /// </summary>
        public void Save(string fileName, MapLayout mapLayout, MapGenerationSettings settings)
        {
            if (mapLayout == null) throw new ArgumentNullException(nameof(mapLayout));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            InitializeMapDataIfNeeded(mapLayout, settings);
            PopulateNodeDataRows(mapLayout);
            PopulateEdgeDataRows(mapLayout);

            string json = JsonUtility.ToJson(_mapData, prettyPrint: true);
            File.WriteAllText(GetFilePath(fileName), json);
            Debug.Log($"맵이 저장되었습니다: {GetFilePath(fileName)}");
        }

        /// <summary>
        /// 저장된 JSON 파일을 삭제합니다.
        /// </summary>
        public void DeleteSave(string fileName)
        {
            string path = GetFilePath(fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                _mapData = null;
                Debug.Log($"맵 저장 파일이 삭제되었습니다: {path}");
            }
            else
            {
                Debug.LogWarning($"삭제할 맵 파일이 없습니다: {path}");
            }
        }

        /// <summary>
        /// 파일에서 JSON을 읽어 MapData로 역직렬화 시도합니다.
        /// </summary>
        public bool TryLoadData(string fileName, out MapData data)
        {
            string path = GetFilePath(fileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"맵 파일을 찾을 수 없습니다: {path}");
                data = null;
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<MapData>(json);
                _mapData = data;
                return data != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"맵 데이터 로드에 실패했습니다: {path}\n{ex.Message}");
                data = null;
                return false;
            }
        }

        /// <summary>
        /// 파일에서 MapLayout을 불러옵니다. 저장된 정보가 없으면 false를 반환합니다.
        /// </summary>
        public bool TryLoadLayout(string fileName, out MapData data, MapGenerationSettings settings)
        {
            if (!TryLoadData(fileName, out data)) 
                return false;

            Debug.Log($"맵 레이아웃이 복원되었습니다: {GetFilePath(fileName)}");
            return true;
        }

        /// <summary>
        /// 선택된 노드 좌표를 내부 MapData에 업데이트합니다.
        /// </summary>
        public void UpdateSelection(Vector2Int newPosition)
        {
            if (_mapData == null)
                _mapData = new MapData();

            _mapData.currentRow   = newPosition.y;
            _mapData.currentIndex = newPosition.x;
        }

        #region 내부 헬퍼 메서드

        private void InitializeMapDataIfNeeded(MapLayout layout, MapGenerationSettings settings)
        {
            if (_mapData == null)
            {
                _mapData = new MapData
                {
                    currentRow   = -1,
                    currentIndex = -1,
                    maxRow        = settings.rowCount,
                    maxCol        = settings.colCount,
                    nodes         = new NodeDataRow[layout.Grid.Count],
                    edges         = new EdgeDataRow[layout.Paths.Count]
                };
            }
        }

        private void PopulateNodeDataRows(MapLayout layout)
        {
            for (int r = 0; r < layout.Grid.Count; r++)
            {
                var list = layout.Grid[r]
                    .Select(n => new NodeData
                    {
                        row      = (int)n.position.y,
                        col      = (int)n.position.x,
                        type     = n.type,
                        isActive = n.IsActive
                    })
                    .ToList();

                _mapData.nodes[r] = new NodeDataRow { row = list };
            }
        }

        private void PopulateEdgeDataRows(MapLayout layout)
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
                    var toKey   = ((int)e.To.position.y,   (int)e.To.position.x);

                    if (!indexMap.TryGetValue(fromKey, out int fIdx)
                     || !indexMap.TryGetValue(toKey,   out int tIdx))
                    {
                        Debug.LogWarning($"저장 중: 매핑 누락 from={fromKey}, to={toKey}");
                        continue;
                    }

                    list.Add(new EdgeData
                    {
                        fromIndex  = fIdx,
                        toIndex    = tIdx,
                        generation = e.Generation,
                        isActive = e.IsActive,
                        hasPassed = e.HasPassed
                    });
                }

                _mapData.edges[g] = new EdgeDataRow { path = list };
            }
        }

        #endregion
    }
}
