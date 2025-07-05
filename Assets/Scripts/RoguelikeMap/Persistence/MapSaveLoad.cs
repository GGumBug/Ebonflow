using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoguelikeMap
{
    /// <summary>
    /// MapSaveLoad를 ES3SerializerBase<MapData>로 리팩터링한 예시.
    /// - 파일 이름을 동적으로 설정하기 위해 _currentFileName 필드를 사용.
    /// - RelativePath는 적절히 설정(예: "Maps" 폴더).
    /// - Key는 MapData를 고유하게 식별할 문자열로 지정.
    /// </summary>
    public class MapSaveLoad : ES3SerializerBase<MapData>
    {
        // =================================================================================
        // 1) ES3SerializerBase 구현을 위해 반드시 오버라이드해야 할 추상 프로퍼티들
        // =================================================================================

        /// <summary>
        /// Application.persistentDataPath 밑의 상대 폴더 경로
        /// (예: "Maps" 폴더를 만들어서 그 아래에 저장).
        /// 필요에 따라 빈 문자열("")로 두어 바로 persistentDataPath에 저장할 수도 있습니다.
        /// </summary>
        protected override string RelativePath => "Map";

        // =================================================================================
        // 2) MapSaveLoad 고유 멤버
        // =================================================================================

        // 현재 작업 중인 MapData 인스턴스
        private MapData _mapData;

        /// <summary>
        /// 생성자: ES3SerializerBase의 DI 생성자를 그대로 호출할 수 있습니다.
        /// </summary>
        public MapSaveLoad(
            string basePath = null,
            ES3Settings settings = null,
            ILogger logger = null
        ) : base(basePath, settings, logger)
        {
            _mapData = null;
            _currentFileName = null;
        }

        /// <summary>
        /// 저장된 맵 데이터에 선택 정보가 있는지 여부
        /// </summary>
        public bool HasSelection() => _mapData != null
                                     && _mapData.currentRow >= 0
                                     && _mapData.currentIndex >= 0;

        /// <summary>
        /// 선택된 노드 좌표 반환
        /// </summary>
        public Vector2Int GetCurrentNodePosition()
        {
            if (_mapData == null)
                throw new InvalidOperationException("맵 데이터가 로드되지 않았습니다.");
            return new Vector2Int(_mapData.currentIndex, _mapData.currentRow);
        }

        /// <summary>
        /// MapLayout 정보를 ES3를 통해 직렬화하여 저장합니다.
        /// (파일 확장자 .json은 FileName 프로퍼티에서 자동으로 붙습니다.)
        /// </summary>
        public void Save(string fileName, MapLayout mapLayout, MapGenerationSettings settings)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (mapLayout == null)
                throw new ArgumentNullException(nameof(mapLayout));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            // 1) 현재 파일명을 세팅
            _currentFileName = fileName;

            // 2) _mapData 초기화 및 노드/엣지 데이터 채우기
            InitializeMapDataIfNeeded(mapLayout, settings);
            PopulateNodeDataRows(mapLayout);
            PopulateEdgeDataRows(mapLayout);

            // 3) ES3SerializerBase.Save 호출
            bool success = base.Save(_mapData);
            if (success)
                Debug.Log($"맵이 저장되었습니다 (ES3) → {Path.Combine(Application.persistentDataPath, RelativePath, FileName)}");
            else
                Debug.LogError("맵 저장에 실패했습니다.");
        }

        /// <summary>
        /// 저장된 ES3 파일을 삭제합니다.
        /// </summary>
        public void DeleteSave(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            _currentFileName = fileName;
            bool deleted = base.Delete();
            if (deleted)
            {
                _mapData = null;
                Debug.Log($"맵 저장 파일이 삭제되었습니다 (ES3) → {Path.Combine(Application.persistentDataPath, RelativePath, FileName)}");
            }
            else
            {
                Debug.LogWarning($"삭제할 맵 파일이 없습니다 (ES3) → {Path.Combine(Application.persistentDataPath, RelativePath, FileName)}");
            }
        }

        /// <summary>
        /// ES3에서 MapData를 로드한 뒤, out 파라미터로 반환합니다.
        /// 성공 여부는 bool 리턴값으로 알 수 있습니다.
        /// </summary>
        public MapData TryLoadData(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            _currentFileName = fileName;
            var loaded = base.Load();
            if (loaded == null)
            {
                Debug.LogWarning($"맵 파일을 찾을 수 없습니다 (ES3) → {Path.Combine(Application.persistentDataPath, RelativePath, FileName)}");
                return null;
            }
            _mapData = loaded;
            return loaded;
        }

        /// <summary>
        /// ES3에서 로드한 데이터를 활용해 MapLayout을 복원합니다.
        /// 저장된 정보가 없으면 false를 반환합니다.
        /// (기존 TryLoadData 로직과 동일하게 동작)
        /// </summary>
        public MapData TryLoadLayout(string fileName, MapGenerationSettings settings)
        {
            var data = TryLoadData(fileName);

            if (data == null)
                return null;

            Debug.Log($"맵 레이아웃이 복원되었습니다 (ES3) → {Path.Combine(Application.persistentDataPath, RelativePath, FileName)}");
            return data;
        }

        /// <summary>
        /// 선택된 노드 좌표를 내부 MapData에 업데이트합니다.
        /// </summary>
        public void UpdateSelection(Vector2Int newPosition)
        {
            if (_mapData == null)
                _mapData = new MapData();

            _mapData.currentRow = newPosition.y;
            _mapData.currentIndex = newPosition.x;
        }

        #region 내부 헬퍼 메서드 (원본과 동일)

        private void InitializeMapDataIfNeeded(MapLayout layout, MapGenerationSettings settings)
        {
            if (_mapData == null)
            {
                _mapData = new MapData
                {
                    currentRow = -1,
                    currentIndex = -1,
                    maxRow = settings.rowCount,
                    maxCol = settings.colCount,
                    nodes = new NodeDataRow[layout.Grid.Count],
                    edges = new EdgeDataRow[layout.Paths.Count]
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
                        row = (int)n.position.y,
                        col = (int)n.position.x,
                        type = n.type,
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
                    var toKey = ((int)e.To.position.y, (int)e.To.position.x);

                    if (!indexMap.TryGetValue(fromKey, out int fIdx)
                     || !indexMap.TryGetValue(toKey, out int tIdx))
                    {
                        Debug.LogWarning($"저장 중: 매핑 누락 from={fromKey}, to={toKey}");
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

                _mapData.edges[g] = new EdgeDataRow { path = list };
            }
        }

        #endregion
    }
}
