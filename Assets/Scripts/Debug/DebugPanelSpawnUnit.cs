using UnityEngine;
using TMPro;
using UnityEngine.UI;
using AutoBattle;

public class DebugPanelSpawnUnit : MonoBehaviour
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

    [Header("팀 선택")]
    [Tooltip("스폰할 유닛의 TeamType을 선택하세요.")]
    [SerializeField] private TMP_Dropdown _teamDropdown;

    [Header("실행 버튼")]
    [Tooltip("클릭 시 입력한 위치·팀으로 유닛을 생성합니다.")]
    [SerializeField] private Button _btnCreate;

    private void Awake()
    {
        // Dropdown 옵션 초기화 (enum값을 문자열로)
        _teamDropdown.options.Clear();
        foreach (var name in System.Enum.GetNames(typeof(TeamType)))
            _teamDropdown.options.Add(new TMP_Dropdown.OptionData(name));
        _teamDropdown.RefreshShownValue();

        // 버튼 콜백 연결
        _btnCreate.onClick.AddListener(OnCreateButtonClicked);

        _inputID.SetTextWithoutNotify("0");
        _inputStarLevel.SetTextWithoutNotify("1");
    }

    private void OnDestroy()
    {
        _btnCreate.onClick.RemoveListener(OnCreateButtonClicked);
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

        if (AStarAlgorithmManager.Instance.Grid.IsOutOfBounds(spawnPos))
        {
            Debug.LogWarning("BattleGrid 범위를 벗어났습니다.");
            return;
        }

        TeamType team = (TeamType)_teamDropdown.value;

        CreateDebugUnit(id, starLevel, team, spawnPos);
    }

    private void CreateDebugUnit(int unitID, int starLevel, TeamType team, Vector2Int pos)
    {
        AutoBattleUnitManager mgr = AutoBattleUnitManager.Instance;

        Unit newUnit = team == TeamType.Ally
            ? mgr.SpawnAlly( /*unitID*/ unitID, /*star*/ starLevel, pos, AutoBattleUnitManager.Instance.UnitBench)
            : mgr.SpawnEnemy(/*unitID*/ unitID, /*star*/ starLevel, pos, AStarAlgorithmManager.Instance.Grid);

        AStarAlgorithmManager.Instance.Grid.SetNodeBlock(pos, true, newUnit.Agent);
        Debug.Log($"DebugUI: {team} 유닛 생성 완료 @ {pos}");
    }
}
