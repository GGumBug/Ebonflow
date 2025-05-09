using System.Collections.Generic;

namespace RoguelikeMap
{
    public class MapLayout
    {
        public int MaxRow { get; }
        public int MaxColumn { get; }
        public List<List<MapNode>> Grid    { get; }
        public List<List<MapEdge>> Paths   { get; }

        public MapLayout(
            int maxRow,
            int maxColumn,
            List<List<MapNode>> grid,
            List<List<MapEdge>> paths)
        {
            MaxRow = maxRow;
            MaxColumn = maxColumn;
            Grid  = grid;
            Paths = paths;
        }
    }
}