using AutoBattle;
using DeckSystem;
using System;

public class UnitSaleComponent
{
    private int _sellValue;
    CardDrawManager _cardDrawManager;

    public event Action RequestReleaseAndPool;

    public UnitSaleComponent(int sellValue)
    {
        _sellValue = sellValue;
        _cardDrawManager = CardDrawManager.Instance;
    }

    public void Sell(CardData cardData)
    {
        _cardDrawManager.ReturnCardToDeck(cardData);

        AutoBattleDataManager.Instance.AutoBattlePlayerDataContext.AddSoulCoin(_sellValue);

        RequestReleaseAndPool?.Invoke();
    }
}
