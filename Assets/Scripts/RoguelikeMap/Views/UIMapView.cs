using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeMap.UI
{
    public class UIMapView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScrollRect    _mapScrollRect;
        [SerializeField] private RectTransform _mapContentRect;
        [SerializeField] private GameObject    _nodeViewPrefab;
        [SerializeField] private GameObject    _edgeViewPrefab;

        [Header("Layout Settings")]
        [Tooltip("X 좌표 변환 시 곱해질 스케일")]
        [SerializeField] private float _xScale = 193f;
        [Tooltip("X 좌표 변환 시 더해질 오프셋")]
        [SerializeField] private float _xOffset = 400f;
        [Tooltip("Y 좌표 변환 시 곱해질 스케일")]
        [SerializeField] private float _yScale = 300f;
        [Tooltip("Y 좌표 변환 시 더해질 오프셋")]
        [SerializeField] private float _yOffset = 300f;


        [Header("Padding")]
        private float _paddingHorizontal = 3f;
        private float _paddingVertical   = 3f;
        private float _cellSpacingHorizontal = 2f;
        private float _cellSpacingVertical = 3f;

        public void RenderMap(MapLayout layout)
        {
            foreach (var path in layout.Paths)
            {
                foreach (var edge in path)
                {
                    var go = Instantiate(_edgeViewPrefab, _mapContentRect);
                    var view = go.GetComponent<EdgeView>();
                    view.Setup(Convert(edge.From.position), Convert(edge.To.position));
                }
            }

            int lastRowIndex = layout.MaxRow - 2;

            float totalRowSpacing = layout.Grid[lastRowIndex][0].position.y * _cellSpacingVertical;

            float totalVerticalPadding = _paddingVertical * 2f;

            float contentHeightSize = (totalRowSpacing + totalVerticalPadding) * 100;

            _mapContentRect.sizeDelta = new Vector2(0 , contentHeightSize);

            float screenWidth = GetComponent<RectTransform>().rect.width;

            _cellSpacingHorizontal = screenWidth * 0.01f / (layout.MaxColumn - 2) * 0.5f;

            foreach (var row in layout.Grid)
            {
                foreach (var node in row)
                {
                    float xPos = node.position.x * _cellSpacingHorizontal + _paddingHorizontal;
                    float yPos = - (node.position.y * _cellSpacingVertical) - _paddingVertical;

                    var go = Instantiate(_nodeViewPrefab, _mapContentRect);
                    var view = go.GetComponent<NodeView>();
                    view.Setup(new Vector2(xPos, yPos), node.type);
                }
            }

            // 강제 레이아웃 리빌드
            LayoutRebuilder.ForceRebuildLayoutImmediate(_mapContentRect);
        }

        Vector2 Convert(Vector2 oldPos)
        {
            float newX = oldPos.x * _xScale + _xOffset;
            float newY = oldPos.y * _yScale + _yOffset;
            return new Vector2(newX, newY);
        }
    }
}