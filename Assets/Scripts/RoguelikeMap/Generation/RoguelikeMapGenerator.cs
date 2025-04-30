using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeMap
{
    public class RoguelikeMapGenerator
    {
        public List<List<MapNode>> GenerateEmptyMapTemplate(int rowCount, int colCount)
        {
            var template = new List<List<MapNode>>(rowCount);
            for (int r = 0; r < rowCount; r++)
            {
                var row = new List<MapNode>(colCount);
                for (int c = 0; c < colCount; c++)
                    row.Add(new MapNode(r, c, RoomType.None));
                template.Add(row);
            }
            return template;
        }
    }
}