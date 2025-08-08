using UnityEngine;
using System.IO;
using System;

namespace RoguelikeMap
{
    public class MapDataSerializer : ES3SerializerBase<MapData>
    {
        protected override string RelativePath => "Map";
        private readonly MapDataMapper _mapper = new();

        public event Func<MapData> GetCurrentMapData;
        public event Action<MapData> SetLoadedMapData;

        public MapDataSerializer(
            string basePath = null,
            ES3Settings settings = null,
            ILogger logger = null
        ) : base(basePath, settings, logger) {}

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

            var current = GetCurrentMapData?.Invoke();
            current = _mapper.InitializeMapDataIfNeeded(current, mapLayout, settings);
            _mapper.PopulateNodeDataRows(mapLayout, current);
            _mapper.PopulateEdgeDataRows(mapLayout, current);

            bool success = base.Save(current, fileName);
            if (success)
                Debug.Log($"맵이 저장되었습니다 (ES3) → {Path.Combine(Application.persistentDataPath, RelativePath, FileName)}");
            else
                Debug.LogError("맵 저장에 실패했습니다.");
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

            var loaded = base.Load(fileName);
            if (loaded == null)
            {
                Debug.LogWarning($"맵 파일을 찾을 수 없습니다 (ES3) → {Path.Combine(Application.persistentDataPath, RelativePath, FileName)}");
                return null;
            }
            SetLoadedMapData?.Invoke(loaded);
            return loaded;
        }

        public MapData TryLoadLayout(string fileName, MapGenerationSettings settings)
        {
            var data = TryLoadData(fileName);

            if (data == null)
                return null;

            Debug.Log($"맵 레이아웃이 복원되었습니다 (ES3) → {Path.Combine(Application.persistentDataPath, RelativePath, FileName)}");
            return data;
        }

        public MapData FromLayout(MapLayout layout, MapGenerationSettings settings, MapData reuse = null) => _mapper.FromLayout(layout, settings, reuse);
    }
}
