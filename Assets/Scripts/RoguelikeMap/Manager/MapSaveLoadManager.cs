using UnityEngine;

namespace RoguelikeMap
{
    public class MapSaveLoadManager : Singleton<MapSaveLoadManager>, IDonDestroy
    {
        private MapDataContext _mapDataContext;
        private RoguelikeMapGenerator _mapGenerator;

        public MapGenerationSettings Settings { get; private set; }
        public MapLayout MapLayout { get; private set; }

        public void Init(MapGenerationSettings settings)
        {
            Settings = settings;
        }

        public void Setup(RoguelikeMapDirector roguelikeMapManager)
        {
            _mapDataContext = new MapDataContext();

            roguelikeMapManager.MapController.OnCellSelected += _mapDataContext.UpdateSelection;
            roguelikeMapManager.MapController.GetCurrentNodePosition += _mapDataContext.GetCurrentNodePosition;
            roguelikeMapManager.MapController.HasSelection += _mapDataContext.HasSelection;

            MapLayout = LoadOrGenerateMap();
        }

        /// <summary>
        /// 저장된 레이아웃이 있으면 불러오고, 없으면 새로 생성 후 저장합니다.
        /// </summary>
        private MapLayout LoadOrGenerateMap()
        {
            _mapGenerator = new RoguelikeMapGenerator(Settings);

            var data = _mapDataContext.TryLoadLayout(Settings);

            if (data != null)
            {
                Debug.Log("저장된 맵 레이아웃을 불러왔습니다.");
                return _mapGenerator.ReconstructLayout(data, Settings);
            }
            else
            {
                Debug.Log("저장된 맵 레이아웃이 없어 새로 생성합니다.");
                var newLayout = _mapGenerator.CreateMap();
                _mapDataContext.Save(newLayout, Settings);
                return newLayout;
            }
        }

        public void SaveMap()
        {
            _mapDataContext.Save(MapLayout, Settings);
        }
    }
}