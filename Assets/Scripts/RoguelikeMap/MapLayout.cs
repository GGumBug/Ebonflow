using System.Collections.Generic;

namespace RoguelikeMap
{
    public class MapLayout
    {
        public List<List<MapNode>> Grid    { get; }
        public List<List<MapEdge>> Paths   { get; }

        public MapLayout(
            List<List<MapNode>> grid,
            List<List<MapEdge>> paths)
        {
            Grid  = grid;
            Paths = paths;
        }
    }
}