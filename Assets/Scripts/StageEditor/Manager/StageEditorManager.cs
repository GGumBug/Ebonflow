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
        private StageSaveLoad _stageSaveLoad;

        public StageEditorManager(StageEditorInputReader stageEditorInputReader, StageSaveUnitSpawner stageSaveUnitSpawner)
        {
            _stageEditorInputReader = stageEditorInputReader;
            _stageSaveUnits = new();
            _stageSaveLoad = new();

            _stageSaveLoad.RequestSpawnStageSaveUnit += stageSaveUnitSpawner.SpawnStageSaveUnit;
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

            var saveUnit = col.GetComponent<StageSaveUnit>();
            if (saveUnit == null)
                return;

            if (saveUnit is StageSaveUnit unit)
            {
                saveUnit.Destroyed();
            }
        }

        public void SaveStageData(int id, int act, int min, int max) => _stageSaveLoad.SaveStageData(_stageSaveUnits, id, act, min, max);
        public void LoadStageData(int id, int act, int min, int max) => _stageSaveLoad.LoadStageData(_stageSaveUnits, id, act, min, max);
    }
}