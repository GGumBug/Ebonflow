using AutoBattle;
using System;

public class AutoBattlePlayerDataContext
{
    private const string PlayerDataFileName = "AutoBattlePlayerData";
    private AutoBattlePlayerData _playerData;
    private AutoBattlePlayerDataSaveLoad _autoBattlePlayerDataSaveLoad;

    public event Action OnAddSoulCoin;
    public event Action OnSpendSoulCoin;

    public int GetLevel() => _playerData.level;
    public int GetSoulCoin() => _playerData.soulCoin;
    public bool CanBuy(int price) => _playerData.soulCoin >= price;

    public AutoBattlePlayerDataContext()
    {
        _autoBattlePlayerDataSaveLoad = new AutoBattlePlayerDataSaveLoad();
        _playerData = _autoBattlePlayerDataSaveLoad.Load(PlayerDataFileName);
        if (_playerData == null)
        {
            _playerData = new AutoBattlePlayerData(1, 10);
        }
    }

    public int AddSoulCoin(int amount)
    {
        _playerData.soulCoin += amount;
        OnAddSoulCoin?.Invoke();
        return _playerData.soulCoin;
    }

    public bool SpendSoulCoin(int amount)
    {
        if (amount > _playerData.soulCoin)
            return false;

        _playerData.soulCoin = Math.Max(0, _playerData.soulCoin - amount);
        OnSpendSoulCoin?.Invoke();
        return true;
    }

    public void SaveAutoBattlePlayerData()
    {
        _autoBattlePlayerDataSaveLoad.Save(_playerData, PlayerDataFileName);
    }
}
