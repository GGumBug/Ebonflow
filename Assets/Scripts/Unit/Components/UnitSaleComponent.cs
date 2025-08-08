using AutoBattle;
using DeckSystem;
using System;
using UnityEngine;

public class UnitSaleComponent
{
    private int _sellValue;
    private AutoBattleDataManager _autoBattleDataManager;
    private CardDrawManager _cardDrawManager;
    private UnitDragController _unitDragController;

    public event Func<CardData> RequestCardData;
    public event Func<int> RequestInstanceID;
    public event Action RequestReleaseAndPool;

    public UnitSaleComponent(int sellValue, Func<int> requestInstanceID)
    {
        _sellValue = sellValue;
        RequestInstanceID += requestInstanceID;
        _cardDrawManager = CardDrawManager.Instance;
        _unitDragController = AutoBattleUnitManager.Instance.DragController;
        _autoBattleDataManager = AutoBattleDataManager.Instance;
    }

    public void Sell()
    {
        CardData data = RequestCardData.Invoke();
        _cardDrawManager.ReturnCardToDeck(data);
        _autoBattleDataManager.AutoBattlePlayerDataContext.AddSoulCoin(data.price);
        _unitDragController.OnSellZoneHoverChanged(false);

        int instanceId = RequestInstanceID.Invoke();
        if (instanceId != -1)
            _autoBattleDataManager.AutoBattlePlayerDataContext.RemoveUnit(instanceId);
        else
            Debug.LogError($"instanceId is -1");

        RequestReleaseAndPool?.Invoke();
    }
}
