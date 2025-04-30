using System.Collections.Generic;

namespace RoguelikeMap
{
    public class MapGrid
    {
        public List<List<MapNode>> _gridTemplate;

        public MapGrid(List<List<MapNode>> gridTemplate)
        {
            _gridTemplate = gridTemplate;
        }
    }
}