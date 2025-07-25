using AutoBattle;
using DeckSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private Button btnBuy;
    [SerializeField] private TextMeshProUGUI txtUnitTier;
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private TextMeshProUGUI txtUnitID;

    public CardData Data { get; private set; }

    public event Func<int, bool> RequestSpendSoulCoin;

    private void Awake()
    {
        btnBuy.onClick.AddListener(BuyCardUnit);
    }

    public void SetData(CardData cardData, bool canBuy)
    {
        btnBuy.interactable = canBuy;

        Data = cardData;
        txtUnitTier.text = Data.tier.ToString();
        txtPrice.text = "Price" + Data.price.ToString();
        txtUnitID.text = "UnitID" + Data.unitID.ToString();
    }

    public void SetEvents(AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        RequestSpendSoulCoin += autoBattlePlayerDataContext.SpendSoulCoin;
    }

    public void CheckCanBuy(int soulCoin)
    {
        bool canBuy = Data.price <= soulCoin;
        btnBuy.interactable = canBuy;
    }

    private void BuyCardUnit()
    {
        if (RequestSpendSoulCoin.Invoke(Data.price))
        {
            AutoBattleUnitManager.Instance.SpawnToBench(Data.unitID, Data.starLevel);
        }
    }
}
