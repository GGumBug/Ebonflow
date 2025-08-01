using AutoBattle.Input;
using StageEditor.Input;
using System.Collections.Generic;
using UnityEngine;

namespace StageEditor
{
    public class StageEditorManager
    {
        private const int UNIT_MASK = 1 << 7;
        private StageEditorInputReader _stageEditorInputReader;
        private List<StageSaveUnit> _stageSaveUnits;

        public StageEditorManager(StageEditorInputReader stageEditorInputReader)
        {
            _stageEditorInputReader = stageEditorInputReader;
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

        public void CurrentMousePositionUnitDestroy()
        {
            Vector3 screenPos = _stageEditorInputReader.MousePosition;
            screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPos);

            Collider2D col = Physics2D.OverlapPoint((Vector2)worldPoint, UNIT_MASK);
            if (col == null)
                return;

            var draggable = col.GetComponent<StageSaveUnit>();
            if (draggable == null)
                return;

            if (draggable is StageSaveUnit unit)
            {
                draggable.Destroyed();
            }
        }
    }
}