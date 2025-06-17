using UnityEngine;
using RoguelikeMap.UI;

namespace RoguelikeMap
{
    public class RoguelikeMapDirector : MonoBehaviour
    {
        private bool _drawGizmo;
        private UIMapView _uiMapView;
        private MapSaveLoadManager _mapSaveLoadManager;

        public RoguelikeMapController MapController { get; private set; }

        /// <summary>
        /// 맵 생성 설정과 UIMapView를 초기화합니다.
        /// </summary>
        public void Setup(UIMapView uiMapView)
        {
            Debug.Assert(uiMapView != null, "uiMapView must not be null.");
            _mapSaveLoadManager = MapSaveLoadManager.Instance;

            _drawGizmo  = _mapSaveLoadManager.Settings.isDrawGizmo;
            
            MapController = new RoguelikeMapController();
            _uiMapView = uiMapView;
        }

        public void InitializeMapView()
        {
            MapController.Setup(_mapSaveLoadManager.MapLayout);
            _uiMapView.RenderMap(_mapSaveLoadManager.MapLayout, MapController.SelectNode);
            MapController.CheckAndActivateFirstRow();
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmo || _mapSaveLoadManager.MapLayout == null)
                return;

            var paths = _mapSaveLoadManager.MapLayout.Paths;
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