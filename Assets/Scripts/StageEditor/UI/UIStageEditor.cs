using System;
using UnityEngine;

namespace StageEditor.UI
{
    public class UIStageEditor : MonoBehaviour
    {
        [SerializeField] private StageSpawnUnitPanel _stageSaveUnitPanel;

        public void Setup(Func<Vector2Int, bool> requestIsOutOfBounds, Action<int, int, Vector2Int> requestStageSaveUnitSpawn)
        {
            _stageSaveUnitPanel.Setup(requestIsOutOfBounds, requestStageSaveUnitSpawn);
        }
    }
}

