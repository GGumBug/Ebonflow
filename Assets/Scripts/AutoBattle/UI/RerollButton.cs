using UnityEngine;

public class RerollButton : BuyButtonBase
{
    private const int REROLL_PRICE = 2;

    public override void SetBuyButton(AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        base.SetBuyButton(autoBattlePlayerDataContext);

        price = REROLL_PRICE;
        CheckCanBuy();
    }
}
