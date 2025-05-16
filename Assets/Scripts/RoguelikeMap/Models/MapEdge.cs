using System;

namespace RoguelikeMap
{
    public class MapEdge
    {
        private bool _isActive = false;
        private bool _hasPassed = false;

        /// <summary>출발 노드</summary>
        public MapNode From;

        /// <summary>도착 노드</summary>
        public MapNode To;

        /// <summary>세대 정보</summary>
        public int Generation;

        public Action<MapEdge> OnChangeLineState;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;

                _isActive = value;
                OnChangeLineState?.Invoke(this);
            }
        }

        public bool HasPassed
        {
            get => _hasPassed;
            set
            {
                if (_hasPassed == value) return;
                _hasPassed = value;
                OnChangeLineState?.Invoke(this);
            }
        }
    }
}