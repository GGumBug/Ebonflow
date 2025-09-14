using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIStartMenu : UIBase
{
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonReset;

    public void Setup(UnityAction StartGame, UnityAction ResetData)
    {
        buttonStart.onClick.AddListener(StartGame);
        buttonReset.onClick.AddListener(ResetData);
    }
}
