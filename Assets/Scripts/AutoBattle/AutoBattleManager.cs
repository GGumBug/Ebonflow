using UnityEngine;

public class AutoBattleManager : Singleton<AutoBattleManager>
{
    private AutoBattleStateController _stateController;

    private void Awake()
    {
        _stateController = new AutoBattleStateController();
    }
}
