using AutoBattle;
using DeckSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BansheeGz.BGDatabase.BGJsonRepoModel;

public class CardView : MonoBehaviour
{
    [SerializeField] private Button btnBuy;
    [SerializeField] private TextMeshProUGUI txtUnitTier;
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private TextMeshProUGUI txtUnitID;

    private AutoBattleUnitManager _autoBattleUnitManager;

    public int Index { get; private set; }
    public CardData Data { get; private set; }

    public event Func<int, bool> RequestSpendSoulCoin;
    public event Action<int> RequestNewCardData;

    private void Awake()
    {
        btnBuy.onClick.AddListener(BuyCardUnit);
        _autoBattleUnitManager = AutoBattleUnitManager.Instance;
    }

    public void SetData(CardData cardData, bool canBuy)
    {
        btnBuy.interactable = canBuy;

        Data = cardData;
        txtUnitTier.text = Data.tier.ToString();
        txtPrice.text = "Price" + Data.price.ToString();
        txtUnitID.text = "UnitID" + Data.unitID.ToString();
    }

    public void SetCardView(int index, CardDrawManager cardDrawManager, AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        Index = index;
        RequestNewCardData += cardDrawManager.Reroll;
        RequestSpendSoulCoin += autoBattlePlayerDataContext.SpendSoulCoin;
    }

    public void CheckCanBuy(int soulCoin)
    {
        bool canBuy = Data.price <= soulCoin;
        btnBuy.interactable = canBuy;
    }

    private void BuyCardUnit()
    {
        if (_autoBattleUnitManager.UnitBench.FirstEmptyIndex() >= 0 && RequestSpendSoulCoin.Invoke(Data.price))
        {
            _autoBattleUnitManager.SpawnToBench(Data.unitID, Data.starLevel);
            RequestNewCardData?.Invoke(Index);
        }
    }
}
