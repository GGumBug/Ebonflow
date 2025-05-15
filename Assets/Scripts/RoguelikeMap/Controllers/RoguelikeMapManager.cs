using UnityEngine;
using RoguelikeMap.UI;

namespace RoguelikeMap
{
    public class RoguelikeMapManager : MonoBehaviour
    {
        private bool _drawGizmo;
        private MapLayout _mapLayout;
        private MapSaveLoad _mapSaveLoad;
        private RoguelikeMapGenerator _mapGenerator;
        private UIMapView _uiMapView;

        public void Setup(MapGenerationSettings settings, UIMapView uiMapView)
        {
            _drawGizmo = settings.isDrawGizmo;
            _mapSaveLoad = new MapSaveLoad();
            _mapLayout = LoadOrGenerateMap("Test", settings);
            
            _uiMapView = uiMapView;
            _uiMapView.RenderMap(_mapLayout);
        }

        private MapLayout LoadOrGenerateMap(string saveKey, MapGenerationSettings settings)
        {
            if (_mapSaveLoad.TryLoadLayout(saveKey, out var layout, settings))
            {
                Debug.Log("저장된 맵 재구축");
                return layout;
            }

            Debug.Log("저장된 맵 없음 → 새로 생성");
            _mapGenerator = new RoguelikeMapGenerator(settings);
            var newLayout = _mapGenerator.CreateMap();
            _mapSaveLoad.Save(saveKey, newLayout, settings);
            return newLayout;
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmo || _mapLayout == null) return;

            var paths = _mapLayout.Paths;

            for (int gen = 0; gen < paths.Count; gen++)
            {
                Color lineColor;
                switch (gen)
                {
                    case 0: lineColor = Color.red; break;
                    case 1: lineColor = Color.magenta; break;
                    case 2: lineColor = Color.yellow; break;
                    case 3: lineColor = Color.blue; break;
                    case 4: lineColor = Color.green; break;
                    case 5: lineColor = Color.cyan; break;
                    default: lineColor = Color.white; break;
                }

                foreach (var edge in paths[gen])
                {
                    // From 노드
                    DrawNodeGizmo(edge.From);
                    // To 노드 (optional: 중복 그리기를 피하고 싶으면 빼셔도 됩니다)
                    // DrawNodeGizmo(edge.To);

                    // 엣지 라인
                    Vector3 a = (Vector2)edge.From.position;
                    Vector3 b = (Vector2)edge.To.position;
                    Gizmos.color = lineColor;
                    Gizmos.DrawLine(a, b);
                }
            }
        }

        private void DrawNodeGizmo(MapNode node)
        {
            // 위치
            Vector3 pos = (Vector2)node.position;

            // LocationType 에 따른 색 결정
            Color nodeColor;
            switch (node.type)
            {
                case LocationType.Monster: nodeColor = Color.gray; break;
                case LocationType.Elite: nodeColor = Color.red; break;
                case LocationType.Camp: nodeColor = Color.green; break;
                case LocationType.Boss: nodeColor = Color.black; break;
                default: nodeColor = Color.white; break;
            }

            Gizmos.color = nodeColor;
            Gizmos.DrawWireSphere(pos, 0.2f);
        }
    }
}