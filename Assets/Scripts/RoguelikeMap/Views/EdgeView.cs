using UnityEngine;

namespace RoguelikeMap.UI
{
    [RequireComponent(typeof(LineRenderer))]
    public class EdgeView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _line;
        
        private Canvas _canvas; // Screen Space - Overlay 또는 Camera

        void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();

            _line.startWidth = 0.05f;
            _line.endWidth   = 0.05f;
        }

        /// <summary>
        /// 월드 공간의 두 점을 받아 UI Canvas 좌표로 변환해 LineRenderer에 그립니다.
        /// </summary>
        public void Setup(Vector3 worldA, Vector3 worldB)
        {
            Camera worldCamera = Camera.main;
            
            // 1) 월드 → 스크린
            Vector3 screenA = worldCamera.WorldToScreenPoint(worldA);
            Vector3 screenB = worldCamera.WorldToScreenPoint(worldB);

            // 2) 스크린 → Canvas 로컬
            RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
            Vector2 localA, localB;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenA, _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldCamera, out localA);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenB, _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldCamera, out localB);

            // 3) LineRenderer 세팅 (UI 좌표계가 아니라, WorldSpace Canvas라면 이 부분도 달라집니다)
            _line.positionCount = 2;
            _line.useWorldSpace = false;          // Canvas 로컬 공간에 그릴 때
            _line.SetPosition(0, localA);
            _line.SetPosition(1, localB);
        }
    }
}