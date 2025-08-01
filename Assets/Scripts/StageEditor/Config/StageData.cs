using System;
using System.Collections.Generic;

namespace StageEditor
{
    [Serializable]
    public class StageData
    {
        public int stageAct;
        public int min;
        public int max;
        public List<StageEditorUnitInfo> unitList = new List<StageEditorUnitInfo>();
    }
}