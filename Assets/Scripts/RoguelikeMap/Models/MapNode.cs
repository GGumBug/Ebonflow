using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeMap
{
    public enum LocationType
    {
        None,
        Camp,
        Monster,
        Elite,
        Boss
    }

    public class MapNode
    {
        public Vector2 position;
        public LocationType type;
        public List<MapEdge> Edges;

        public Action<bool> OnActiveStateChanged;

        private bool _isActive = false;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;

                _isActive = value;
                OnActiveStateChanged?.Invoke(_isActive);
            }
        }

        public MapNode(int row, int col, LocationType type, bool isActive = false)
        {
            position = new Vector2(col, row);
            this.type = type;
            _isActive = isActive;
            Edges = new();
        }
    }

}