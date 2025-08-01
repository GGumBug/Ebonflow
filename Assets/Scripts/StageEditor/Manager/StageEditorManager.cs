using UnityEngine;
using System.Collections.Generic;

namespace StageEditor
{
    public class StageEditorManager
    {
        private List<StageSaveUnit> _stageSaveUnits;

        public StageEditorManager()
        {
            _stageSaveUnits = new();
        }

        public void AddStageSaveUnitToList(StageSaveUnit stageSaveUnit)
        {
            _stageSaveUnits.Add(stageSaveUnit);
        }

        public void RemoveStageSaveUnitToList(StageSaveUnit stageSaveUnit)
        {
            if (_stageSaveUnits.Contains(stageSaveUnit))
            {
                _stageSaveUnits.Remove(stageSaveUnit);
            }
        }
    }
}