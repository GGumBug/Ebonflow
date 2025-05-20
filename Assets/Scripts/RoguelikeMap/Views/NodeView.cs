using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoguelikeMap.UI
{
    public class NodeView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _locationIcon;
        [SerializeField] private TextMeshProUGUI _textLabel;
        [SerializeField] private Button _btnLocation;

        private Vector2Int _cellPosition;
        private LocationType _locationType = LocationType.None;

        public event Action<Vector2Int> SelectNodeRequested;
        public event Action<int, int, int> NodeClickAction;

        private const int LabelCharCount = 1;

        private static readonly Dictionary<LocationType, Color> TypeColors = new()
        {
            { LocationType.Monster, Color.red },
            { LocationType.Elite,   Color.magenta },
            { LocationType.Camp,    Color.green },
        };

        private void Awake()
        {
            Debug.Assert(_rectTransform != null, "RectTransform is not assigned.");
            Debug.Assert(_locationIcon != null, "LocationIcon is not assigned.");
            Debug.Assert(_textLabel != null, "TextLabel is not assigned.");

            _btnLocation.onClick.AddListener(OnClick);
        }

        public void Setup(MapNode mapNode, int xIndex, Vector2 position, Action<Vector2Int> selectNodeRequested, Action<int, int, int> nodeClickAction)
        {
            CacheNodeData(mapNode, xIndex);
            UpdatePosition(position);
            UpdateLabel();
            UpdateIconColor();

            SelectNodeRequested += selectNodeRequested;
            NodeClickAction += nodeClickAction;
        }

        private void CacheNodeData(MapNode mapNode, int xIndex)
        {
            _cellPosition = new Vector2Int(xIndex, (int)mapNode.position.y);
            _locationType = mapNode.type;
            SetActiveState(mapNode.IsActive);
            
            mapNode.OnActiveStateChanged += SetActiveState;
        }

        private void UpdatePosition(Vector2 position)
        {
            float cellWidth = _rectTransform.sizeDelta.x;
            float posX = position.x * cellWidth;
            float posY = -position.y * _rectTransform.sizeDelta.y;
            _rectTransform.anchoredPosition = new Vector2(posX, posY);
        }

        private void UpdateLabel()
        {
            _textLabel.text = _locationType.ToString().Substring(0, LabelCharCount);
        }

        private void UpdateIconColor()
        {
            if (TypeColors.TryGetValue(_locationType, out var color))
                _locationIcon.color = color;
            else
                _locationIcon.color = Color.white;
        }

        public void SetActiveState(bool isActive)
        {
            _btnLocation.interactable = isActive;
        }

        // 버튼 또는 터치 이벤트에 연결
        public void OnClick()
        {
            SelectNodeRequested?.Invoke(_cellPosition);
            NodeClickAction?.Invoke(_cellPosition.x, _cellPosition.y, (int)_locationType);
        }
    }
}
