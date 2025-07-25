using UnityEngine;

public class LevelUpButton : BuyButtonBase
{
    private const int LEVEL_UP_PRICE = 4;

    public override void SetBuyButton(AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        base.SetBuyButton(autoBattlePlayerDataContext);

        price = LEVEL_UP_PRICE;
        CheckCanBuy();
    }
}
