using DeckSystem;

public class RerollButton : BuyButtonBase
{
    private const int REROLL_PRICE = 2;

    public override void SetBuyButton(AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        base.SetBuyButton(autoBattlePlayerDataContext);

        price = REROLL_PRICE;
        CheckCanBuy();
    }

    public void SetEvent(CardDrawManager cardDrawManager)
    {
        btnBuy.onClick.AddListener(cardDrawManager.DrawFiveCard);
    }
}
