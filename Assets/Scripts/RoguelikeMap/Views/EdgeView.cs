using UnityEngine;
using Radishmouse;

namespace RoguelikeMap.UI
{
    [RequireComponent(typeof(UILineRenderer))]
    public class EdgeView : MonoBehaviour
    {
        [SerializeField] private UILineRenderer _line;
        
        private Canvas _canvas; // Screen Space - Overlay 또는 Camera

        void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        /// <summary>
        /// 월드 공간의 두 점을 받아 UI Canvas 좌표로 변환해 LineRenderer에 그립니다.
        /// </summary>
        public void Setup(Vector3 worldA, Vector3 worldB)
        {
            _line.points = new Vector2[]{worldA, worldB};
            _line.SetAllDirty();
        }
    }
}