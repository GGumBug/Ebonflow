using System;
using UnityEngine;

namespace StageEditor.UI
{
    public class UIStageEditor : MonoBehaviour
    {
        [SerializeField] private StageSaveUnitPanel _stageSaveUnitPanel;

        public void Setup(GameObject stageSaveUnitPrefab, StageEditorManager stageEditorManager, Func<Vector2Int, bool> requestIsOutOfBounds)
        {
            _stageSaveUnitPanel.Setup(stageSaveUnitPrefab, stageEditorManager, requestIsOutOfBounds);
        }
    }
}

