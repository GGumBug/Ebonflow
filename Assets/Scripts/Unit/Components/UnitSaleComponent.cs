using AutoBattle;
using AutoBattle.Input;
using System;

public class UnitSaleComponent
{
    private int _sellValue;

    public event Action RequestReleaseAndPool;

    public UnitSaleComponent(int sellValue)
    {
        _sellValue = sellValue;
    }

    public void Sell()
    {
        AutoBattleDataManager.Instance.AutoBattlePlayerDataContext.AddSoulCoin(_sellValue);

        RequestReleaseAndPool?.Invoke();
    }
}
