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

        public bool IsActive { get; set; } = false;

        public MapNode(int row, int col, LocationType type)
        {
            position = new Vector2(col, row);
            this.type = type;
            Edges = new();
        }
    }

}