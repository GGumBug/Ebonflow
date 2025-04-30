using UnityEngine;
namespace RoguelikeMap
{
    public struct MapEdge
    {
        public MapNode From, To;
        public int Generation; // 0~5 까지, 색깔 표현용
    }
}