using UnityEngine;

public class LevelUpButton : BuyButtonBase
{
    private const int LEVEL_UP_PRICE = 4;

    protected override void Awake()
    {
        base.Awake();

    }

    public override void SetBuyButton(AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        base.SetBuyButton(autoBattlePlayerDataContext);

        price = LEVEL_UP_PRICE;
        CheckCanBuy();
    }
}
