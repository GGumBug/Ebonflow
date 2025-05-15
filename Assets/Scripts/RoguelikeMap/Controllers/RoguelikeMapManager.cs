using UnityEngine;
using RoguelikeMap.UI;

namespace RoguelikeMap
{
    public class RoguelikeMapManager : MonoBehaviour
    {
        private bool drawGizmo;
        private MapGenerationSettings settings;
        private MapLayout mapLayout;
        private MapSaveLoad mapSaveLoad;
        private RoguelikeMapGenerator mapGenerator;
        private RoguelikeMapController mapController;
        private UIMapView uiMapView;

        /// <summary>
        /// 맵 생성 설정과 UIMapView를 초기화합니다.
        /// </summary>
        public void Setup(MapGenerationSettings settings, UIMapView uiMapView)
        {
            Debug.Assert(settings != null, "settings must not be null.");
            Debug.Assert(uiMapView != null, "uiMapView must not be null.");

            this.settings = settings;
            drawGizmo = settings.isDrawGizmo;
            mapSaveLoad = new MapSaveLoad();
            mapLayout = LoadOrGenerateMap("Test", settings);

            mapController = new RoguelikeMapController(mapLayout);
            mapController.OnCellSelected += mapSaveLoad.UpdateSelection;
            mapController.OnSaveMap += SaveMap;

            this.uiMapView = uiMapView;
            uiMapView.RenderMap(mapLayout, mapController.HandleNodeClick);

            if (!mapSaveLoad.HasSelection)
            {
                foreach (var node in mapLayout.Grid[0])
                {
                    node.IsActive = true;
                }
            }
        }

        /// <summary>
        /// 저장된 레이아웃이 있으면 불러오고, 없으면 새로 생성 후 저장합니다.
        /// </summary>
        private MapLayout LoadOrGenerateMap(string saveKey, MapGenerationSettings settings)
        {
            if (mapSaveLoad.TryLoadLayout(saveKey, out var layout, settings))
            {
                Debug.Log("Loaded saved map layout.");
                return layout;
            }

            Debug.Log("No saved layout found. Generating new map.");
            mapGenerator = new RoguelikeMapGenerator(settings);
            var newLayout = mapGenerator.CreateMap();
            mapSaveLoad.Save(saveKey, newLayout, settings);
            return newLayout;
        }

        private void SaveMap()
        {
            mapSaveLoad.Save("Test", mapLayout, settings);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmo || mapLayout == null)
            {
                return;
            }

            var paths = mapLayout.Paths;
            for (var generation = 0; generation < paths.Count; generation++)
            {
                var lineColor = GetGizmoColorByGeneration(generation);
                foreach (var edge in paths[generation])
                {
                    DrawNodeGizmo(edge.From);
                    var start = edge.From.position;
                    var end = edge.To.position;
                    Gizmos.color = lineColor;
                    Gizmos.DrawLine(start, end);
                }
            }
        }

        /// <summary>
        /// 세대별 기즈모 라인 색상을 반환합니다.
        /// </summary>
        private Color GetGizmoColorByGeneration(int generation)
        {
            return generation switch
            {
                0 => Color.red,
                1 => Color.magenta,
                2 => Color.yellow,
                3 => Color.blue,
                4 => Color.green,
                5 => Color.cyan,
                _ => Color.white,
            };
        }

        /// <summary>
        /// 노드 위치와 타입에 따라 색상을 결정하여 원형 기즈모를 그립니다.
        /// </summary>
        private void DrawNodeGizmo(MapNode node)
        {
            var position = node.position;
            var nodeColor = node.type switch
            {
                LocationType.Monster => Color.gray,
                LocationType.Elite => Color.red,
                LocationType.Camp => Color.green,
                LocationType.Boss => Color.black,
                _ => Color.white,
            };

            Gizmos.color = nodeColor;
            Gizmos.DrawWireSphere(position, 0.2f);
        }
    }
}