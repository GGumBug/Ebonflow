using AutoBattle;
using TMPro;
using UnityEngine;

public class SoulCoinPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txtSoulCoin;

    public void Setup()
    {
        var playerDataContext = AutoBattleDataManager.Instance.AutoBattlePlayerDataContext;
        SetSoulCoin(playerDataContext.GetSoulCoin());

        playerDataContext.OnAddSoulCoin += () => SetSoulCoin(playerDataContext.GetSoulCoin());
        playerDataContext.OnSpendSoulCoin += () => SetSoulCoin(playerDataContext.GetSoulCoin());
    }

    public void SetSoulCoin(int currentSoulCoin)
    {
        txtSoulCoin.text = currentSoulCoin.ToString();
    }
}
