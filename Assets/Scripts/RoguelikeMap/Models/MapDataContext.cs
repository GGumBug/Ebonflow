using System;
using UnityEngine;

namespace RoguelikeMap
{
    public class MapDataContext : DataContext<MapData>
    {
        private readonly MapDataSerializer _serializer; // 직렬화 특화 기능 사용 용도

        public MapDataContext()
            : base(
                fileName: "MapData",
                serializer: new MapDataSerializer(),
                defaultFactory: () => new MapData
                {
                    currentRow = -1,
                    currentIndex = -1,
                    maxRow = 0,
                    maxCol = 0,
                    nodes = Array.Empty<NodeDataRow>(),
                    edges = Array.Empty<EdgeDataRow>()
                })
        {
            _serializer = (MapDataSerializer)dataSaveLoad;

            _serializer.GetCurrentMapData += () => data;
            _serializer.SetLoadedMapData += (loadData) => data = loadData;
        }

        /// <summary>
        /// 저장된 맵 데이터에 선택 정보가 있는지 여부
        /// </summary>
        public bool HasSelection() => Data != null
                                     && Data.currentRow >= 0
                                     && Data.currentIndex >= 0;

        /// <summary>
        /// MapLayout과 Settings로 내부 데이터 갱신 후 저장
        /// </summary>
        public void InitializeFromLayout(MapLayout layout, MapGenerationSettings settings)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            data = _serializer.FromLayout(layout, settings, data);
            Save();
            Debug.Log($"맵이 저장되었습니다 (ES3) → {Application.persistentDataPath}/Map/{fileName}.json");
        }

        /// <summary>
        /// 현재 선택 좌표 갱신 후 저장
        /// </summary>
        public void UpdateSelection(Vector2Int newPosition, bool saveImmediately = true)
        {
            if (data == null) data = defaultFactory();
            data.currentRow = newPosition.y;
            data.currentIndex = newPosition.x;

            if (saveImmediately)
                Save();
        }

        /// <summary>
        /// 현재 데이터의 선택 좌표 반환 (미로딩/초기값이면 예외)
        /// </summary>
        public Vector2Int GetCurrentNodePosition()
        {
            if (data == null)
                throw new InvalidOperationException("맵 데이터가 로드되지 않았습니다.");
            return new Vector2Int(data.currentIndex, data.currentRow);
        }

        /// <summary>
        /// 저장 파일 삭제(직렬화기 편의 API 필요)
        /// </summary>
        public bool Delete()
        {
            var deleted = _serializer.Delete();
            if (deleted) data = null;
            return deleted;
        }

        public void Save(MapLayout mapLayout, MapGenerationSettings settings) => _serializer.Save(fileName, mapLayout, settings);
        public MapData TryLoadData() => _serializer.TryLoadData(fileName);
        public MapData TryLoadLayout(MapGenerationSettings settings) => _serializer.TryLoadLayout(fileName, settings);
    }
}
