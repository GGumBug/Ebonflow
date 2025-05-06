using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeMap
{
    public enum RoomType
    {
        None,
        Start,
        Combat,
        Boss
    }

    public class MapNode
    {
        public Vector2 position;
        public RoomType type;
        public List<MapEdge> Edges;

        public MapNode(int row, int col, RoomType type)
        {
            position = new Vector2(col, row);
            this.type = type;
            Edges = new();
        }
    }

}