using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StageEditor.UI
{
    public class StageSavePanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputID;
        [SerializeField] private TMP_InputField inputAct;
        [SerializeField] private TMP_InputField inputMinFloor;
        [SerializeField] private TMP_InputField inputMaxFloor;
        [SerializeField] private Button btnLoad;
        [SerializeField] private Button btnSave;

        public event Action<int, int, int, int> OnLoadStageDataAction;
        public event Action<int, int, int, int> OnSaveStageDataAction;

        private void Awake()
        {
            btnLoad.onClick.AddListener(OnLoadStageData);
            btnSave.onClick.AddListener(OnSaveStageData);
        }

        private void OnLoadStageData()
        {
            if (!int.TryParse(inputID.text, out int id) ||
                !int.TryParse(inputAct.text, out int act) ||
                !int.TryParse(inputMinFloor.text, out int min) ||
                !int.TryParse(inputMaxFloor.text, out int max)
                )
            {
                Debug.LogWarning("AutoBattleDebugUI: act, min, max 입력이 올바른 정수가 아닙니다.");
                return;
            }

            OnLoadStageDataAction.Invoke(id, act, min, max);
        }

        private void OnSaveStageData()
        {
            if (!int.TryParse(inputID.text, out int id) ||
                !int.TryParse(inputAct.text, out int act) ||
                !int.TryParse(inputMinFloor.text, out int min) ||
                !int.TryParse(inputMaxFloor.text, out int max)
                )
            {
                Debug.LogWarning("AutoBattleDebugUI: act, min, max 입력이 올바른 정수가 아닙니다.");
                return;
            }

            OnSaveStageDataAction.Invoke(id, act, min, max);
        }
    }
}