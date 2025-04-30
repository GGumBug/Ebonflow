using System.Collections.Generic;

namespace RoguelikeMap
{
    public class MapGrid
    {
        private List<List<MapNode>> _map;

        public List<List<MapNode>> GetMap => _map;

        public MapGrid(List<List<MapNode>> map)
        {
            _map = map;
        }
    }
}