using System;

namespace StageEditor
{
    [Serializable]
    public class StageEditorUnitInfo
    {
        public int unitID;
        public int starLevel;
        public int gridX;
        public int gridY;

        public StageEditorUnitInfo(
            int unitID,
            int starLevel,
            int gridX,
            int gridY
            )
        {
            this.unitID = unitID;
            this.starLevel = starLevel;
            this.gridX = gridX;
            this.gridY = gridY;
        }
    }
}