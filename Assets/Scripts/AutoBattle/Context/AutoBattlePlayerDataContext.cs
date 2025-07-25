using AutoBattle;
using System;

public class AutoBattlePlayerDataContext
{
    private const string PlayerDataFileName = "AutoBattlePlayerData";
    private AutoBattlePlayerData _playerData;
    private AutoBattlePlayerDataSaveLoad _autoBattlePlayerDataSaveLoad;

    public event Action<int> OnAddSoulCoin;
    public event Action<int> OnSpendSoulCoin;

    public AutoBattlePlayerDataContext()
    {
        _autoBattlePlayerDataSaveLoad = new AutoBattlePlayerDataSaveLoad();
        _playerData = _autoBattlePlayerDataSaveLoad.Load(PlayerDataFileName);
        if (_playerData == null)
        {
            _playerData = new AutoBattlePlayerData(1);
        }
    }

    public int AddSoulCoin(int amount)
    {
        _playerData.soulCoin += amount;
        OnAddSoulCoin?.Invoke(_playerData.soulCoin);
        return _playerData.soulCoin;
    }

    public bool SpendSoulCoin(int amount)
    {
        if (amount > _playerData.soulCoin)
            return false;

        _playerData.soulCoin = Math.Max(0, _playerData.soulCoin - amount);
        OnSpendSoulCoin?.Invoke(_playerData.soulCoin);
        return true;
    }

    public int GetSoulCoin()
    {
        return _playerData.soulCoin;
    }

    public void SaveAutoBattlePlayerData()
    {
        _autoBattlePlayerDataSaveLoad.Save(_playerData, PlayerDataFileName);
    }
}
