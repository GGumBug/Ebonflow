using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace RoguelikeMap.UI
{
    public class NodeView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _locationIcon;
        [SerializeField] private TextMeshProUGUI _textLabel;

        private static readonly Dictionary<LocationType, Color> _typeColors = new()
        {
            { LocationType.Monster, Color.red },
            { LocationType.Elite,   Color.magenta },
            { LocationType.Camp,    Color.green },
        };

        public void Setup(Vector2 position, LocationType type)
        {
            float cellW = _rectTransform.sizeDelta.x;
            
            // 화면 X = (열 인덱스 – 가운데열) × 셀 너비
            float x = position.x * cellW;

            // Y는 기존처럼
            float y = -position.y * _rectTransform.sizeDelta.y;

            _rectTransform.anchoredPosition = new Vector2(x, y);

            // 3) 텍스트(첫 글자) 세팅
            _textLabel.text = type.ToString().Substring(0, 1);

            // 4) 아이콘 색상 세팅
            if (_typeColors.TryGetValue(type, out var c))
                _locationIcon.color = c;
            else
                _locationIcon.color = Color.white;
        }
    }
}