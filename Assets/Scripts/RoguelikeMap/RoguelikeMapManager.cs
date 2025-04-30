using UnityEngine;

namespace RoguelikeMap
{
    public class RoguelikeMapManager : MonoBehaviour
    {
        private MapGrid _grid;
        private RoguelikeMapGenerator _mapGenerator;

        public void Setup(int row, int col)
        {
            _mapGenerator = new RoguelikeMapGenerator();
            _grid = new MapGrid(_mapGenerator.GenerateEmptyMapTemplate(row, col));
        }
    }
}