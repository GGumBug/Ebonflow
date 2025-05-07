using System;
using System.Collections.Generic;

namespace RoguelikeMap
{
    [Serializable]
    public class MapData
    {
        public List<NodeData> nodes;
        public List<EdgeData> edges;
    }

    [Serializable]
    public struct NodeData
    {
        public int row, col;
        public LocationType type;
    }

    [Serializable]
    public struct EdgeData
    {
        public int fromIndex;
        public int toIndex;
        public int generation;
    }
}

