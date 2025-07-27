using AutoBattle;

public class UnitSaleComponent
{
    private int _sellValue;

    public void Sell()
    {
        AutoBattleDataManager.Instance.AutoBattlePlayerDataContext.AddSoulCoin(_sellValue);

        // 4) 풀링 또는 삭제
        //Destroy(gameObject);
    }
}
