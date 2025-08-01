using StageEditor;
using UnityEngine;
using StageEditor.Input;

namespace StageEditor
{
    public class StageSaveUnitSpawner
    {
        private GameObject _stageSaveUnitPrefab;
        private StageEditorManager _stageEditorManager;
        private StageEditorInputReader _stageEditorInputReader;

        public StageSaveUnitSpawner(GameObject stageSaveUnitPrefab, StageEditorManager stageEditorManager, StageEditorInputReader stageEditorInputReader)
        {
            _stageSaveUnitPrefab = stageSaveUnitPrefab;
            _stageEditorManager = stageEditorManager;
            _stageEditorInputReader = stageEditorInputReader;
        }

        public void SpawnStageSaveUnit(int unitID, int starLevel, Vector2Int pos)
        {
            GameObject newGo = Object.Instantiate(_stageSaveUnitPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
            StageSaveUnit newUnit = newGo.GetComponent<StageSaveUnit>();
            newUnit.Setup(unitID, starLevel, _stageEditorManager.RemoveStageSaveUnitToList, () => _stageEditorInputReader.MousePosition);
            _stageEditorManager.AddStageSaveUnitToList(newUnit);
        }
    }
}

