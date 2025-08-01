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

        [Header("팀 선택")]
        [Tooltip("스폰할 유닛의 TeamType을 선택하세요.")]
        [SerializeField] private TMP_Dropdown _teamDropdown;

        [Header("실행 버튼")]
        [Tooltip("클릭 시 입력한 위치·팀으로 유닛을 생성합니다.")]
        [SerializeField] private Button _btnCreate;
    }
}