using UnityEngine;

public class StageEditorScene : MonoBehaviour
{
    [Header("Grid Bounds")]
    [SerializeField] private Vector2Int _gridBottomLeft;
    [SerializeField] private Vector2Int _gridTopRight;

    [Header("Prefabs")]
    [SerializeField] private GameObject _stageSaveUnitPrefab;

    private StageEditorManager _stageEditorManager;

    private void Awake()
    {
        _stageEditorManager = new StageEditorManager(
            _stageSaveUnitPrefab
            );
    }
}
