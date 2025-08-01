using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        private Func<Vector2Int, bool> _requestIsOutOfBounds;
        private Action<int, int, Vector2Int> _requestStageSaveUnitSpawn;

        private void Awake()
        {
            _btnCreate.onClick.AddListener(OnCreateButtonClicked);

            _inputID.SetTextWithoutNotify("0");
            _inputStarLevel.SetTextWithoutNotify("1");
        }

        public void Setup(Func<Vector2Int, bool> requestIsOutOfBounds, Action<int, int, Vector2Int> requestStageSaveUnitSpawn)
        {
            _requestIsOutOfBounds = requestIsOutOfBounds;
            _requestStageSaveUnitSpawn = requestStageSaveUnitSpawn;
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

            _requestStageSaveUnitSpawn.Invoke(id, starLevel, spawnPos);
        }
    }
}