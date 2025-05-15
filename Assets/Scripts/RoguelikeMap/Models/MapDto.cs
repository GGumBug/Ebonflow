using System;
using System.Collections.Generic;

namespace RoguelikeMap
{
    [Serializable]
    public class MapData
    {
        // JsonUtility 다차원 배열 지원 x 랩핑 방법 채택
        public int selectedRow = -1; // -1 이면 “아직 선택 없음” 상태
        public int selectedIndex = -1; // ListCount 기준
        public int maxRow;
        public int maxCol;
        public NodeDataRow[] nodes;
        public EdgeDataRow[] edges;
    }

    [Serializable]
    public class NodeDataRow
    {
        // 한 행(row)에 속한 노드들
        public List<NodeData> row = new();
    }

    [Serializable]
    public class EdgeDataRow
    {
        // 한 세대(generation)에 속한 엣지들
        public List<EdgeData> path = new();
    }

    [Serializable]
    public struct NodeData
    {
        public int row;
        public int col;
        public LocationType type;
        public bool isActive;
    }

    [Serializable]
    public struct EdgeData
    {
        public int fromIndex;
        public int toIndex;
        public int generation;
    }
}

