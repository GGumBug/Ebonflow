using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace StageEditor.UI
{
    public class StageSaveUnitPanel : MonoBehaviour
    {
        [Header("Spawn 유닛 ID")]
        [Tooltip("스폰 할 유닛 ID를 입력하세요.")]
        [SerializeField] private TMP_InputField _inputID;

        [Header("Spawn 유닛 StarLevel")]
        [Tooltip("스폰 할 유닛 ID를 입력하세요.")]
        [SerializeField] private TMP_InputField _inputStarLevel;

        [Header("Spawn 위치 설정")]
        [Tooltip("스폰할 X 좌표를 입력하세요.")]
        [SerializeField] private TMP_InputField _inputX;
        [Tooltip("스폰할 Y 좌표를 입력하세요.")]
        [SerializeField] private TMP_InputField _inputY;

        [Header("실행 버튼")]
        [Tooltip("클릭 시 입력한 위치·팀으로 유닛을 생성합니다.")]
        [SerializeField] private Button _btnCreate;

        private GameObject _stageSaveUnitPrefab;
        private Func<Vector2Int, bool> _requestIsOutOfBounds;
        private Action<StageSaveUnit> _requestAddStageSaveUnitToList;

        private void Awake()
        {
            _btnCreate.onClick.AddListener(OnCreateButtonClicked);

            _inputID.SetTextWithoutNotify("0");
            _inputStarLevel.SetTextWithoutNotify("1");
        }

        public void Setup(GameObject stageSaveUnitPrefab, StageEditorManager stageEditorManager, Func<Vector2Int, bool> requestIsOutOfBounds)
        {
            _stageSaveUnitPrefab = stageSaveUnitPrefab;
            _requestIsOutOfBounds = requestIsOutOfBounds;
            _requestAddStageSaveUnitToList = stageEditorManager.AddStageSaveUnitToList;
        }

        private void OnCreateButtonClicked()
        {
            if (!int.TryParse(_inputID.text, out int id) ||
                !int.TryParse(_inputStarLevel.text, out int starLevel) ||
                !int.TryParse(_inputX.text, out int x) ||
                !int.TryParse(_inputY.text, out int y))
            {
                Debug.LogWarning("AutoBattleDebugUI: ID, X, Y 입력이 올바른 정수가 아닙니다.");
                return;
            }

            Vector2Int spawnPos = new Vector2Int(x, y);

            if (_requestIsOutOfBounds.Invoke(spawnPos))
            {
                Debug.LogWarning("Grid 범위를 벗어났습니다.");
                return;
            }

            CreateSaveStageUnit(id, starLevel, spawnPos);
        }

        private void CreateSaveStageUnit(int unitID, int starLevel, Vector2Int pos)
        {
            GameObject newGo = Instantiate(_stageSaveUnitPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
            StageSaveUnit newUnit = newGo.GetComponent<StageSaveUnit>();
            _requestAddStageSaveUnitToList.Invoke(newUnit);
        }
    }
}