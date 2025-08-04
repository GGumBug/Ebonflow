using StageEditor.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageEditor
{
    public class StageEditorManager
    {
        private const string STAGE_FILE_NAME = "StageData";
        private const int UNIT_MASK = 1 << 7;

        private Action<int, int, Vector2Int> requestSpawnStageSaveUnit;
        private StageEditorInputReader _stageEditorInputReader;
        private List<StageSaveUnit> _stageSaveUnits;
        private StageSaveLoad _stageSaveLoad;

        public StageEditorManager(StageEditorInputReader stageEditorInputReader, StageSaveUnitSpawner stageSaveUnitSpawner)
        {
            _stageEditorInputReader = stageEditorInputReader;
            _stageSaveUnits = new();
            _stageSaveLoad = new();

            requestSpawnStageSaveUnit += stageSaveUnitSpawner.SpawnStageSaveUnit;
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

        public void SaveStageData(int id, int act, int min, int max)
        {
            if (_stageSaveUnits.Count <= 0)
            {
                Debug.LogWarning("저장 할 스테이지 데이터가 없습니다.");
                return;
            }

            StageData stageData = new StageData();

            if (id == -1)
            {
                id  = _stageSaveLoad.GetFileCount();
                stageData.stageID = id;
            }    
            else
                stageData.stageID = id;
            
            stageData.stageAct = act;
            stageData.min = min;
            stageData.max = max;

            List<StageEditorUnitInfo> newUnitInfoList = new();

            foreach (var unit in _stageSaveUnits)
                newUnitInfoList.Add(unit.GetStageEditorUnitInfo());

            stageData.unitList = newUnitInfoList;

            _stageSaveLoad.Save(stageData, $"{STAGE_FILE_NAME}_{id}_{act}_{min}_{max}");
        }

        public void LoadStageData(int id, int act, int min, int max)
        {
            StageData loadData = _stageSaveLoad.Load($"{STAGE_FILE_NAME}_{id}_{act}_{min}_{max}");
            if (loadData == null)
            {
                Debug.LogError("데이터 불러오기 실패.");
                return;
            }

            int count = _stageSaveUnits.Count;
            for (int i = count - 1; i > -1; --i)
            {
                _stageSaveUnits[i].Destroyed();
            }

            _stageSaveUnits.Clear();

            foreach (var data in loadData.unitList)
                requestSpawnStageSaveUnit.Invoke(data.unitID, data.starLevel, new Vector2Int(data.gridX, data.gridY));
        }
    }
}