using UnityEngine;
using RoguelikeMap.UI;

namespace RoguelikeMap
{
    public class RoguelikeMapManager : MonoBehaviour
    {
        private const string MapDataFileName = "MapData";

        private MapGenerationSettings _settings;
        private bool _drawGizmo;
        private MapLayout _mapLayout;
        private MapSaveLoad _mapSaveLoad;
        private RoguelikeMapGenerator _mapGenerator;
        private RoguelikeMapController _mapController;
        private UIMapView _uiMapView;

        /// <summary>
        /// 맵 생성 설정과 UIMapView를 초기화합니다.
        /// </summary>
        public void Setup(MapGenerationSettings settings, UIMapView uiMapView, INodeClickHandler nodeClickHandler)
        {
            Debug.Assert(settings != null, "settings must not be null.");
            Debug.Assert(uiMapView != null, "uiMapView must not be null.");

            _settings   = settings;
            _drawGizmo  = _settings.isDrawGizmo;
            _mapSaveLoad = new MapSaveLoad();
            _mapLayout  = LoadOrGenerateMap(MapDataFileName, _settings);

            _mapController = new RoguelikeMapController(_mapLayout);
            _mapController.OnCellSelected += _mapSaveLoad.UpdateSelection;
            _mapController.OnSaveMap      += SaveMap;
            _mapController.GetCurrentNodePosition += _mapSaveLoad.GetCurrentNodePosition;
            _mapController.HasSelection += _mapSaveLoad.HasSelection;

            _uiMapView = uiMapView;
            _uiMapView.RenderMap(_mapLayout, _mapController.HandleNodeClick);

            _mapController.CheckAndActivateFirstRow();
        }

        /// <summary>
        /// 저장된 레이아웃이 있으면 불러오고, 없으면 새로 생성 후 저장합니다.
        /// </summary>
        private MapLayout LoadOrGenerateMap(string saveKey, MapGenerationSettings settings)
        {
            if (_mapSaveLoad.TryLoadLayout(saveKey, out var layout, settings))
            {
                Debug.Log("저장된 맵 레이아웃을 불러왔습니다.");
                return layout;
            }

            Debug.Log("저장된 맵 레이아웃이 없어 새로 생성합니다.");
            _mapGenerator = new RoguelikeMapGenerator(settings);
            var newLayout = _mapGenerator.CreateMap();
            _mapSaveLoad.Save(saveKey, newLayout, settings);
            return newLayout;
        }

        /// <summary>
        /// 현재 맵 레이아웃을 저장합니다.
        /// </summary>
        private void SaveMap()
        {
            _mapSaveLoad.Save(MapDataFileName, _mapLayout, _settings);
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmo || _mapLayout == null)
                return;

            var paths = _mapLayout.Paths;
            for (int generation = 0; generation < paths.Count; generation++)
            {
                var lineColor = GetGizmoColorByGeneration(generation);
                foreach (var edge in paths[generation])
                {
                    DrawNodeGizmo(edge.From);
                    var start = edge.From.position;
                    var end   = edge.To.position;
                    Gizmos.color = lineColor;
                    Gizmos.DrawLine(start, end);
                }
            }
        }

        /// <summary>
        /// 세대별 기즈모 라인 색상을 반환합니다.
        /// </summary>
        private Color GetGizmoColorByGeneration(int generation) => generation switch
        {
            0 => Color.red,
            1 => Color.magenta,
            2 => Color.yellow,
            3 => Color.blue,
            4 => Color.green,
            5 => Color.cyan,
            _ => Color.white,
        };

        /// <summary>
        /// 노드 위치와 타입에 따라 색상을 결정하여 원형 기즈모를 그립니다.
        /// </summary>
        private void DrawNodeGizmo(MapNode node)
        {
            var position = node.position;
            var nodeColor = node.type switch
            {
                LocationType.Monster => Color.gray,
                LocationType.Elite   => Color.red,
                LocationType.Camp    => Color.green,
                LocationType.Boss    => Color.black,
                _                    => Color.white,
            };

            Gizmos.color = nodeColor;
            Gizmos.DrawWireSphere(position, 0.2f);
        }
    }
}