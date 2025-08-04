using System;
using UnityEngine;

namespace StageEditor.UI
{
    public class UIStageEditor : MonoBehaviour
    {
        [SerializeField] private StageSpawnUnitPanel stageSpawnUnitPanel;
        [SerializeField] private StageSavePanel stageSavePanel;

        public void Setup(
            Func<Vector2Int, bool> requestIsOutOfBounds, 
            Action<int, int, Vector2Int> requestStageSaveUnitSpawn, 
            Action<int, int, int, int> onSaveStageData,
            Action<int, int, int, int> onLoadStageData)
        {
            stageSpawnUnitPanel.Setup(requestIsOutOfBounds, requestStageSaveUnitSpawn);

            stageSavePanel.OnLoadStageDataAction += onLoadStageData;
            stageSavePanel.OnSaveStageDataAction += onSaveStageData;
        }
    }
}

