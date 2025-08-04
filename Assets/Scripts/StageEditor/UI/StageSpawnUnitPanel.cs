using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StageEditor.UI
{
    public class StageSpawnUnitPanel : MonoBehaviour
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

        private const int MIN_NOT_PLACEABLE_ROW_INDEX = 2;
        private const int MIN_PLACEABLE_ROW_INDEX = MIN_NOT_PLACEABLE_ROW_INDEX + 1;

        private Func<Vector2Int, bool> _requestIsOutOfBounds;
        private Action<int, int, Vector2Int> _requestStageSaveUnitSpawn;

        private void Awake()
        {
            _btnCreate.onClick.AddListener(OnCreateButtonClicked);

            _inputID.SetTextWithoutNotify("0");
            _inputStarLevel.SetTextWithoutNotify("1");
            _inputY.onEndEdit.AddListener(ValidateYInput);
        }

        public void Setup(Func<Vector2Int, bool> requestIsOutOfBounds, Action<int, int, Vector2Int> requestStageSaveUnitSpawn)
        {
            _requestIsOutOfBounds = requestIsOutOfBounds;
            _requestStageSaveUnitSpawn = requestStageSaveUnitSpawn;
        }

        private void ValidateYInput(string rawValue)
        {
            // 1) 숫자 파싱 시도
            if (int.TryParse(rawValue, out int y))
            {
                // 2) 최소 배치 가능 행보다 아래라면 보정
                if (y <= MIN_NOT_PLACEABLE_ROW_INDEX)
                {
                    y = MIN_PLACEABLE_ROW_INDEX;
                    _inputY.text = y.ToString();
                }
            }
            else
            {
                // 3) 숫자 아닌 값이 들어왔을 때 기본값 설정
                _inputY.text = MIN_PLACEABLE_ROW_INDEX.ToString();
            }
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