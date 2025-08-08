using AutoBattle;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>플레이어 데이터 컨텍스트</summary>
public class AutoBattlePlayerDataContext : DataContext<AutoBattlePlayerData>
{
    public event Action OnAddSoulCoin;
    public event Action OnSpendSoulCoin;
    public event Action<int> OnStreakChanged;

    public event Action<PlayerUnitRecord> OnUnitAdded;
    public event Action<int> OnUnitRemoved; // instanceId
    public event Action<UnitPlacementRecord> OnPlacementChanged;

    public AutoBattlePlayerDataContext()
        : base(
            fileName: "PlayerData",
            serializer: new AutoBattlePlayerDataSaveLoad(),
            defaultFactory: () => new AutoBattlePlayerData(level: 1, soulCoin: 10)
        )
    { }

    public override void Load()
    {
        base.Load();

        if (Data.ownedUnits == null)
        {
            Data.ownedUnits = new();
            Data.placements = new();
        }
    }

    public int GetLevel() => Data.level;
    public int GetSoulCoin() => Data.soulCoin;
    public int GetWinLossStreak() => Data.winLossStreak;
    public bool CanBuy(int price) => Data.soulCoin >= price;
    public bool OwnsUnit(int instanceId) => Data.ownedUnits.Any(u => u.instanceId == instanceId);

    public IReadOnlyList<PlayerUnitRecord> OwnedUnits => Data.ownedUnits;
    public IReadOnlyList<UnitPlacementRecord> Placements => Data.placements;

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

    public void UpdateStreak(bool victory)
    {
        if (victory)
        {
            if (Data.winLossStreak >= 0) Data.winLossStreak++;
            else Data.winLossStreak = 1;
        }
        else
        {
            if (Data.winLossStreak <= 0) Data.winLossStreak--;
            else Data.winLossStreak = -1;
        }
        OnStreakChanged?.Invoke(Data.winLossStreak);
    }

    public int CreateUnit(int unitId, int starLevel)
    {
        int instanceId = Data.nextInstanceId++;
        var rec = new PlayerUnitRecord
        {
            instanceId = instanceId,
            unitId = unitId,
            starLevel = starLevel
        };
        Data.ownedUnits.Add(rec);
        OnUnitAdded?.Invoke(rec);
        return instanceId;
    }

    public bool RemoveUnit(int instanceId)
    {
        int removed = Data.ownedUnits.RemoveAll(u => u.instanceId == instanceId);
        if (removed > 0)
        {
            Data.placements.RemoveAll(p => p.instanceId == instanceId);
            OnUnitRemoved?.Invoke(instanceId);
            return true;
        }
        return false;
    }

    public void UpsertPlacement(int instanceId, GridType grid, int x, int y)
    {
        if (!OwnsUnit(instanceId))
            throw new InvalidOperationException($"소유하지 않은 유닛 instanceId={instanceId}");

        var idx = Data.placements.FindIndex(p => p.instanceId == instanceId);
        var rec = new UnitPlacementRecord { instanceId = instanceId, grid = grid, x = x, y = y };

        if (idx >= 0) Data.placements[idx] = rec;
        else Data.placements.Add(rec);

        OnPlacementChanged?.Invoke(rec);
    }

    public bool TryGetPlacement(int instanceId, out UnitPlacementRecord rec)
    {
        int idx = Data.placements.FindIndex(p => p.instanceId == instanceId);
        if (idx >= 0) { rec = Data.placements[idx]; return true; }
        rec = default;
        return false;
    }
}

