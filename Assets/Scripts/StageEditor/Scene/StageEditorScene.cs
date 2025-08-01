using StageEditor.UI;
using UnityEngine;

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
        public bool IsOutOfBounds(Vector2Int toGridIndex) =>
        toGridIndex.x < 0 || toGridIndex.x > _gridTopRight.x ||
        toGridIndex.y < 0 || toGridIndex.y > _gridTopRight.y;

        private void Awake()
        {
            _stageEditorManager = new StageEditorManager(
                _stageSaveUnitPrefab
                );

            _uiStageEditor.Setup(_stageSaveUnitPrefab, _stageEditorManager, (toGridIndex) => IsOutOfBounds(toGridIndex));
        }
    }
}