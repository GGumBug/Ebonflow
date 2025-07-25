using AutoBattle;
using DeckSystem;
using System;
using TMPro;
using UnityEngine;

public class CardView : BuyButtonBase
{
    
    [SerializeField] private TextMeshProUGUI txtUnitTier;
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private TextMeshProUGUI txtUnitID;

    private AutoBattleUnitManager _autoBattleUnitManager;

    public int Index { get; private set; }
    public CardData Data { get; private set; }

    public event Action<int> RequestNewCardData;

    private void Awake()
    {
        btnBuy.onClick.AddListener(Buy);
        _autoBattleUnitManager = AutoBattleUnitManager.Instance;
    }

    public void SetData(CardData cardData)
    {
        Data = cardData;
        price = cardData.price;
        txtUnitTier.text = Data.tier.ToString();
        txtPrice.text = "Price" + Data.price.ToString();
        txtUnitID.text = "UnitID" + Data.unitID.ToString();
        CheckCanBuy();
    }

    public void SetCardView(int index, CardDrawManager cardDrawManager)
    {
        Index = index;
        RequestNewCardData += cardDrawManager.ResetIndexCard;
    }

    protected void Buy()
    {
        if (_autoBattleUnitManager.UnitBench.FirstEmptyIndex() >= 0 && CheckCanBuy())
        {
            _autoBattleUnitManager.SpawnToBench(Data.unitID, Data.starLevel);
            requestSpendSoulCoin.Invoke(price);
            RequestNewCardData?.Invoke(Index);
        }
        else
        {
            Debug.Log("Unit Bench 자리가 부족합니다.");
        }
    }
}
