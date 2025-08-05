using System;
using AutoBattle;

/// <summary>플레이어 데이터 컨텍스트</summary>
public class AutoBattlePlayerDataContext : DataContext<AutoBattlePlayerData>
{
    public event Action OnAddSoulCoin;
    public event Action OnSpendSoulCoin;

    public AutoBattlePlayerDataContext()
        : base(
            fileName: "PlayerData",
            serializer: new AutoBattlePlayerDataSaveLoad(),
            defaultFactory: () => new AutoBattlePlayerData(level: 1, soulCoin: 10)
        )
    { }

    public int GetLevel() => Data.level;
    public int GetSoulCoin() => Data.soulCoin;
    public bool CanBuy(int price) => Data.soulCoin >= price;

    public int AddSoulCoin(int amount)
    {
        Data.soulCoin += amount;
        OnAddSoulCoin?.Invoke();
        return Data.soulCoin;
    }

    public bool SpendSoulCoin(int amount)
    {
        if (amount > Data.soulCoin) return false;
        Data.soulCoin -= amount;
        OnSpendSoulCoin?.Invoke();
        return true;
    }
}

