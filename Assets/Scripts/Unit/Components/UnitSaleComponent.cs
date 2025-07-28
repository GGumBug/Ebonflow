using AutoBattle;
using DeckSystem;
using System;

public class UnitSaleComponent
{
    private int _sellValue;
    CardDrawManager _cardDrawManager;

    public event Func<CardData> RequestCardData;
    public event Action RequestReleaseAndPool;

    public UnitSaleComponent(int sellValue)
    {
        _sellValue = sellValue;
        _cardDrawManager = CardDrawManager.Instance;
    }

    public void Sell()
    {
        CardData data = RequestCardData.Invoke();
        _cardDrawManager.ReturnCardToDeck(data);
        AutoBattleDataManager.Instance.AutoBattlePlayerDataContext.AddSoulCoin(data.price);
        RequestReleaseAndPool?.Invoke();
    }
}
