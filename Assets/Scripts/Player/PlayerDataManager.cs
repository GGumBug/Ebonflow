using UnityEngine;

public class PlayerDataManager : Singleton<PlayerDataManager>, IDonDestroy
{
    private PlayerSaveLoad _playerSaveLoad;

    public void Setup()
    {
        _playerSaveLoad = new PlayerSaveLoad();
    }
}
