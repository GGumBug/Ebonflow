using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Data;

public class DebugPanelSpawnUnit : MonoBehaviour
{
    [Header("Spawn 유닛 ID")]
    [Tooltip("스폰 할 유닛 ID를 입력하세요.")]
    [SerializeField] private TMP_InputField _inputID;

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
    }

    private void OnDestroy()
    {
        _btnCreate.onClick.RemoveListener(OnCreateButtonClicked);
    }

    private void OnCreateButtonClicked()
    {
        if (!int.TryParse(_inputX.text, out int id) || 
            !int.TryParse(_inputX.text, out int x) ||
            !int.TryParse(_inputY.text, out int y))
        {
            Debug.LogWarning("AutoBattleDebugUI: ID, X, Y 입력이 올바른 정수가 아닙니다.");
            return;
        }

        int unitID = id;
        TeamType team = (TeamType)_teamDropdown.value;
        Vector2Int spawnPos = new Vector2Int(x, y);

        CreateDebugUnit(unitID, team, spawnPos);
    }

    private void CreateDebugUnit(int unitID , TeamType team, Vector2Int pos)
    {
        // 예: AutoBattleManager 쪽에서 Addressable 로드가 이미 끝났다고 가정
        AutoBattleManager mgr = AutoBattleManager.Instance;

        Unit newUnit = team == TeamType.Ally
            ? mgr.SpawnAlly( /*unitID*/ unitID, /*star*/ 1, pos)
            : mgr.SpawnEnemy(/*unitID*/ unitID, /*star*/ 1, pos);

        Debug.Log($"DebugUI: {team} 유닛 생성 완료 @ {pos}");
    }
}
