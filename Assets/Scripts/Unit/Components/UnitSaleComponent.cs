using AutoBattle;
using DeckSystem;
using System;

public class UnitSaleComponent
{
    private int _sellValue;
    private CardDrawManager _cardDrawManager;
    private UnitDragController _unitDragController;

    public event Func<CardData> RequestCardData;
    public event Action RequestReleaseAndPool;

    public UnitSaleComponent(int sellValue)
    {
        _sellValue = sellValue;
        _cardDrawManager = CardDrawManager.Instance;
        _unitDragController = AutoBattleUnitManager.Instance.DragController;
    }

    public void Sell()
    {
        CardData data = RequestCardData.Invoke();
        _cardDrawManager.ReturnCardToDeck(data);
        AutoBattleDataManager.Instance.AutoBattlePlayerDataContext.AddSoulCoin(data.price);
        _unitDragController.OnSellZoneHoverChanged(false);
        RequestReleaseAndPool?.Invoke();
    }
}
