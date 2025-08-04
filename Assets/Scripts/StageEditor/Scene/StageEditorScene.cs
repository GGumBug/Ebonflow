using StageEditor.UI;
using UnityEngine;
using StageEditor.Input;

namespace StageEditor
{
    public class StageEditorScene : MonoBehaviour
    {
        [Header("Grid Bounds")]
        [SerializeField] private Vector2Int _gridBottomLeft;
        [SerializeField] private Vector2Int _gridTopRight;

        [Header("Prefabs")]
        [SerializeField] private GameObject _stageSaveUnitPrefab;

        [Header("UI")]
        [SerializeField] private UIStageEditor _uiStageEditor;

        private StageEditorManager _stageEditorManager;
        private StageEditorInputReader _stageEditorInputReader;
        private StageSaveUnitSpawner _stageSaveUnitSpawner;

        public bool IsOutOfBounds(Vector2Int toGridIndex) =>
        toGridIndex.x < 0 || toGridIndex.x > _gridTopRight.x ||
        toGridIndex.y < 0 || toGridIndex.y > _gridTopRight.y;

        private void Awake()
        {
            _stageEditorInputReader = gameObject.AddComponent<StageEditorInputReader>();
            _stageSaveUnitSpawner = new StageSaveUnitSpawner();
            _stageEditorManager = new StageEditorManager(_stageEditorInputReader, _stageSaveUnitSpawner);

            _stageSaveUnitSpawner.Setup(_stageSaveUnitPrefab, _stageEditorManager, _stageEditorInputReader);
            _uiStageEditor.Setup(
                (toGridIndex) => IsOutOfBounds(toGridIndex), 
                _stageSaveUnitSpawner.SpawnStageSaveUnit, 
                _stageEditorManager.SaveStageData,
                _stageEditorManager.LoadStageData
                );

            _stageEditorInputReader.OnRightMouseStarted += _stageEditorManager.CurrentMousePositionUnitDestroy;
        }
    }
}